using System.Collections.Generic;
using System.IO;
using System;

namespace GZipTest.Buffering
{
    class BuffManager 
    {
        public const int ChunkSize = 1024 * 8;

        SeqBuf _seqBuf = new SeqBuf();

        ParallelBufQueue _compressedBuffers;
        ParallelBufQueue _decompressedBuffers;

        Queue<long> _sequencePositions = new Queue<long>();
        Queue<MemoryStream> _releasedBuffer = new Queue<MemoryStream>();


        public int WorkBuffersCount { get { return 0; } }
        public int ReleasedBuffersCount { get { return _releasedBuffer.Count; } }

        public Func<bool> SuspendAction = () => { return false; };

        public byte StreamsCount { get; set; }

        public BuffManager(int streamsNumber)
        {
            _compressedBuffers = new ParallelBufQueue(streamsNumber);
            _decompressedBuffers = new ParallelBufQueue(streamsNumber);
        }

        public void AddSequencePos(long position)
        {
            lock (_sequencePositions)
            {
                _sequencePositions.Enqueue(position);
            }
        }

        public long GetSequencePos()
        {
            lock (_sequencePositions)
            {
                if (_sequencePositions.Count > 0)
                    return _sequencePositions.Peek();
            }

            return -1;
        }

        public bool AtEndOfSequence()
        {
            lock (_sequencePositions)
            {
                return _sequencePositions.Count == 1;
            }
        }

        public void NextSequencePos()
        {
            lock (_sequencePositions)
            {
                if (_sequencePositions.Count > 0)
                    _sequencePositions.Dequeue();
            }
        }

        public void WriteSequenceBuf(MemoryStream memBytes, long position)
        {
            _seqBuf.Write(memBytes, position);
            
        }

        public MemoryStream ReadSequenceBuf(out long position)
        {
            return _seqBuf.Read(out position);
        }

        public void WriteCompressedBuffer(MemoryStream memBytes, long position, byte streamId)
        {
            _compressedBuffers.Enqueue(memBytes, position, streamId);
        }

        public void WriteDecompressedBuffer(MemoryStream memBytes, long position, byte streamId)
        {
            _decompressedBuffers.Enqueue(memBytes, position, streamId);
        }

        public MemoryStream ReadDecompressedBuffer(long position)
        {
            return _decompressedBuffers.Dequeue(position);
        }

        public MemoryStream ReadCompressedBuffer(long position, out byte streamId)
        {
            return _compressedBuffers.Dequeue(position, out streamId);
        }
        

        public MemoryStream ReadCompressedBufferForStream(out long position, byte streamId)
        {
            var memBytes =  ReadParallelBufferForStream(_compressedBuffers, out position, streamId);
            //AddSequencePos(position);
            return memBytes;
        }

        public MemoryStream ReadDeCompressedBufferForStream(out long position, byte streamId)
        {
            return ReadParallelBufferForStream(_decompressedBuffers, out position, streamId);
        }

        private MemoryStream ReadParallelBufferForStream(ParallelBufQueue buffers, out long position, byte streamId)
        {

            MemoryStream memBytes = null;

            do
            {
                memBytes = buffers.Dequeue(out position, streamId);
            }
            while (memBytes == null &&
                SuspendAction()); //suspend if buffer is empty

            return memBytes;
        }

        public void ReleaseMem(MemoryStream memBytes)
        {
            lock (_releasedBuffer)
            {
                memBytes.Position = 0;
                _releasedBuffer.Enqueue(memBytes);
            }
        }

        public MemoryStream GetFreeMem()
        {
            lock (_releasedBuffer)
            {
                if (_releasedBuffer.Count > 0)
                    return _releasedBuffer.Dequeue();
            }

            return new MemoryStream(ChunkSize);
        }

    }
}

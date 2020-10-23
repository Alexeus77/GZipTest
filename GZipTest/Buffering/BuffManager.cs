using System.Collections.Generic;
using System.IO;
using System;
using static GZipTest.DebugDiagnostics;

namespace GZipTest.Buffering
{
    class BuffManager
    {
        //chunk size currently cannot be large because compressed chunk length should be ushort size
        public const int ChunkSize = 1024 * 8;

        //suspend for a while if buffer size is over this limit
        private const int SeqBufferUpLimit = 2000;
        private const int ParallelBufferUpLimit = 3000;

        //suspend timing
        private const int suspendSeqBuffer = 100;
        
        //sequential buffer for read data from input stream
        SeqBuf _seqBuf = new SeqBuf();

        //here we store compressed data both then compressing and reading compressed stream
        ParallelBufQueue _compressedBuffers;

        //here we store decompressed data on decompress process only
        ParallelBufQueue _decompressedBuffers;

        //ordering positions
        Queue<uint> _sequencePositions = new Queue<uint>();
        Queue<MemoryStream> _releasedBuffer = new Queue<MemoryStream>();


        public readonly bool NeedSuspendBuffers = false;

        public int CompressedBuffersCount()
        {
            return _compressedBuffers.MemBuffersCount;
         }

        public int DeCompressedBuffersCount()
        {
            return _decompressedBuffers.MemBuffersCount;
        }

        public int SeqBuffersCount() { return _seqBuf.BufCount; }
        
        public int ReleasedBuffersCount() { return _releasedBuffer.Count; }

        public Func<int, bool> SuspendAction = (i) => { return false; };
        public int StreamsNumber { get; private set; }


        public BuffManager(int streamsNumber)
        {
            _compressedBuffers = new ParallelBufQueue(streamsNumber);
            _decompressedBuffers = new ParallelBufQueue(streamsNumber);
            StreamsNumber = streamsNumber;
        }

        public void AddSequencePos(uint position)
        {
            lock (_sequencePositions)
            {
                _sequencePositions.Enqueue(position);
            }
        }

        public uint PeekSequencePos()
        {
            lock (_sequencePositions)
            {
                if (_sequencePositions.Count > 0)
                    return _sequencePositions.Peek();
            }

            return UInt32.MaxValue;
        }

        public bool AtEndOfSequence()
        {
            lock (_sequencePositions)
            {
                return _sequencePositions.Count == 1;
            }
        }

        public uint GetNextSequencePos()
        {
            lock (_sequencePositions)
            {
                if (_sequencePositions.Count > 1)
                {
                    _sequencePositions.Dequeue();
                    return _sequencePositions.Peek();
                }
            }

            return UInt32.MaxValue;
        }

        
        public void WriteSequenceBuf(MemoryStream memBytes, uint position)
        {
            _seqBuf.Write(memBytes, position);

            if (NeedSuspendBuffers && _seqBuf.BufCount > SeqBufferUpLimit)
                SuspendAction(suspendSeqBuffer);
            
        }

        public MemoryStream ReadSequenceBuf(out uint position)
        {
            return _seqBuf.Read(out position);
        }

        public void WriteCompressedBuffer(MemoryStream memBytes, long position, byte streamId)
        {
            _compressedBuffers.Enqueue(memBytes, position, streamId);

            if (NeedSuspendBuffers)
                SuspendParallelBufferInc(_compressedBuffers);
        }

        public void WriteDecompressedBuffer(MemoryStream memBytes, long position, byte streamId)
        {
            _decompressedBuffers.Enqueue(memBytes, position, streamId);

            if (NeedSuspendBuffers)
                SuspendParallelBufferInc(_decompressedBuffers);
        }

        private void SuspendParallelBufferInc(ParallelBufQueue buff)
        {
            if (buff.MemBuffersCount > ParallelBufferUpLimit)
                SuspendAction(suspendSeqBuffer);
        }

        public MemoryStream ReadDecompressedBuffer(long position)
        {
            return _decompressedBuffers.Dequeue(position);
        }

        public MemoryStream ReadCompressedBuffer(long position, out byte streamId, bool getTail)
        {
            return _compressedBuffers.Dequeue(position, out streamId, getTail);
        }


        
        public MemoryStream ReadCompressedBufferForStream(out long position, byte streamId)
        {
            return ReadParallelBufferForStream(_compressedBuffers, out position, streamId);
        }

        public MemoryStream ReadDeCompressedBufferForStream(out long position, byte streamId)
        {
            return ReadParallelBufferForStream(_decompressedBuffers, out position, streamId);
        }

        private MemoryStream ReadParallelBufferForStream(ParallelBufQueue buffers, out long position, byte streamId)
        {

            MemoryStream memBytes;

            do
            {
                memBytes = buffers.Dequeue(out position, streamId);
            }
            while (memBytes == null &&
                SuspendAction(1)); //suspend if buffer is empty. 
                                   //we need to wait for data then reading through gzipstream

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

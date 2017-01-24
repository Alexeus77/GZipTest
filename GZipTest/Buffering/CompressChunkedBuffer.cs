using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Diagnostics.Debug;


namespace GZipTest.Buffering
{
    class CompressChunkedBuffer
    {
        const int chunkSize = 1024 * 8;

        public class CompressBuf
        {
            Queue<MemoryStream> _compressBuffer = new Queue<MemoryStream>();
            Queue<long> _compressPositions = new Queue<long>();

            public void Enqueue(MemoryStream memBytes, long position)
            {
                lock (_compressBuffer)
                {
                    _compressBuffer.Enqueue(memBytes);
                    _compressPositions.Enqueue(position);
                }
            }

            public MemoryStream Dequeue()
            {
                if (_compressBuffer.Count > 0)
                {
                    lock (_compressBuffer)
                    {
                        _compressPositions.Dequeue();
                        return _compressBuffer.Dequeue();
                    }

                }

                return null;
            }

            public MemoryStream Dequeue(long position)
            {
                if (_compressBuffer.Count > 0)
                {
                    if (_compressPositions.Peek() == position)
                    {
                        lock (_compressBuffer)
                        {
                            _compressPositions.Dequeue();
                            return _compressBuffer.Dequeue();
                        }
                    }
                }

                return null;
            }

            public MemoryStream Dequeue(out long position)
            {
                if (_compressBuffer.Count > 0)
                {
                    lock (_compressBuffer)
                    {
                        position = _compressPositions.Dequeue();
                        return _compressBuffer.Dequeue();
                    }

                }
                position = 0;
                return null;
            }
        }

        public class CompressBufQueue
        {
            CompressBuf[] _compressBuffers;

            public CompressBufQueue(int streamsNumber)
            {
                _compressBuffers = new CompressBuf[streamsNumber];
            }

            public void Enqueue(MemoryStream memBytes, long position, byte streamId)
            {
                _compressBuffers[streamId].Enqueue(memBytes, position);
            }

            public MemoryStream Dequeue(long position)
            {
                for (int i = 0; i < _compressBuffers.Length; i++)
                {
                    var memoryBytes = _compressBuffers[i].Dequeue(position);
                    if (memoryBytes != null)
                        return memoryBytes;
                }
                return null;
            }

            public MemoryStream Dequeue(out long position, int streamId)
            {
                return _compressBuffers[streamId].Dequeue(out position);
            }

            public MemoryStream Dequeue()
            {
                for (int i = 0; i < _compressBuffers.Length; i++)
                {
                    var memoryBytes = _compressBuffers[i].Dequeue();
                    if (memoryBytes != null)
                        return memoryBytes;
                }
                return null;
            }
        }


        Queue<long> _positions1 = new Queue<long>();
        Queue<long> _positions2 = new Queue<long>();
        Queue<MemoryStream> _readToBuf = new Queue<MemoryStream>();

        CompressBufQueue _compressBuffers;

        Queue<MemoryStream> _releasedBuffer = new Queue<MemoryStream>();


        public int WorkBuffersCount { get { return 0; } }
        public int ReleasedBuffersCount { get { return _releasedBuffer.Count; } }

        public byte StreamsCount { get; set; }

        public CompressChunkedBuffer(int streamsNumber)
        {
            _compressBuffers = new CompressBufQueue(streamsNumber);
        }

        public void WriteBuf(MemoryStream memBytes, long position)
        {
            lock (_readToBuf)
            {
                _readToBuf.Enqueue(memBytes);
                _positions1.Enqueue(position);
                _positions2.Enqueue(position);
            }
        }

        public MemoryStream ReadBuf(out long position)
        {
            if (_positions1.Count > 0)
            {
                lock (_readToBuf)
                {
                    position = _positions1.Dequeue();
                    return _readToBuf.Dequeue();
                }
            }

            position = 0;
            return null;
        }


        public void ToCompressBuffers(MemoryStream memBytes, long position, byte streamId)
        {
            _compressBuffers.Enqueue(memBytes, position, streamId);
        }

        public long GetPosition()
        {
            lock (_positions2)
            {
                if (_positions2.Count > 0)
                    return _positions2.Dequeue();
            }

            return -1;
        }

        public MemoryStream FromCompressBuffers(long position)
        {
            return _compressBuffers.Dequeue(position);
        }

        public MemoryStream FromCompressBuffers(out long position, byte streamId)
        {
            return _compressBuffers.Dequeue(out position, streamId);
        }

        public void Release(MemoryStream memBytes)
        {
            lock (_releasedBuffer)
            {
                memBytes.Position = 0;
                _releasedBuffer.Enqueue(memBytes);
            }
        }

        public MemoryStream GetFree()
        {
            lock (_releasedBuffer)
            {
                if (_releasedBuffer.Count > 0)
                    return _releasedBuffer.Dequeue();
            }

            return new MemoryStream(chunkSize);
        }


    }
}

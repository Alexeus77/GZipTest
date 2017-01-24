using System.IO;

namespace GZipTest.Buffering
{

    class ParallelBufQueue
    {
        ParallelBuf[] _compressBuffers;

        public ParallelBufQueue(int streamsNumber)
        {
            _compressBuffers = new ParallelBuf[streamsNumber];

            for (int i = 0; i < streamsNumber; i++)
            {
                _compressBuffers[i] = new ParallelBuf();
            }
        }

        public void Enqueue(MemoryStream memBytes, long position, byte streamId)
        {
            _compressBuffers[streamId].Enqueue(memBytes, position);
        }

        public MemoryStream Dequeue(byte streamId)
        {
            return _compressBuffers[streamId].Dequeue();
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

        public MemoryStream Dequeue(long position, out byte streamId)
        {
            for (byte i = 0; i < _compressBuffers.Length; i++)
            {
                var memoryBytes = _compressBuffers[i].Dequeue(position);
                if (memoryBytes != null)
                {
                    streamId = i;
                    return memoryBytes;
                }
            }

            streamId = 0;
            return null;
        }

        public MemoryStream Dequeue(out long position, out byte streamId)
        {
            for (byte i = 0; i < _compressBuffers.Length; i++)
            {
                var memoryBytes = _compressBuffers[i].Dequeue(out position);
                if (memoryBytes != null)
                {
                    streamId = i;
                    return memoryBytes;
                }
            }
            position = 0;
            streamId = 0;
            return null;
        }

        public MemoryStream Dequeue(out long position, byte streamId)
        {
            return _compressBuffers[streamId].Dequeue(out position);
        }
    }
}
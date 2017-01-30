using System.IO;

namespace GZipTest.Buffering
{

    class ParallelBufQueue
    {
        ParallelBuf[] _parallelBuffers;

        int _memBuffersCount = 0;

        public int GetMemBuffersCount()
        {
            return _memBuffersCount;
        }


        public ParallelBufQueue(int streamsNumber)
        {
            _parallelBuffers = new ParallelBuf[streamsNumber];

            for (int i = 0; i < streamsNumber; i++)
            {
                _parallelBuffers[i] = new ParallelBuf();
            }
        }

        public void Enqueue(MemoryStream memBytes, long position, byte streamId)
        {
            _parallelBuffers[streamId].Enqueue(memBytes, position);
            System.Threading.Interlocked.Increment(ref _memBuffersCount);
        }

        public MemoryStream Dequeue(byte streamId)
        {
            var memoryBytes = _parallelBuffers[streamId].Dequeue();
            if (memoryBytes != null)
                System.Threading.Interlocked.Decrement(ref _memBuffersCount);
            return memoryBytes;

        }

        public MemoryStream Dequeue(long position)
        {
            for (int i = 0; i < _parallelBuffers.Length; i++)
            {
                var memoryBytes = _parallelBuffers[i].Dequeue(position);
                if (memoryBytes != null)
                {
                    System.Threading.Interlocked.Decrement(ref _memBuffersCount);
                    return memoryBytes;
                }
            }
            return null;
        }

        public MemoryStream Dequeue(long position, out byte streamId, bool getTail)
        {
            for (byte i = 0; i < _parallelBuffers.Length; i++)
            {
                if (_parallelBuffers[i].Count > 1 || getTail)
                {
                    var memoryBytes = _parallelBuffers[i].Dequeue(position);
                    if (memoryBytes != null)
                    {
                        System.Threading.Interlocked.Decrement(ref _memBuffersCount);
                        streamId = i;
                        return memoryBytes;
                    }
                }
            }

            streamId = 0;
            return null;
        }

        public MemoryStream Dequeue(out long position, out byte streamId, bool getTail)
        {
            for (byte i = 0; i < _parallelBuffers.Length; i++)
            {
                if (_parallelBuffers[i].Count > 1 || getTail)
                {
                    var memoryBytes = _parallelBuffers[i].Dequeue(out position);
                    if (memoryBytes != null)
                    {
                        System.Threading.Interlocked.Decrement(ref _memBuffersCount);
                        streamId = i;
                        return memoryBytes;
                    }
                }
            }
            position = 0;
            streamId = 0;
            return null;
        }

        public MemoryStream Dequeue(out long position, byte streamId)
        {
            var memoryBytes = _parallelBuffers[streamId].Dequeue(out position);
            if (memoryBytes != null)
                System.Threading.Interlocked.Decrement(ref _memBuffersCount);

            return memoryBytes;
        }
    }
}
using System.Collections.Generic;
using System.IO;

namespace GZipTest.Buffering
{
    public class ChunkedMemBuffer : IChunkedMemBuffer
    {
        Queue<MemoryStream> _workBuffer = new Queue<MemoryStream>();
        Queue<MemoryStream> _releasedBuffer = new Queue<MemoryStream>();

        object _lockRelease = new object();
        object _lockWork = new object();

        public int WorkBuffersCount { get { return _workBuffer.Count; } }
        public int ReleasedBuffersCount { get { return _releasedBuffer.Count; } }

        public bool IsNotBalanced()
        {
            return WorkBuffersCount > 1000;
        }

        public void Write(MemoryStream memBytes)
        {
            lock (_lockWork)
            {
                memBytes.Position = 0;
                _workBuffer.Enqueue(memBytes);
            }

            //suspend thread if buffer overwritten to allow other thread in the pipeline to free the buffer
            if (IsNotBalanced())
                System.Threading.Thread.Sleep(50);
        }

        public MemoryStream Read()
        {
            lock (_lockWork)
            {
                if (_workBuffer.Count > 0)
                    return _workBuffer.Dequeue();
            }
            return null;
        }

        public void Release(MemoryStream memBytes)
        {
            lock (_lockRelease)
            {
                memBytes.Position = 0;
                _releasedBuffer.Enqueue(memBytes);
            }

        }

        public MemoryStream GetFree()
        {
            lock (_lockRelease)
            {
                if (_releasedBuffer.Count > 0)
                    return _releasedBuffer.Dequeue();
            }

            return new MemoryStream(8 * 1024);
        }

        public long WorkSize()
        {
            lock (_workBuffer)
            {
                long size = 0;
                foreach (var memStream in _workBuffer)
                    size += memStream.Length;

                return size;
            }
        }
    }
}

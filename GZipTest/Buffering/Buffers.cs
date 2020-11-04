using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Buffering
{
    class Buffers
    {
        private readonly Queue<MemoryStream> buffers = new Queue<MemoryStream>();
        private readonly Queue<MemoryStream> releasedBuffers = new Queue<MemoryStream>();
        private readonly int buffersLimit;
        private readonly int releasedBuffersLimit;
        private readonly AutoResetEvent eventBufferDequeue = new AutoResetEvent(true);
        private readonly AutoResetEvent eventBufferQueue = new AutoResetEvent(true);

        private int bufferLimitHitCount = 0;

        public int BufferCapacity { get; private set; }

        public Buffers(int bufferCapacity, int buffersLimit = 100, int releasedBuffersLimit = 100)
        {
            this.BufferCapacity = bufferCapacity;
            this.buffersLimit = buffersLimit;
            this.releasedBuffersLimit = releasedBuffersLimit;
        }

        public int BuffersCount
        {
            get
            {
                lock (buffers)
                {
                    return buffers.Count;
                }
            }
        }

        public int ReleasedCount
        {
            get
            {
                lock (buffers)
                {
                    return releasedBuffers.Count;
                }
            }
        }

        public MemoryStream GetBuffer()
        {
            //eventBufferQueue.WaitOne(100);
            return DequeueBuffer();
        }

        public void ReleaseBuffer(MemoryStream bufferStream)
        {
            lock (releasedBuffers)
            {
                if (releasedBuffers.Count <= releasedBuffersLimit)
                    releasedBuffers.Enqueue(bufferStream);
            }
        }

        public void EnqueueBuffer(MemoryStream bufferStream)
        {
            lock (buffers)
            {
                if (buffers.Count > buffersLimit)
                    eventBufferDequeue.WaitOne(500);
                
                buffers.Enqueue(bufferStream);
                eventBufferQueue.Set();
            }
        }

        private MemoryStream DequeueBuffer()
        {
            eventBufferDequeue.Set();

            lock (buffers)
            {
                return buffers.Count == 0 ? null : buffers.Dequeue();
            }
        }

        public MemoryStream GetMemory()
        {
            MemoryStream memory;

            lock (releasedBuffers)
            {

                memory = releasedBuffers.Count > 0 ?
                    releasedBuffers.Dequeue() :
                    new MemoryStream(BufferCapacity);

                memory.Position = 0;
                return memory;
            }
        }
    }
}


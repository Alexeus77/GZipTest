using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Diagnostics.Debug;

namespace GZipTest.Buffering
{
    class ChunkedMemBuffer : IChunkedMemBuffer
    {
        const int chunkSize = 1024 * 8;

        public class StreamThread
        {
            public byte Id { get; set; }
            public MemoryStream MemoryStream { get; set; }
        }

        Queue<long>[] streamPos = new Queue<long>[2];
        
        SortedDictionary<long, StreamThread> _workBuffer = new SortedDictionary<long, StreamThread>();
        Queue<MemoryStream> _releasedBuffer = new Queue<MemoryStream>();

        object _lockRelease = new object();
        object _lockWork = new object();

        public int WorkBuffersCount { get { return _workBuffer.Count; } }
        public int ReleasedBuffersCount { get { return _releasedBuffer.Count; } }

        public byte StreamsCount { get; set; }

        public ChunkedMemBuffer()
        {
            streamPos[0] = new Queue<long>();
            streamPos[1] = new Queue<long>();
        }

        bool IsNotBalanced()
        {
            return WorkBuffersCount > 1000;
        }

        //to write buffer in single thread
        public void Write(MemoryStream memBytes, long position)
        {
            Write(memBytes, position, 0);
        }

        public void Write(MemoryStream memBytes, long position, byte streamId)
        {
            lock (_lockWork)
            {

                if (_workBuffer.ContainsKey(position))
                {
                    var existingBytes = _workBuffer[position].MemoryStream;
                    memBytes.WriteTo(existingBytes);
                }
                else
                {
                    streamPos[streamId].Enqueue(position);

                    _workBuffer.Add(position, new StreamThread
                    { MemoryStream = memBytes, Id = streamId });
                }
            }

            //suspend thread if buffer overwritten to allow other thread in the pipeline to free the buffer
            if (IsNotBalanced())
                System.Threading.Thread.Sleep(50);
        }

        //this method is for reading buffer in position sorted order regadless stream number
        public MemoryStream Read()
        {
            long position;
            return Read(out position);
        }

        public MemoryStream Read(out long position)
        {
            byte streamNum;
            return Read(out position, out streamNum);
        }

        public MemoryStream Read(out long position, out byte streamId)
        {
            lock (_lockWork)
            {
                if (_workBuffer.Count > 0)
                {
                    var memBytes = _workBuffer.First().Value.MemoryStream;
                    position = _workBuffer.First().Key;
                    streamId = _workBuffer.First().Value.Id;
                    
                    streamPos[streamId].Dequeue();
                    
                    _workBuffer.Remove(position);

                    memBytes.Position = 0;
                    return memBytes;
                }
            }
            streamId = 0;
            position = 0;

            return null;
        }

        public MemoryStream ReadForStream(out long position, byte streamId)
        {
            lock (_lockWork)
            {
                if (_workBuffer.Count > 0)
                {
                    if (streamPos[streamId].Count > 0)
                    {
                        position = streamPos[streamId].Dequeue();

                        var memBytes = _workBuffer[position].MemoryStream;
                        _workBuffer.Remove(position);

                        memBytes.Position = 0;
                        return memBytes;
                    }

                    //var getFirstBuffForStream = _workBuffer.Where(w => w.Value.Id == streamId).
                    //    Take(1);

                    //if (getFirstBuffForStream.Count() > 0)
                    //{
                    //    position = getFirstBuffForStream.First().Key;

                    //    var memBytes = _workBuffer[position].MemoryStream;

                    //    _workBuffer.Remove(position);

                    //    memBytes.Position = 0;
                    //    return memBytes;
                    //}
                }
            }
            position = 0;
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

            return new MemoryStream(chunkSize);
        }

        public long WorkSize()
        {
            lock (_workBuffer)
            {
                long size = 0;
                foreach (var memStream in _workBuffer)
                    size += memStream.Value.MemoryStream.Length;

                return size;
            }
        }
    }
}

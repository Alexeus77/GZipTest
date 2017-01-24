using System.Collections.Generic;
using System.IO;

namespace GZipTest.Buffering
{

    class ParallelBuf
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
            lock (_compressBuffer)
            {
                if (_compressBuffer.Count > 0)
                {

                    var memBytes = _compressBuffer.Dequeue();
                    memBytes.Position = 0;

                    _compressPositions.Dequeue();
                    return memBytes;
                }

            }

            return null;
        }

        public MemoryStream Dequeue(long position)
        {
            lock (_compressBuffer)
            {
                if (_compressBuffer.Count > 0)
                {
                    if (_compressPositions.Peek() == position)
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
            lock (_compressBuffer)
            {
                if (_compressBuffer.Count > 0)
                {
                   
                    position = _compressPositions.Dequeue();
                    var memBytes = _compressBuffer.Dequeue();
                    memBytes.Position = 0;
                    return memBytes;
                }

            }
            position = 0;
            return null;
        }
    }
}
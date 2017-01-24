using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GZipTest.Buffering
{
    class SeqBuf
    {
        Queue<long> _positions = new Queue<long>();
        Queue<MemoryStream> _buf = new Queue<MemoryStream>();

        public void Write(MemoryStream memBytes, long position)
        {
            lock (_buf)
            {
                _buf.Enqueue(memBytes);
                _positions.Enqueue(position);
            }
        }

        public MemoryStream Read(out long position)
        {
            lock (_buf)
            {
                if (_positions.Count > 0)
                {

                    position = _positions.Dequeue();
                    return _buf.Dequeue();

                }
            }

            position = 0;
            return null;
        }

        public int BufCount { get { return _buf.Count; } }
    }
}

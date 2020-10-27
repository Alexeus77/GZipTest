using GZipTest.Streaming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.Buffering
{
    class ReadBufferedStream : Buffering.BuffStream
    {
        public const byte HeaderSize = sizeof(long);

        public ReadBufferedStream() : base(1024*1024)
        {

        }

        private ReadBufferedStream(int bufferCapacity)  : base(bufferCapacity)
        {
            
        }

        public void Write(MemoryStream buffer)
        {
            EnqueueBuffer(buffer);
        }

        public void Write(MemoryStream buffer, long position, int count)
        {
            WriteBuffer(buffer.GetBuffer(), position, count);

            ReleaseBuffer(buffer);
        }

        private void WriteBuffer(byte[] bufferChunck, long position, int count)
        {
            var bufferStream = GetMemory();
            bufferStream.WriteLong(position);

            bufferStream.Write(bufferChunck, 0, count);
            bufferStream.SetLength(count + sizeof(long));

            EnqueueBuffer(bufferStream);
        }
    }
}

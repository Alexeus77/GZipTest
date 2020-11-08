using System;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;

namespace GZipTest.Tests
{
    class EndlessStream : Stream
    {
        private int counter = 0;
        private long position = 0;
        private long length;

        private static Random random = new Random();

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position { get => position; set => throw new NotImplementedException(); }

        public EndlessStream(long length = long.MaxValue)
        {
            this.length = length;
        }

        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (counter > 10)
            {
                Debugger.Break();
                counter = 0;
            }

            counter++;

            //random.NextBytes(buffer);

            count = (int)Math.Min((long)count, length - position);

            position += count;

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}

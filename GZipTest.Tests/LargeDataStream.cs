using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GZipTest.Tests.Mocks
{
    internal class LargeDataStream : Stream
    {
        private long _lenght = 0;
        private long _position = 0;
        private MemoryStream _imitateData;

        public LargeDataStream(MemoryStream imitateData, long length)
        {
            _imitateData = imitateData;
            _lenght = Math.Max(length, imitateData.Length);
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return _lenght;
            }
        }

        public override long Position
        {
            get
            {
                return _position;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            int memOffset = 0;

            while (count > bytesRead)
            {
                if (_imitateData.Position == _imitateData.Length)
                    _imitateData.Position = 0;

                var bytesToRead = Math.Min(count - bytesRead, 
                    (int)(_imitateData.Length - _imitateData.Position));

                System.Buffer.BlockCopy(
                    _imitateData.GetBuffer(), memOffset, buffer, offset, bytesToRead);

                bytesRead += bytesToRead;
                
            }

            return count - bytesRead;
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

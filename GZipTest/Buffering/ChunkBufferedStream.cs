using System;
using System.IO;


namespace GZipTest.Buffering
{
    public class ChunkBufferedStream : Stream
    {
        private static ChunkedMemBuffer _chunkedMemBuffer = new ChunkedMemBuffer();
        private long _position = 0;
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public ChunkedMemBuffer ChunkedMemBuffer { get { return _chunkedMemBuffer; } }

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

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        #region not implemented

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException("CanSeek");
            }
        }
        
        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public override void Flush()
        {
            throw new NotImplementedException();
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        #endregion not implemented


        private MemoryStream _mem = null;
        private bool _restRead = false;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesToRead = count;
            int bufferOffset = 0;

            do
            {
                //dequeue memstream from chunked buffer or use previously saved memstream
                MemoryStream memBytes = _restRead ? _mem : _chunkedMemBuffer.Read();

                if (memBytes == null)
                    break;

                //min value among: 1)rest buffer 2)bytes count to read 3)rest bytes to read in current do while cycle
                bytesToRead = (int)
                    Math.Min(buffer.Length - offset - bufferOffset,
                    Math.Min(count, bytesToRead));

                //read to buffer to offset plus current cycle offset as much bytes as possible from current memstream
                int bytesRead = memBytes.Read(buffer, offset + bufferOffset, bytesToRead);

                //update counters with current number of bytes read
                bytesToRead -= bytesRead;
                bufferOffset += bytesRead;

                //check if any bytes rest in current memstream
                _restRead = memBytes.Length != memBytes.Position;

                //save current memstream to use on the next read or release it in chunked buffer
                if (_restRead)
                    _mem = memBytes;
                else
                    _chunkedMemBuffer.Release(memBytes);

            } while (bytesToRead > 0);

            //return actual count of bytes read by procedure
            return count - bytesToRead;
        }





        public override void Write(byte[] buffer, int offset, int count)
        {

            if (count == 0)
                return;
            
            //get memstream available for writing from chunked buffer (new or reused)
            MemoryStream memBytes = _chunkedMemBuffer.GetFree();

            //write memstream and its length
            memBytes.Write(buffer, offset, count);
            memBytes.SetLength(count);

            //enqueue memstream to chunked buffer
            _chunkedMemBuffer.Write(memBytes);

            _position += count;
            
        }
    }

}



using System;
using System.IO;
using System.Collections.Generic;
using static GZipTest.DebugDiagnostics;


namespace GZipTest.Buffering
{
    class BuffStream : Stream
    {
        private Queue<MemoryStream> buffers = new Queue<MemoryStream>();
        private Queue<MemoryStream> releasedBuffers = new Queue<MemoryStream>();

        private long dataLength = 0;

        public int BufferCapacity { get; private set; }

        public override long Length  => dataLength;
        public override long Position { get; set; }

        public BuffStream(int bufferCapacity)
        {
            this.BufferCapacity = bufferCapacity;
        }

        public override void SetLength(long value)
        {
            this.dataLength = value;
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
             

        public override void Flush()
        {
            throw new NotImplementedException();
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        

        #endregion not implemented


        public override int Read(byte[] buffer, int offset, int count)
        {
            //int bytesToRead = count;
            //int bufferOffset = 0;

            //do
            //{
            //    long position = 0;

            //    //dequeue memstream from chunked buffer or use previously saved memstream
            //    var memBytes = _restRead ? _memCurrent : _chunkedMemBuffer.ReadCompressedBufferForStream(out position, Id);

            //    if (memBytes == null)
            //        break;

            //    //store read position for proper ordering compressed blocks

            //    if (_positionCurrent != position && !_restRead)
            //    {
            //        ReadPositions.Enqueue(position);

            //        _positionCurrent = position;
            //    }

            //    //min value among: 1)rest buffer 2)bytes count to read 3)rest bytes to read in current do while cycle
            //    bytesToRead = (int)
            //        Math.Min(buffer.Length - offset - bufferOffset,
            //        Math.Min(count, bytesToRead));

            //    //read to buffer to offset plus current cycle offset as much bytes as possible from current memstream
            //    int bytesRead = memBytes.Read(buffer, offset + bufferOffset, bytesToRead);

            //    //update counters with current number of bytes read
            //    bytesToRead -= bytesRead;
            //    bufferOffset += bytesRead;

            //    WriteLine($"ZR::{Id} {position} {bytesRead}");

            //    //check if any bytes rest in current memstream
            //    _restRead = memBytes.Length != memBytes.Position;

            //    //save current memstream to use on the next read or release it in chunked buffer
            //    if (_restRead)
            //        _memCurrent = memBytes;
            //    else
            //        _chunkedMemBuffer.ReleaseMem(memBytes);

            //} while (bytesToRead > 0);


            //return actual count of bytes read by procedure


            //MemoryStream memoryStream = DequeueBuffer();

            return count;
        }


        public MemoryStream GetBuffer()
        {
            return DequeueBuffer();
        }

        

        public void ReleaseBuffer(MemoryStream bufferStream)
        {
            lock (releasedBuffers)
            {
                releasedBuffers.Enqueue(bufferStream);
            }
        }

        protected void EnqueueBuffer(MemoryStream bufferStream)
        {
            lock (buffers)
            {
                buffers.Enqueue(bufferStream);
            }
        }

        private MemoryStream DequeueBuffer()
        {
            lock (buffers)
            {
                return buffers.Count == 0 ? null : buffers.Dequeue();
            }
        }

        public MemoryStream GetMemory()
        {
            lock (releasedBuffers)
            {
                return releasedBuffers.Count > 0 ? 
                    releasedBuffers.Dequeue() : 
                    new MemoryStream(BufferCapacity);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }


}


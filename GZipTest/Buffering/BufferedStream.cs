using System;
using System.IO;
using System.Collections.Generic;
using static GZipTest.DebugDiagnostics;


namespace GZipTest.Buffering
{
    class BufferedStream : Stream
    {
        private BuffManager _chunkedMemBuffer;

        public byte Id { get; private set; }

        public BufferedStream(byte id, BuffManager chunkedMemBuffer)
        {
            Id = id;
            _chunkedMemBuffer = chunkedMemBuffer;
        }


        public override long Position { get; set; }

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


        private MemoryStream _memCurrent = null;
        private bool _restRead = false;
        private long _positionCurrent = -1;

        public Queue<long> ReadPositions { get; set; } = new Queue<long>();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesToRead = count;
            int bufferOffset = 0;

            do
            {
                long position = 0;

                //dequeue memstream from chunked buffer or use previously saved memstream
                var memBytes = _restRead ? _memCurrent : _chunkedMemBuffer.ReadCompressedBufferForStream(out position, Id);

                if (memBytes == null)
                    break;

                //store read position for proper ordering compressed blocks

                if (_positionCurrent != position && !_restRead)
                {
                    ReadPositions.Enqueue(position);

                    _positionCurrent = position;
                }

                //min value among: 1)rest buffer 2)bytes count to read 3)rest bytes to read in current do while cycle
                bytesToRead = (int)
                    Math.Min(buffer.Length - offset - bufferOffset,
                    Math.Min(count, bytesToRead));

                //read to buffer to offset plus current cycle offset as much bytes as possible from current memstream
                int bytesRead = memBytes.Read(buffer, offset + bufferOffset, bytesToRead);

                //update counters with current number of bytes read
                bytesToRead -= bytesRead;
                bufferOffset += bytesRead;

                WriteLine($"ZR::{Id} {position} {bytesRead}");

                //check if any bytes rest in current memstream
                _restRead = memBytes.Length != memBytes.Position;

                //save current memstream to use on the next read or release it in chunked buffer
                if (_restRead)
                    _memCurrent = memBytes;
                else
                    _chunkedMemBuffer.ReleaseMem(memBytes);

            } while (bytesToRead > 0);


            //return actual count of bytes read by procedure
            return count - bytesToRead;
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {

            if (count == 0)
                return;

            //get memstream available for writing from chunked buffer (new or reused)
            MemoryStream memBytes = _positionCurrent == Position && _memCurrent != null ?
                _memCurrent : _chunkedMemBuffer.GetFreeMem();

            WriteLine($"ZW::{Id} {Position} {count} {memBytes.Length}");
            
            //write memstream and its length
            memBytes.Write(buffer, offset, count);
            memBytes.SetLength(memBytes.Position);

            //enqueue memstream to chunked buffer with position ordering and stream number indication
            if (_positionCurrent != Position)
            {
                _chunkedMemBuffer.WriteCompressedBuffer(memBytes, Position, Id);
                _positionCurrent = Position;
                _memCurrent = memBytes;
            }
            else
                WriteLine2($"ZW::{Id} {Position} {count} {memBytes.Length}");
        }

    }


}



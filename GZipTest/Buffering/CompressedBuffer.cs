using System.IO;

namespace GZipTest.Buffering
{
    class CompressedBuffer : BuffStream
    {
        MemoryStream bufferStream = null;
        private long compressedLen = 0;

        public CompressedBuffer(byte Id) : base(Id)
        {

        }

        public override long Length => compressedLen;

        public void Begin()
        {
            //bufferStream = GetMemory(ReadBufferedStream.BufferCapacity + sizeof(byte));
            //bufferStream.WriteByte(Id);
            //compressedLen = 1;

            bufferStream = GetMemory();
        }

        public void End()
        {
            bufferStream.SetLength(Length);
            EnqueueBuffer(bufferStream);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            bufferStream.Write(buffer, 0, buffer.Length);

            compressedLen += count;
        }

        public void Write(MemoryStream buffer)
        {
            EnqueueBuffer(buffer);
        }

    }
}

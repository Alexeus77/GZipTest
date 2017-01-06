using System.IO;

namespace GZipTest.Buffering
{
    interface IChunkedMemBuffer
    {
        MemoryStream GetFree();
        MemoryStream Read();
        MemoryStream Read(out long position);
        MemoryStream Read(out long position, out byte streamNumber);
        MemoryStream ReadForStream(out long position, byte streamNumber);
        void Release(MemoryStream memBytes);
        void Write(MemoryStream memBytes, long Position);
        void Write(MemoryStream memBytes, long Position, byte streamNumber);
        byte StreamsCount { get; set; }

        long WorkSize();
    }
}
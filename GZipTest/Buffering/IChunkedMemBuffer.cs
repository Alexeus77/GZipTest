using System.IO;

namespace GZipTest.Buffering
{
    interface IChunkedMemBuffer
    {
        int ReleasedBuffersCount { get; }
        byte StreamsCount { get; set; }
        int WorkBuffersCount { get; }

        MemoryStream FromCompressBuffers(long position);
        MemoryStream FromCompressBuffers(out long position, out byte streamId);
        MemoryStream GetFreeMem();
        long GetPosition();
        MemoryStream ReadBuf(out long position);
        void ReleaseMem(MemoryStream memBytes);
        void ToCompressBuffers(MemoryStream memBytes, long position, byte streamId);
        void WriteBuf(MemoryStream memBytes, long position);
    }
}
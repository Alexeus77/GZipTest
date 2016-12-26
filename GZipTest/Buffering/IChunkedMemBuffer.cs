using System.IO;

namespace GZipTest.Buffering
{
    interface IChunkedMemBuffer
    {
        MemoryStream GetFree();
        MemoryStream Read();
        void Release(MemoryStream memBytes);
        void Write(MemoryStream memBytes);
        bool IsNotBalanced();

        long WorkSize();
    }
}
using System.IO;

namespace GZipTest.Compression
{
    public interface ICompressor
    {
        void Compress(Stream fromStream, Stream toStream);
        void Decompress(Stream fromStream, Stream toStream);
    }
}
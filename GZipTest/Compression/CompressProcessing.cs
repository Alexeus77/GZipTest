using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Tasks;


namespace GZipTest.Compression
{
    public static class Process
    {
        public static void Compress(Stream fromStream, Stream toStream)
        {
            CompressParallel(fromStream, toStream);
        }

        public static void Decompress(Stream fromStream, Stream toStream)
        {
            DeCompressParallel(fromStream, toStream);
        }

       
        private static void CompressParallel(Stream readStream, Stream writeStream)
        {
            IChunkedMemBuffer buff = new ChunkedMemBuffer();
            using (var chunkedStream = new ChunkBufferedStream())
            {
                using (var gzip = new GZipStream(chunkedStream, CompressionMode.Compress))
                {
                    Tasker tasker = new Tasker();
                    tasker.Run(CompressorProcedures.ReadFromStreamToBufferAll, readStream, buff, buff.IsNotBalanced).
                        ThenRunWithContinue(CompressorProcedures.CompressAllToChunkedStream, gzip, buff, null, () => { gzip.Close(); } ).
                        ThenRun(CompressorProcedures.WriteAllCompressedBufferToStream, writeStream,
                        chunkedStream.ChunkedMemBuffer, null).
                        Start().WaitAll();

                }
            }
        }

        private static void DeCompressParallel(Stream readStream, Stream writeStream)
        {
            IChunkedMemBuffer buff = new ChunkedMemBuffer();
            using (var chunkedStream = new ChunkBufferedStream())
            {
                using (var gzip = new GZipStream(chunkedStream, CompressionMode.Decompress))
                {
                    Tasker tasker = new Tasker();
                    tasker.Run(CompressorProcedures.ReadFromStreamToBufferAll, readStream,
                        chunkedStream.ChunkedMemBuffer, chunkedStream.ChunkedMemBuffer.IsNotBalanced).
                        ThenRunWithContinue(CompressorProcedures.ReadFromCompressedStreamToBufferAll, gzip, buff, buff.IsNotBalanced, () => { gzip.Close(); }).
                        ThenRun(CompressorProcedures.WriteFromBufferToStreamAll, writeStream, buff, null).
                        Start().WaitAll();
                }
            }
        }

    }

}

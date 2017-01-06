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
            var chunkedStream = new ChunkBufferedStream(0);
            var gzip = new GZipStream(chunkedStream, CompressionMode.Compress);

            IChunkedMemBuffer buff2 = new ChunkedMemBuffer();
            var chunkedStream2 = new ChunkBufferedStream(1);
            var gzip2 = new GZipStream(chunkedStream2, CompressionMode.Compress);

            Tasker tasker = new Tasker();
            tasker.Run(CompressorProcedures.ReadAllFromStreamToBuffer, readStream, buff).
                ThenRunWithContinue(CompressorProcedures.CompressAllToChunkedStream, gzip, buff, () => { gzip.Close(); }).
                ThenRunWithContinue(CompressorProcedures.CompressAllToChunkedStream, gzip2, buff, () => { gzip2.Close(); }).
                ThenRun(CompressorProcedures.WriteAllCompressedFromBufferToStream, writeStream,
                ChunkBufferedStream.ChunkedMemBuffer).
                Start().WaitAll();


        }

        private static void DeCompressParallel(Stream readStream, Stream writeStream)
        {
            IChunkedMemBuffer buff = new ChunkedMemBuffer();
            var chunkedStream = new ChunkBufferedStream(0);
            var gzip = new GZipStream(chunkedStream, CompressionMode.Decompress);

            IChunkedMemBuffer buff2 = new ChunkedMemBuffer();
            var chunkedStream2 = new ChunkBufferedStream(1);
            var gzip2 = new GZipStream(chunkedStream2, CompressionMode.Decompress);
            Tasker tasker = new Tasker();

            tasker.Run(CompressorProcedures.ReadAllFromCompressedStreamToBuffer, readStream, ChunkBufferedStream.ChunkedMemBuffer).
                ThenRunWithContinue(CompressorProcedures.ReadAllFromGZipStreamToBuffer, gzip, buff, () => { gzip.Close(); }).
                ThenRunWithContinue(CompressorProcedures.ReadAllFromGZipStreamToBuffer, gzip2, buff, () => { gzip2.Close(); }).
                ThenRun(CompressorProcedures.WriteAllFromBufferToStream, writeStream, buff).
                Start().WaitAll();

        }

    }

}

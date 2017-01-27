using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Tasks;
using static GZipTest.DebugDiagnostics;


namespace GZipTest.Compression
{
    public static class Process
    {
        const int parallelCompressions = 4;

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
            
            var buff = new BuffManager(parallelCompressions);
            var gZipStreams = GetGZipStreams(buff, parallelCompressions, CompressionMode.Compress);

            Tasker tasker = new Tasker();

            buff.SuspendAction = tasker.SuspendAction;

            try
            {

                tasker.Run(CompressorProcedures.ReadFromStreamToBuffer, readStream, buff).
                    ThenRunForEach(gZipStreams, CompressorProcedures.CompressBufferDataToStream, CloseGZip, buff.SuspendAction, buff).
                    ThenRunWithContinue(CompressorProcedures.WriteCompressedBufferToStream,
                        writeStream, buff, CompressorProcedures.WriteCompressedBufferTailToStream).
                Start().WaitAll();

                
                
            }
            catch
            {
                WriteLine($"{buff.CompressedBuffersCount()} : {buff.DeCompressedBuffersCount()} : " +
                    $"{buff.SeqBuffersCount()} : {buff.ReleasedBuffersCount()}");

                throw;
            }

            WriteLine($"{buff.CompressedBuffersCount()} : {buff.DeCompressedBuffersCount()} : " +
                    $"{buff.SeqBuffersCount()} : {buff.ReleasedBuffersCount()}");

        }

        private static void CloseGZip(GZipStream gzip)
        {
            gzip.Close();
        }

        private static void DeCompressParallel(Stream readStream, Stream writeStream)
        {

            var buff = new BuffManager(parallelCompressions);
            var gZipStreams = GetGZipStreams(buff, parallelCompressions, CompressionMode.Decompress);

            Tasker tasker = new Tasker();

            buff.SuspendAction = tasker.SuspendAction;

            tasker.Run(CompressorProcedures.ReadFromCompressedStreamToBuffer, readStream, buff).
                ThenRunForEach(gZipStreams, CompressorProcedures.DecompressFromStreamToBuffer, CloseGZip,
                buff.SuspendAction, buff).
                ThenRun(CompressorProcedures.WriteDecompressedToStream, writeStream, buff).
            Start().WaitAll();



        }

        private static GZipStream[] GetGZipStreams(BuffManager buffManager, int parallelCompressions, CompressionMode compressionMode)
        {
            GZipStream[] gzipStreams = new GZipStream[parallelCompressions];
            Buffering.BufferedStream[] chunkedStreams = new Buffering.BufferedStream[parallelCompressions];

            for (byte i = 0; i < parallelCompressions; i++)
            {
                chunkedStreams[i] = new Buffering.BufferedStream(i, buffManager);
                gzipStreams[i] = new GZipStream(chunkedStreams[i], compressionMode, true);
            }

            return gzipStreams;
        }

        private static void CompressSeq(Stream readStream, Stream writeStream)
        {
            var buff = new BuffManager(2);
            var chunkedStream = new Buffering.BufferedStream(0, buff);
            var gzip = new GZipStream(chunkedStream, CompressionMode.Compress);

            CompressorProcedures.ReadFromStreamToBuffer(readStream, buff);
            CompressorProcedures.CompressBufferDataToStream(gzip, buff);
            gzip.Close();
            CompressorProcedures.WriteCompressedBufferToStream(writeStream, buff);
            CompressorProcedures.WriteCompressedBufferTailToStream(writeStream, buff);

        }

        private static void DeCompressSeq(Stream readStream, Stream writeStream)
        {
            var buff = new BuffManager(2);
            var chunkedStream = new Buffering.BufferedStream(0, buff);
            var gzip = new GZipStream(chunkedStream, CompressionMode.Decompress);

            CompressorProcedures.ReadFromCompressedStreamToBuffer(readStream, buff);
            CompressorProcedures.DecompressFromStreamToBuffer(gzip, buff);
            gzip.Close();

            CompressorProcedures.WriteDecompressedToStream(writeStream, buff);

        }

    }

}



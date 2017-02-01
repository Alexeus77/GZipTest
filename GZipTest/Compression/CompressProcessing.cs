using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Tasks;
using static GZipTest.DebugDiagnostics;
using GZipTest.Streaming;


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

            byte parallelCompressions = (byte)(System.Environment.ProcessorCount);

            var buff = new BuffManager(parallelCompressions);
            var gZipStreams = GetGZipStreams(buff, CompressionMode.Compress);

            Tasker tasker = new Tasker();

            buff.SuspendAction = Tasker.SuspendAction;
            
            Streaming.StreamExt.WriteFileHeader(writeStream, parallelCompressions);

            tasker.Run(CompressorProcedures.ReadFromStreamToBuffer, readStream, buff).
                ThenQueueForEach(gZipStreams, CompressorProcedures.CompressBufferDataToStream, CloseGZip, buff.SuspendAction, buff).

            StartAsync().
            ThenRunWithContinueSync(
                    CompressorProcedures.WriteCompressedBufferToStream,
                    writeStream, buff,
                    CompressorProcedures.WriteCompressedBufferTailToStream).
            WaitAll();

            ConsoleWriteLine($"Used streams number: {parallelCompressions}");
            ConsoleWriteLine($"Used buffers: {buff.ReleasedBuffersCount()}");


            //WriteLine($"{buff.CompressedBuffersCount()} : {buff.DeCompressedBuffersCount()} : " +
            //        $"{buff.SeqBuffersCount()} : {buff.ReleasedBuffersCount()}");

        }

        private static void CloseGZip(GZipStream gzip)
        {
            gzip.Close();
        }

        private static void DeCompressParallel(Stream readStream, Stream writeStream)
        {
            byte streamsNumber;

            if (readStream.ReadFileHeader(out streamsNumber))
            {

                var buff = new BuffManager(streamsNumber);
                var gZipStreams = GetGZipStreams(buff, CompressionMode.Decompress);

                Tasker tasker = new Tasker();

                buff.SuspendAction = Tasker.SuspendAction;

                tasker.Run(CompressorProcedures.ReadFromCompressedStreamToBuffer, readStream, buff).
                    ThenQueueForEach(gZipStreams, CompressorProcedures.DecompressFromStreamToBuffer, CloseGZip,
                    buff.SuspendAction, buff).

                StartAsync().
                ThenRunWithContinueSync(CompressorProcedures.WriteDecompressedToStream,
                        writeStream, buff, null).
                WaitAll();

                ConsoleWriteLine($"Used streams number: {streamsNumber}");
                ConsoleWriteLine($"Used buffers: {buff.ReleasedBuffersCount()}");


            }
            else
            {
                throw new InvalidDataException("Input stream is of invalid data format.");
            }

        }

        private static GZipStream[] GetGZipStreams(BuffManager buffManager, CompressionMode compressionMode)
        {
            GZipStream[] gzipStreams = new GZipStream[buffManager.StreamsNumber];
            Buffering.BufferedStream[] chunkedStreams = new Buffering.BufferedStream[buffManager.StreamsNumber];

            for (byte i = 0; i < gzipStreams.Length; i++)
            {
                chunkedStreams[i] = new Buffering.BufferedStream(i, buffManager);
                gzipStreams[i] = new GZipStream(chunkedStreams[i], compressionMode, true);
            }

            return gzipStreams;
        }

        //private static void CompressSeq(Stream readStream, Stream writeStream)
        //{
        //    const int parallelCompressions = 2;

        //    var buff = new BuffManager(parallelCompressions);
        //    var chunkedStream = new Buffering.BufferedStream(0, buff);
        //    var gzipStreams = GetGZipStreams(buff, CompressionMode.Compress);

        //    CompressorProcedures.ReadFromStreamToBuffer(readStream, buff);
        //    foreach (var gzip in gzipStreams)
        //    {
        //        CompressorProcedures.CompressBufferDataToStream(gzip, buff);
        //        gzip.Close();
        //    }

        //    writeStream.WriteFileHeader(parallelCompressions);

        //    CompressorProcedures.WriteCompressedBufferToStream(writeStream, buff);
        //    CompressorProcedures.WriteCompressedBufferTailToStream(writeStream, buff);

        //}

        //private static void DeCompressSeq(Stream readStream, Stream writeStream)
        //{
        //    byte streamsNumber;

        //    if (readStream.ReadFileHeader(out streamsNumber))
        //    {
        //        var buff = new BuffManager(streamsNumber);
        //        var gzipStreams = GetGZipStreams(buff, CompressionMode.Decompress);

        //        CompressorProcedures.ReadFromCompressedStreamToBuffer(readStream, buff);
        //        foreach (var gzip in gzipStreams)
        //        {
        //            CompressorProcedures.DecompressFromStreamToBuffer(gzip, buff);
        //            gzip.Close();
        //        }

        //        CompressorProcedures.WriteDecompressedToStream(writeStream, buff);
        //    }

        //}

    }

}



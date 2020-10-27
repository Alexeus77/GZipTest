using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Tasks;
using static GZipTest.DebugDiagnostics;
using GZipTest.Streaming;
using System;
using System.Linq;

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

            byte parallelCompressions = 4; // (byte)(Math.Max(Environment.ProcessorCount, readStream.Length / 4E9 + 1));

            var bufStreams = Enumerable.Range(1, parallelCompressions).
                Select(i => new Buffering.ReadBufferedStream()).ToArray();

            ITasker tasker = new Tasker();
            
            var readBufStream = new ReadBufferedStream();

            //var gzs = new GZipStream[] { new GZipStream(bufStreams[0], CompressionMode.Compress) };

            tasker.Queue(CompressorProcedures.ReadFromStreamToBuffer, readStream, readBufStream).
                ThenQueueForEach(CompressorProcedures.CompressBufferDataToStream, bufStreams, readBufStream, null).
                Queue(CompressorProcedures.WriteCompressedBufferToStream, writeStream, bufStreams).
                StartAsync().
                WaitAll();

            ConsoleWriteLine($"Used streams number: {parallelCompressions}");
            
        }
                

        private static void DeCompressParallel(Stream readStream, Stream writeStream)
        {
            byte parallelCompressions = 1; // (byte)(Math.Max(Environment.ProcessorCount, readStream.Length / 4E9 + 1));

            var bufStreams = Enumerable.Range(1, parallelCompressions).
                Select(i => new Buffering.ReadBufferedStream()).ToArray();

            ITasker tasker = new Tasker();

            var readBufStream = new ReadBufferedStream();

            tasker.Queue(CompressorProcedures.ReadFromCompressedStreamToBuffer, readStream, readBufStream).
               ThenQueueForEach(CompressorProcedures.DecompressFromStreamToBuffer, bufStreams, readBufStream, null).
               Queue(CompressorProcedures.WriteDecompressedToStream, writeStream, bufStreams).
               StartAsync().
               WaitAll();

        }


    }

}



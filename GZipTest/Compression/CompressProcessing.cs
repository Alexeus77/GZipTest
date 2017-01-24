﻿using System.IO;
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
            const int parallelCompressions = 1;

            var buff = new BuffManager(parallelCompressions);
            var gZipStreams = GetGZipStreams(buff, parallelCompressions, CompressionMode.Compress);

            Tasker tasker = new Tasker();
            tasker.Run(CompressorProcedures.ReadAllFromStreamToBuffer, readStream, buff).
                ThenRunForEach(gZipStreams, CompressorProcedures.CompressAllToChunkedStream, (gzip) => { gzip.Close(); },
                    buff.SuspendAction, buff).
                Start().WaitAll();

            CompressorProcedures.WriteAllCompressedToStream(writeStream, buff);

        }

        private static GZipStream[] GetGZipStreams(BuffManager buffManager, int parallelCompressions, CompressionMode compressionMode)
        {
            GZipStream[] gzipStreams = new GZipStream[parallelCompressions];
            ChunkBufferedStream[] chunkedStreams = new ChunkBufferedStream[parallelCompressions];

            for (byte i = 0; i < parallelCompressions; i++)
            {
                chunkedStreams[i] = new ChunkBufferedStream(i, buffManager);
                gzipStreams[i] = new GZipStream(chunkedStreams[i], compressionMode);
            }

            return gzipStreams;
        }

        private static void DeCompressParallel(Stream readStream, Stream writeStream)
        {

            const int parallelCompressions = 1;

            var buff = new BuffManager(parallelCompressions);
            var gZipStreams = GetGZipStreams(buff, parallelCompressions, CompressionMode.Decompress);

            Tasker tasker = new Tasker();

            buff.SuspendAction = tasker.SuspendAction;

            tasker.Run(CompressorProcedures.ReadFromCompressedStreamToBuffer, readStream, buff).
                ThenRunForEach(gZipStreams, CompressorProcedures.DecompressToBufferToBuffer, (gzip) => { gzip.Close(); },
                buff.SuspendAction, buff).

                Start().WaitAll();

            CompressorProcedures.WriteDecompressedToStream(writeStream, buff);

        }

        //private static void CompressSeq(Stream readStream, Stream writeStream)
        //{
        //    var buff = new BuffManager(2);
        //    var chunkedStream = new ChunkBufferedStream(0, buff);
        //    var gzip = new GZipStream(chunkedStream, CompressionMode.Compress);

        //    CompressorProcedures.ReadAllFromStreamToBuffer(readStream, buff);
        //    CompressorProcedures.CompressAllToChunkedStream(gzip, buff);
        //    gzip.Close();
        //    CompressorProcedures.WriteAllCompressedToStream(writeStream, buff);

        //}

        //private static void DeCompressSeq(Stream readStream, Stream writeStream)
        //{
        //    var buff = new BuffManager(2);
        //    var chunkedStream = new ChunkBufferedStream(0, buff);
        //    var gzip = new GZipStream(chunkedStream, CompressionMode.Decompress);

        //    CompressorProcedures.ReadFromCompressedStreamToBuffer(readStream, buff);
        //    CompressorProcedures.DecompressToBufferToBuffer(gzip, buff);
        //    gzip.Close();

        //    CompressorProcedures.WriteDecompressedToStream(writeStream, buff);

        //}

    }

}



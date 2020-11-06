using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Tasks;
using static GZipTest.DebugDiagnostics;
using GZipTest.Streaming;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace GZipTest.Compression
{
    public static class Process
    {
        delegate void compressingAction(Stream readStream, Stream writeStream,
            int bufferSize, int compressionThreadsNumber,
            int maxBuffersToUse);

        public static void Compress(Stream fromStream, Stream toStream)
        {
            CompressDeCompress(CompressParallel, fromStream, toStream);
        }

        public static void Decompress(Stream fromStream, Stream toStream)
        {
            CompressDeCompress(DeCompressParallel, fromStream, toStream);
        }

        private static void CompressDeCompress(compressingAction action, Stream readStream, Stream writeStream)
        {
            action(readStream, writeStream, Settings.BufferSize, Settings.CompressorsCount, Settings.MaxBuffers);
        }

        //private static void CompressParallel2(Stream readStream, Stream writeStream,
        //    int bufferSize, int compressionThreadsNumber,
        //    int maxBuffersToUse)
        //{

        //    CompressAction(readStream, writeStream, 
        //        Settings.BufferSize, Settings.CompressorsCount, Settings.MaxBuffers,
        //        CompressorProcedures.ReadFromStream,
        //        CompressorProcedures.Compress,
        //        CompressorProcedures.ReadFromBuffers
        //        );
        //}

        //private static void CompressAction(Stream readStream, Stream writeStream,
        //   int bufferSize, int compressionThreadsNumber,
        //   int maxBuffersToUse,
        //   Action<Stream, Action> readAction,
        //   Action<Buffers, Action> compressDecompressAction,
        //   Action<Buffers[], Action> writeAction)
        //{

        //    var bufStreams = Enumerable.Range(1, compressionThreadsNumber).
        //        Select(i => new Buffers(bufferSize, maxBuffersToUse, maxBuffersToUse)).ToArray();

        //    ITasker tasker = new Tasker();

        //    var readBuffer = new Buffers(bufferSize, maxBuffersToUse, maxBuffersToUse);

        //    tasker.Queue(readAction, readStream).
        //        ThenQueueForEach(compressDecompressAction, bufStreams).
        //        Queue(writeAction, bufStreams).
        //        StartAsync().
        //        WaitAll();
        //}

        private static void CompressParallel(Stream readStream, Stream writeStream, 
            int bufferSize, int compressionThreadsNumber,
            int maxBuffersToUse)
        {

            var bufStreams = Enumerable.Range(1, compressionThreadsNumber).
                Select(i => new Buffers(bufferSize, maxBuffersToUse / 2 / compressionThreadsNumber, maxBuffersToUse)).ToArray();

            ITasker tasker = new Tasker();
            
            var readBuffer = new Buffers(bufferSize, maxBuffersToUse / 2, maxBuffersToUse);

            tasker.Queue(readBuffer.ReadFromStream, readStream).
                ThenQueueForEach(readBuffer.Compress, bufStreams).
                StartAsync().
                ThenRunSync(writeStream.WriteFromCompressedBuffers, bufStreams).
                WaitAll();
        }
                

        private static void DeCompressParallel(Stream readStream, Stream writeStream, 
            int bufferSize, int compressionThreadsNumber, int maxBuffersToUse)
        {

            var bufStreams = Enumerable.Range(1, compressionThreadsNumber).
                Select(i => new Buffers(bufferSize, maxBuffersToUse / 2 / compressionThreadsNumber, maxBuffersToUse)).ToArray();

            ITasker tasker = new Tasker();

            var readBufStream = new Buffers(bufferSize, maxBuffersToUse / 2, maxBuffersToUse);

            tasker.Queue(readBufStream.ReadFromCompressedStream, readStream).
               ThenQueueForEach(readBufStream.Decompress, bufStreams).
               StartAsync().
               ThenRunSync(writeStream.WriteFromDecompressedBuffers, bufStreams).
               WaitAll();

        }
    }

}



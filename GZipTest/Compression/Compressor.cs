using System.IO;
using GZipTest.Buffering;
using GZipTest.Tasks;
using System;
using System.Linq;

namespace GZipTest.Compression
{
    public class Compressor : ICompressor
    {
        ITasker tasker;
        int bufferSize;
        int compressionThreadsNumber;
        int maxBuffersToUse;

        
        public Compressor(ITasker tasker, int bufferSize, int compressionThreadsNumber, int maxBuffersToUse)
        {
            this.tasker = tasker;
            this.bufferSize = bufferSize;
            this.compressionThreadsNumber = compressionThreadsNumber;
            this.maxBuffersToUse = maxBuffersToUse;
        }

        public Compressor() 
            : this(new Tasker(), Settings.BufferSize, Settings.CompressorsCount, Settings.MaxBuffers)
        {
            
        }


    public void Compress(Stream fromStream, Stream toStream)
        {
            CompressAction(fromStream, toStream,
               CompressorProcedures.ReadFromStream,
               CompressorProcedures.Compress,
               CompressorProcedures.WriteFromCompressedBuffers
               );
        }

        public void Decompress(Stream fromStream, Stream toStream)
        {
            CompressAction(fromStream, toStream,
                CompressorProcedures.ReadFromCompressedStream,
                CompressorProcedures.Decompress,
                CompressorProcedures.WriteFromDecompressedBuffers
                );
        }

        private void CompressAction(Stream readStream, Stream writeStream,
           Action<Buffers, Stream> readAction,
           Action<Buffers, Buffers> compressDecompressAction,
           Action<Stream, Buffers[]> writeAction)
        {

            var readBuffer = new Buffers(bufferSize, maxBuffersToUse / 2, maxBuffersToUse);

            var compressBuffers = Enumerable.Range(1, compressionThreadsNumber).
                Select(i => new Buffers(bufferSize, maxBuffersToUse / 2 / compressionThreadsNumber, maxBuffersToUse / compressionThreadsNumber)).ToArray();

            tasker.ClearTasks();

            tasker.Queue(readAction, readBuffer, readStream).
                ThenQueueForEach(compressDecompressAction, readBuffer, compressBuffers).
                StartAsync().
                ThenRunSync(writeAction, writeStream, compressBuffers).
                WaitAll();
        }

    }
}



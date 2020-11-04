using System;
using System.IO;
using System.Threading;
using GZipTest.Buffering;
using GZipTest.Streaming;
using static GZipTest.DebugDiagnostics;

namespace GZipTest.Compression
{
    static class CompressorProcedures
    {
        #region compress

        public static void ReadFromStream(this Buffers toBuffer, Stream fromStream, Action signalCycled)
        {
            int count = toBuffer.BufferCapacity - sizeof(long);
            int numRead;

            do
            {
                var buffer = toBuffer.GetMemory();
                var position = fromStream.Position;
                buffer.WriteLong(fromStream.Position);

                numRead = buffer.ReadFrom(fromStream, count);

                if (numRead > 0)
                {
                    if(numRead != count)
                        buffer.SetLength(numRead + sizeof(long));
                    toBuffer.EnqueueBuffer(buffer);
                }
                signalCycled();

                ThreadMessage($"{position} : {numRead}");
            }
            while (numRead > 0 && numRead == count);
        }

        public static void Compress(this Buffers fromBuffer, Buffers toBuffers, Action signalCycled)
        {
            MemoryStream buffer;

            while ((buffer = fromBuffer.GetBuffer()) != null)
            {
                var compressed = buffer.Compress(toBuffers.GetMemory());
                compressed.Position = 0;
                toBuffers.EnqueueBuffer(compressed);
                fromBuffer.ReleaseBuffer(buffer);

                signalCycled();

                buffer.Position = 0;
                var position = buffer.ReadLong();
                ThreadMessage($"{position} : {buffer.Length}");
            }

        }

        public static void WriteFromCompressedBuffers(this Stream toStream, Buffers[] fromBuffers, Action signalCycled)
        {

            MemoryStream buffer;

            for (int i = 0; i < fromBuffers.Length; i++)
            {
                while ((buffer = fromBuffers[i].GetBuffer()) != null)
                {
                    //write block length
                    toStream.WriteLong(buffer.Length);

                    buffer.Position = 0;
                    toStream.ReadFrom(buffer, (int)buffer.Length);

                    fromBuffers[i].ReleaseBuffer(buffer);
                    signalCycled();

                    ThreadMessage($"{buffer.Length}");
                }
            }
        }


        #endregion compress

        #region decompress

        public static void ReadFromCompressedStream(this Buffers toBuffer, Stream fromStream,  Action signalCycled)
        {

            int numRead = 0;
            int chunckSize;
            do
            {
                MemoryStream memBytes = toBuffer.GetMemory();

                //read length of block
                var position = fromStream.Position;
                chunckSize = (int)fromStream.ReadLong();

                if (chunckSize > 0)
                {

                    numRead = memBytes.ReadFrom(fromStream, chunckSize);

                    if (numRead > 0)
                    {
                        memBytes.SetLength(chunckSize);
                        toBuffer.EnqueueBuffer(memBytes);
                    }

                    ThreadMessage($"{position} : {chunckSize}");

                    signalCycled();
                }

            }
            while (chunckSize > 0 && numRead > 0);
        }

        public static void Decompress(this Buffers fromBuffer, Buffers toBuffer, Action signalCycled)
        {
            MemoryStream buffer;

            while ((buffer = fromBuffer.GetBuffer()) != null)
            {
                //decompress data
                var decompressed = buffer.DeCompress(toBuffer.GetMemory());
                toBuffer.EnqueueBuffer(decompressed);

                fromBuffer.ReleaseBuffer(buffer);

                ThreadMessage($"{buffer.Length} : {decompressed.Length}");

                signalCycled();
            }
        }

        public static void WriteFromDecompressedBuffers(this Stream toStream, Buffers[] fromBuffers, Action signalCycled)
        {
            MemoryStream buffer;
            
            for (int i = 0; i < fromBuffers.Length; i++)
            {
                while ((buffer = fromBuffers[i].GetBuffer()) != null)
                {
                    buffer.Position = 0;
                    var position = buffer.ReadLong();
                    buffer.WriteTo(toStream, sizeof(long), position);

                    fromBuffers[i].ReleaseBuffer(buffer);

                    ThreadMessage($"{position} : {buffer.Length}");

                    signalCycled();
                }
            }
        }

        #endregion decompress
    }
}

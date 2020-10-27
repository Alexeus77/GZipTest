using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Streaming;
using static GZipTest.DebugDiagnostics;

namespace GZipTest.Compression
{
    static class CompressorProcedures
    {
        #region compress
        
        public static void ReadFromStreamToBuffer(Stream fromStream, ReadBufferedStream toBuffer, Action signalCycled)
        {
            int count;
            int numRead;
            
            do
            {
                MemoryStream memBytes = toBuffer.GetMemory();
                //decrease count read to save space for the header
                count = toBuffer.BufferCapacity - ReadBufferedStream.HeaderSize;

                var position = fromStream.Position;
                numRead = memBytes.ReadFrom(fromStream, count);

                if (numRead > 0)
                {
                    //save data along with its position to preserve ordering in buffer
                    toBuffer.Write(memBytes, position, numRead);
                }

                signalCycled();
            }
            while (numRead > 0 && numRead == count);
        }

        public static void CompressBufferDataToStream(ReadBufferedStream toCompressStream, BuffStream fromBuffer, Action signalCycled)
        {

            MemoryStream memBytes;
            
            while ((memBytes = fromBuffer.GetBuffer()) != null)
            {
                //compress data
                var mem = toCompressStream.GetMemory();

                using (var gzStream = new GZipStream(mem, CompressionMode.Compress, true))
                {
                    memBytes.WriteTo(gzStream);
                    gzStream.Close();

                    signalCycled();

                    fromBuffer.ReleaseBuffer(memBytes);
                }

                toCompressStream.Write(mem);
            }
        }
        
        public static void WriteCompressedBufferToStream(Stream toStream, BuffStream[] fromBuffers, Action signalCycled)
        {

            MemoryStream memBytes;

            for (int i = 0; i < fromBuffers.Length; i++)
            {
                while ((memBytes = fromBuffers[i].GetBuffer()) != null)
                {
                    //write block length
                    toStream.WriteLong(memBytes.Length);

                    memBytes.WriteTo(toStream);

                    fromBuffers[i].ReleaseBuffer(memBytes);

                    signalCycled();
                }
            }
        }


        #endregion compress

        #region decompress

        public static void ReadFromCompressedStreamToBuffer(Stream fromStream, ReadBufferedStream toBuffer, Action signalCycled)
        {

            int numRead;

            do
            {
                MemoryStream memBytes = toBuffer.GetMemory();
                
                //read length of block
                var chunckSize = (int)fromStream.ReadLong();

                numRead = memBytes.ReadFrom(fromStream, toBuffer.BufferCapacity);

                if (numRead > 0)
                {
                    memBytes.SetLength(numRead);
                    toBuffer.Write(memBytes);
                }

                signalCycled();
            }
            while (numRead > 0);
        }

        public static void DecompressFromStreamToBuffer(ReadBufferedStream toBuffer, ReadBufferedStream fromStream,  Action signalCycled)
        {
            MemoryStream memBytes;

            while ((memBytes = fromStream.GetBuffer()) != null)
            {
                //decompress data

                memBytes.Position = 0;

                using (var gzStream = new GZipStream(memBytes, CompressionMode.Decompress))
                {
                    int numRead;

                    var toMemory = toBuffer.GetMemory();

                    while ((numRead = toMemory.ReadFrom(gzStream, fromStream.BufferCapacity)) > 0)
                    {
                    }

                    gzStream.Close();

                    toBuffer.Write(toMemory);

                    signalCycled();
                }
            }

        }

        public static void WriteDecompressedToStream(Stream toStream, BuffStream[] fromBuffers, Action signalCycled)
        {

            MemoryStream memBytes;
             
            for (int i = 0; i < fromBuffers.Length; i++)
            {
                while ((memBytes = fromBuffers[i].GetBuffer()) != null)
                {

                    var position = memBytes.ReadLong();

                    memBytes.WriteTo(toStream, sizeof(long), position);

                    fromBuffers[i].ReleaseBuffer(memBytes);

                    signalCycled();
                }
            }
        }

        #endregion decompress



    }
}

using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Streaming;
using static System.Diagnostics.Debug;

namespace GZipTest.Compression
{
    static class CompressorProcedures
    {
        #region compress
        /// <summary>
        /// Reads uncompressed data into chunked buffer
        /// </summary>
        /// <param name="fromStream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static void ReadAllFromStreamToBuffer(Stream fromStream, IChunkedMemBuffer buffer)
        {
            while (ReadFromStreamToBuffer(fromStream, buffer)) { }
        }

        private static bool ReadFromStreamToBuffer(Stream fromStream, IChunkedMemBuffer toBuffer)
        {

            var memBytes = toBuffer.GetFree();
            var count = memBytes.Capacity;

            var numRead = memBytes.ReadFrom(fromStream, count);

            if (numRead > 0)
            {
                //cut length to actual bytes read
                memBytes.SetLength(numRead);

                //save data along with its position to preserve ordering in buffer
                toBuffer.Write(memBytes, fromStream.Position);
            }
            return numRead > 0 && numRead == count;
        }

        /// <summary>
        /// Compresses data from chunked buffer into chunked stream with GZipStream
        /// </summary>
        /// <param name="toStream">GzipStream holding chunked stream as a base</param>
        /// <param name="fromBuffer">Chunked buffer with uncompressed data</param>
        /// <returns></returns>
        public static void CompressAllToChunkedStream(GZipStream gzip, IChunkedMemBuffer fromBuffer)
        {
            while (CompressToChunkedStream(gzip, fromBuffer)) { }
        }

        public static bool CompressToChunkedStream(GZipStream toStream, IChunkedMemBuffer fromBuffer)
        {
            long position;

            //get data from chunked buffer with preserved position
            var memBytes = fromBuffer.Read(out position);
            if (memBytes != null)
            {
                //set underlying stream's position to preserve ordering in stream's buffer
                toStream.BaseStream.Position = position;
                //WriteLine($"C::{(toStream.BaseStream as ChunkBufferedStream).Id} {position} {memBytes.Length}");
                //compress data
                memBytes.WriteTo(toStream);

                //release buffer data
                fromBuffer.Release(memBytes);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Writes compressed data to output stream
        /// </summary>
        /// <param name="toStream">Output stream</param>
        /// <param name="fromBuffer">Chunked stream with compressed data</param>
        /// <returns></returns>
        public static void WriteAllCompressedFromBufferToStream(Stream toStream, IChunkedMemBuffer fromBuffer)
        {
            while (WriteFromCompressedBufferToStream(toStream, fromBuffer)) { }
        }

        private static bool WriteFromCompressedBufferToStream(Stream toStream, IChunkedMemBuffer fromBuffer)
        {
            long position;
            byte streamId;

            var memBytes = fromBuffer.Read(out position, out streamId);

            if (memBytes != null)
            {

                 WriteLine($"WC::{streamId} {position} {memBytes.Length}");

                //write block header
                toStream.WriteHeader(streamId, memBytes.Length);

                //write compressed block
                memBytes.WriteTo(toStream);

                //release buffer
                fromBuffer.Release(memBytes);
                return true;
            }
            else
                return false;
        }

        #endregion compress

        #region decompress

        /// <summary>
        /// Reads compressed data from input stream into chunked buffer
        /// </summary>
        /// <param name="fromStream">input stream</param>
        /// <param name="buffer">chunked buffer</param>
        /// <returns></returns>
        public static void ReadAllFromCompressedStreamToBuffer(Stream fromStream, IChunkedMemBuffer buffer)
        {
            while (ReadFromCompressedStreamToBuffer(fromStream, buffer)) { }
        }

        private static bool ReadFromCompressedStreamToBuffer(Stream fromStream, IChunkedMemBuffer toBuffer)
        {
            //read block header
            byte streamId;
            long length;

            fromStream.ReadHeader(out streamId, out length);

            //get free buffer for data
            var memBytes = toBuffer.GetFree();

            //read num of bytes specified in block header
            int count = (int)length;
            var numRead = memBytes.ReadFrom(fromStream, count);


            if (numRead > 0)
            {
                //WriteLine($"R::{streamId} {count}");

                //cut length to actual num read
                memBytes.SetLength(numRead);

                //write to buffer data, position and stream number
                toBuffer.Write(memBytes, fromStream.Position, streamId);
            }
            return numRead > 0 && numRead == count;

        }

        /// <summary>
        /// Decompresses data with GZipStream through chunked stream to chunked buffer
        /// </summary>
        /// <param name="fromStream">GZip stream with chunked stream as a base</param>
        /// <param name="buffer">Chunked buffer to hold decompressed data</param>
        /// <returns></returns>
        public static void ReadAllFromGZipStreamToBuffer(GZipStream fromStream, IChunkedMemBuffer buffer)
        {
            while (ReadFromGZipStreamToBuffer(fromStream, buffer)) { }
        }

        private static bool ReadFromGZipStreamToBuffer(GZipStream fromStream, IChunkedMemBuffer toBuffer)
        {

            var memBytes = toBuffer.GetFree();
            var count = memBytes.Capacity;
            var chunkedStream = fromStream.BaseStream as ChunkBufferedStream;

            Assert(chunkedStream != null, "The base stream for decompression is not of the valid type. Must be of ChunkBufferedStream type.");

            
            //read from underlying chunked stream through gzipstream descompression
            //each chunked stream reads data marked with its number
            var numRead = memBytes.ReadFrom(fromStream, count);

            if (numRead > 0)
            {
                //read position info from chunked stream
                //all positions read are queued and also blocks should be chained 
                //in the same order comparative with other threads' streams
                var position = chunkedStream.ReadPositions.Dequeue();

                //WriteLine($"D::{chunkedStream.Id} {position} {numRead}");

                memBytes.SetLength(numRead);
                //doesn't matter which chunked stream owns data, 
                //it just must be saved into buffer in position order
                toBuffer.Write(memBytes, position, (fromStream.BaseStream as ChunkBufferedStream).Id);
            }
            return numRead > 0 && numRead == count;
        }

        /// <summary>
        /// Writes decompressed data from buffer to output stream
        /// </summary>
        /// <param name="toStream">output stream</param>
        /// <param name="fromBuffer">chunked buffer</param>
        /// <returns></returns>
        public static void WriteAllFromBufferToStream(Stream toStream, IChunkedMemBuffer fromBuffer)
        {
            while (WriteFromBufferToStream(toStream, fromBuffer)) { }
        }

        private static bool WriteFromBufferToStream(Stream toStream, IChunkedMemBuffer fromBuffer)
        {
            long position;
            byte streamId;

            var memBytes = fromBuffer.Read(out position, out streamId);
            if (memBytes != null)
            {
                WriteLine($"W::{streamId} {position} {memBytes.Length}");

                memBytes.WriteTo(toStream);
                fromBuffer.Release(memBytes);
                return true;
            }
            else
                return false;
        }

        #endregion decompress



    }
}

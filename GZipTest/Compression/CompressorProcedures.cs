using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Streaming;

namespace GZipTest.Compression
{
    static class CompressorProcedures
    {
        #region compress
        
        public static void ReadFromStreamToBuffer(Stream fromStream, BuffManager toBuffer)
        {
            int count;
            int numRead;

            do
            {
                MemoryStream memBytes = toBuffer.GetFreeMem();
                count = BuffManager.ChunkSize;

                numRead = memBytes.ReadFrom(fromStream, count);

                if (numRead > 0)
                {
                    //cut length to actual bytes read
                    memBytes.SetLength(numRead);

                    //save data along with its position to preserve ordering in buffer
                    toBuffer.WriteSequenceBuf(memBytes, fromStream.Position);
                    toBuffer.AddSequencePos(fromStream.Position);

                    WriteLine($"R:: {fromStream.Position} {memBytes.Length}");
                }
            }
            while (numRead > 0 && numRead == count);
        }

        public static void CompressBufferDataToStream(GZipStream toStream, BuffManager fromBuffer)
        {

            MemoryStream memBytes;
            //get data from chunked buffer with preserved position
            long position;
            while ((memBytes = fromBuffer.ReadSequenceBuf(out position)) != null)
            {
                //set underlying stream's position to preserve ordering in stream's buffer
                toStream.BaseStream.Position = position;
                WriteLine($"C::{(toStream.BaseStream as ChunkBufferedStream).Id} {position} {memBytes.Length}");
                //compress data
                memBytes.WriteTo(toStream);

                //release buffer data
                fromBuffer.ReleaseMem(memBytes);
            }
        }

        private static void WriteLine(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public static void WriteCompressedBufferToStream(Stream toStream, BuffManager fromBuffer)
        {

            MemoryStream memBytes;
            byte streamId;

            //stop reading buffer if last block left
            if (fromBuffer.AtEndOfSequence())
                return;

            //we need position for correct block ordering
            long position = fromBuffer.GetSequencePos();

            

            //we get blocks in order. we get id of holding stream and store it in output stream
            while (position != -1 && (memBytes = fromBuffer.ReadCompressedBuffer(position, out streamId)) != null)
            {
                WriteLine($"WC::{streamId} {position} {memBytes.Length}");

                //write block header
                toStream.WriteHeader(streamId, position, memBytes.Length);

                //write compressed block
                memBytes.WriteTo(toStream);

                //release buffer
                fromBuffer.ReleaseMem(memBytes);

                //move to next blocl position
                fromBuffer.NextSequencePos();

                //stop reading buffer if last block left
                if (fromBuffer.AtEndOfSequence())
                    return;

                position = fromBuffer.GetSequencePos();
            }

        }

        public static void WriteCompressedBufferTailToStream(Stream toStream, BuffManager fromBuffer)
        {

            MemoryStream memBytes;
            byte streamId;

            //we need position for correct block ordering
            long position = fromBuffer.GetSequencePos();

            //we get blocks in order. we get id of holding stream and store it in output stream
            while (position != -1 && (memBytes = fromBuffer.ReadCompressedBuffer(position, out streamId)) != null)
            {
                WriteLine($"WC::{streamId} {position} {memBytes.Length}");

                //write block header
                toStream.WriteHeader(streamId, position, memBytes.Length);

                //write compressed block
                memBytes.WriteTo(toStream);

                //release buffer
                fromBuffer.ReleaseMem(memBytes);

                //move to next blocl position
                fromBuffer.NextSequencePos();
                position = fromBuffer.GetSequencePos();
            }

        }

        #endregion compress

        #region decompress

        /// <summary>
        /// Reads compressed data from input stream into chunked buffer
        /// </summary>
        /// <param name="fromStream">input stream</param>
        /// <param name="buffer">chunked buffer</param>
        /// <returns></returns>
        public static void ReadFromCompressedStreamToBuffer(Stream fromStream, BuffManager toBuffer)
        {

            //read block header
            byte streamId;
            long length;
            long position;

            while (fromStream.ReadHeader(out streamId, out position, out length))
            {
                //get free buffer for data
                MemoryStream memBytes = toBuffer.GetFreeMem();
                int numRead;

                //read num of bytes specified in block header
                int count = (int)length;

                numRead = memBytes.ReadFrom(fromStream, count);

                System.Diagnostics.Debug.Assert(numRead == count);

                WriteLine($"R::{streamId} {position} {count}");

                //cut length to actual num read
                memBytes.SetLength(numRead);

                //write to buffer data, position and stream number
                toBuffer.WriteCompressedBuffer(memBytes, position, streamId);

                //store position for ordering blocks
                toBuffer.AddSequencePos(position);

            }
        }

        /// <summary>
        /// Decompresses data with GZipStream through chunked stream to chunked buffer
        /// </summary>
        /// <param name="fromStream">GZip stream with chunked stream as a base</param>
        /// <param name="buffer">Chunked buffer to hold decompressed data</param>
        /// <returns></returns>
        public static void DecompressToBufferToBuffer(GZipStream fromStream, BuffManager toBuffer)
        {
            var chunkedStream = fromStream.BaseStream as ChunkBufferedStream;
            System.Diagnostics.Debug.Assert(chunkedStream != null, "The base stream for decompression is not of the valid type. Must be of ChunkBufferedStream type.");
            

            //read from underlying chunked stream through gzipstream descompression
            //each chunked stream reads data marked with its number
            int numRead;
            long position = 0;
            MemoryStream memCurrent = null;

            MemoryStream memBytes = toBuffer.GetFreeMem();
            while ((numRead = memBytes.ReadFrom(fromStream, BuffManager.ChunkSize)) > 0)
            {
                //read position info from chunked stream
                //all positions read are queued and also blocks should be chained 
                //in the same order comparative with other threads' streams

                if (chunkedStream.ReadPositions.Count > 0)
                    position = chunkedStream.ReadPositions.Dequeue();
                else
                {
                    memCurrent = toBuffer.ReadDeCompressedBufferForStream(out position, chunkedStream.Id);
                    memBytes.WriteTo(memCurrent);
                }

                WriteLine($"D::{chunkedStream.Id} {position} {numRead}");


                memBytes.SetLength(numRead);
                toBuffer.WriteDecompressedBuffer(memBytes, position, chunkedStream.Id);

                memBytes = toBuffer.GetFreeMem();

            }

        }

        /// <summary>
        /// Writes decompressed data from buffer to output stream
        /// </summary>
        /// <param name="toStream">output stream</param>
        /// <param name="buffManager">chunked buffer</param>
        /// <returns></returns>
        public static void WriteDecompressedToStream(Stream toStream, BuffManager buffManager)
        {
            MemoryStream memBytes;

            long position = buffManager.GetSequencePos();

            while (position != -1 && (memBytes = buffManager.ReadDecompressedBuffer(position)) != null)

            {
                WriteLine($"W::{position} {memBytes.Length}");

                memBytes.WriteTo(toStream);
                buffManager.ReleaseMem(memBytes);

                buffManager.NextSequencePos();
                position = buffManager.GetSequencePos();
            }

        }

        #endregion decompress



    }
}

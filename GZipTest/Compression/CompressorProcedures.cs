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
        
        public static void ReadFromStreamToBuffer(Stream fromStream, BuffManager toBuffer)
        {
            int count;
            int numRead;
            uint position = 0;

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
                    toBuffer.WriteSequenceBuf(memBytes, position);
                    toBuffer.AddSequencePos(position++);

                    WriteLine($"R:: {fromStream.Position} {memBytes.Length}");
                }
            }
            while (numRead > 0 && numRead == count);
        }

        public static void CompressBufferDataToStream(GZipStream toStream, BuffManager fromBuffer)
        {

            var bufferStream = toStream.BaseStream as Buffering.BufferedStream;


            if (bufferStream.Length + BuffManager.ChunkSize >= 4E9)
                return;

            MemoryStream memBytes;
            //get data from chunked buffer with preserved position
           
            while ((memBytes = fromBuffer.ReadSequenceBuf(out uint position)) != null)
            {
                //set underlying stream's position to preserve ordering in stream's buffer
                toStream.BaseStream.Position = position;
                WriteLine($"C::{bufferStream.Id} {position} {memBytes.Length}");

                //compress data
                memBytes.WriteTo(toStream);

                bufferStream.SetDataLength(bufferStream.Length + memBytes.Length);

                //release buffer data
                fromBuffer.ReleaseMem(memBytes);
            }
        }
        
        public static void WriteCompressedBufferToStream(Stream toStream, BuffManager fromBuffer)
        {

            MemoryStream memBytes;
            
            //stop reading buffer if last block left
            if (fromBuffer.AtEndOfSequence())
                return;

            
            //we need position for correct block ordering
            long position = fromBuffer.PeekSequencePos();

            

            //we get blocks in order. we get id of holding stream and store it in output stream
            while ((memBytes = fromBuffer.ReadCompressedBuffer(position, out byte streamId, false)) != null &&
                position != -1)
            {
                WriteLine2($"WC::{streamId} {position} {memBytes.Length} {fromBuffer.CompressedBuffersCount()}:{fromBuffer.ReleasedBuffersCount()}");
                
                //write block header
                toStream.WriteBlockHeader(streamId, (ushort)memBytes.Length);

                //write compressed block
                memBytes.WriteTo(toStream);

                //release buffer
                //fromBuffer.ReleaseMem(memBytes);
                
                ////stop reading buffer if last block left
                //if (fromBuffer.AtEndOfSequence())
                //    return;

                position = fromBuffer.GetNextSequencePos();
            }

        }

        public static void WriteCompressedBufferTailToStream(Stream toStream, BuffManager fromBuffer)
        {

            MemoryStream memBytes;
            
            //we need position for correct block ordering
            long position = fromBuffer.PeekSequencePos();

            //we get blocks in order. we get id of holding stream and store it in output stream
            while ((memBytes = fromBuffer.ReadCompressedBuffer(position, out byte streamId, true)) != null &&
                position != uint.MaxValue)
            {
                WriteLine($"WC::{streamId} {position} {memBytes.Length}");
                
                //write block header
                toStream.WriteBlockHeader(streamId, (ushort)memBytes.Length);

                //write compressed block
                memBytes.WriteTo(toStream);

                //release buffer
                //fromBuffer.ReleaseMem(memBytes);

                //move to next block position
                position = fromBuffer.GetNextSequencePos();
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
            ushort length;
        
            uint position = 0;

            while (fromStream.ReadBlockHeader(out streamId, out length))
            {
                //get free buffer for data
                MemoryStream memBytes = toBuffer.GetFreeMem();
                int numRead;

                //read num of bytes specified in block header
                int count = (int)length;

                numRead = memBytes.ReadFrom(fromStream, count);
                
                WriteLine($"R::{streamId} {position} {count}");

                //cut length to actual num read
                memBytes.SetLength(numRead);
                
                //write to buffer data, position and stream number
                toBuffer.WriteCompressedBuffer(memBytes, position, streamId);

                //store position for ordering blocks
                toBuffer.AddSequencePos(position++);

            }
        }

        public static void DecompressFromStreamToBuffer(GZipStream fromStream, BuffManager toBuffer)
        {
            var chunkedStream = fromStream.BaseStream as Buffering.BufferedStream;
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

                memBytes.SetLength(numRead);

                if (chunkedStream.ReadPositions.Count > 0)
                {
                    position = chunkedStream.ReadPositions.Dequeue();
                    toBuffer.WriteDecompressedBuffer(memBytes, position, chunkedStream.Id);
                }
                else
                {
                    memBytes.WriteTo(memCurrent);
                }

                WriteLine($"D::{chunkedStream.Id} {position} {numRead}");
                
                memCurrent = memBytes;

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

            //get posiotion in read order
            long position = buffManager.PeekSequencePos();

            //get the chunk from position needed
            while ((memBytes = buffManager.ReadDecompressedBuffer(position)) != null &&
                position != -1)

            {
                WriteLine($"W::{position} {memBytes.Length}");

                memBytes.WriteTo(toStream);
                
                //move to next position
                position = buffManager.GetNextSequencePos();
            }

        }

        #endregion decompress



    }
}

using System.IO;
using System;

namespace GZipTest.Streaming
{
    static class StreamExt
    {
        
        public static int ReadFrom(this MemoryStream memBytes, Stream stream, int count)
        {
            byte[] bytes = new byte[count];

            var numRead = stream.Read(bytes, 0, count);
            memBytes.Write(bytes, 0, numRead);

            return numRead;
        }

        public static void WriteBlockHeader(this Stream stream, byte streamId, ushort blockLength)
        {
            //write stream number of the block
            stream.WriteByte(streamId);

            ////write block position
            //var positionBytes = System.BitConverter.GetBytes(blockPosition);
            //stream.Write(positionBytes, 0, positionBytes.Length);

            //write block length
            var lengthBytes = BitConverter.GetBytes(blockLength);
            stream.Write(lengthBytes, 0, sizeof(ushort));
        }

        public static void WriteFileHeader(this Stream stream, byte streamsNumber)
        {
            stream.WriteByte(0x1f);
            stream.WriteByte(0x9b);
            stream.WriteByte(streamsNumber);
        }

        public static bool ReadFileHeader(this Stream stream, out byte streamsNumber)
        {
            streamsNumber = 0;

            if (stream.Length > 3)
            {
                byte gzipMagicNumber;
                if (stream.ReadByte() == 0x1f)
                {
                    gzipMagicNumber = (byte)stream.ReadByte();
                    if (gzipMagicNumber == 0x9b)
                        streamsNumber = (byte)stream.ReadByte();
                    else if (gzipMagicNumber == 0x8b)
                        streamsNumber = 1;
                }
            }

            
            return streamsNumber > 0;
        }

        public static bool ReadBlockHeader(this Stream stream, out byte streamId,  out ushort blockLength)
        {
            if (stream.Length - stream.Position > 1 + sizeof(short))
            {

                ///read block length

                //read stream number of the block
                streamId = (byte)stream.ReadByte();

                //var positionBytes = new byte[sizeof(long)];
                //stream.Read(positionBytes, 0, positionBytes.Length);
                //blockPosition = BitConverter.ToInt64(positionBytes, 0);

                var lengthBytes = new byte[sizeof(ushort)];
                stream.Read(lengthBytes, 0, sizeof(ushort));
                blockLength = BitConverter.ToUInt16(lengthBytes, 0);

                return true;
            }

            streamId = 0;
            blockLength = 0;
            
            return false;
        }

    }
}

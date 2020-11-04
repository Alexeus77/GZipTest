using System.IO;
using System;
using System.IO.Compression;

namespace GZipTest.Streaming
{
    static class StreamExt
    {

        public static void WriteLong(this Stream stream, long l)
        {
            stream.Write(BitConverter.GetBytes(l), 0, sizeof(long));
        }

        public static long ReadLong(this Stream stream)
        {
            var buf = new Byte[sizeof(long)];
            stream.Read(buf, 0, sizeof(long));

            return BitConverter.ToInt64(buf, 0);
        }

        public static int ReadFrom(this Stream toStream, Stream fromStream, int count)
        {
            byte[] bytes = new byte[count];

            var numRead = fromStream.Read(bytes, 0, count);
            toStream.Write(bytes, 0, numRead);

            return numRead;
        }

        public static void WriteTo(this MemoryStream memBytes, Stream stream, long fromOffset, long toOffset)
        {
            stream.Seek(toOffset, SeekOrigin.Begin);
            stream.Write(memBytes.GetBuffer(), (int)fromOffset, (int)(memBytes.Length - fromOffset));

        }

        public static MemoryStream Compress(this MemoryStream memoryStream, MemoryStream toStream)
        {
            using (var gz = new GZipStream(toStream, CompressionMode.Compress, true))
            {
                memoryStream.WriteTo(gz);
                gz.Close();
            }

            return toStream;
        }

        public static MemoryStream DeCompress(this MemoryStream memoryStream, MemoryStream toStream)
        {

            toStream.SetLength(0);

            using (var gzStream = new GZipStream(memoryStream, CompressionMode.Decompress, true))
            {
                memoryStream.Position = 0;

                while (toStream.ReadFrom(gzStream, (int)memoryStream.Length) > 0)
                {
                }

                gzStream.Close();
            }

            return toStream;
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

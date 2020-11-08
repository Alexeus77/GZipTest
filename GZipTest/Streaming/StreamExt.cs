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

        public static int ReadFromSetLen(this Stream toStream, Stream fromStream, int count)
        {
            byte[] bytes = new byte[count];

            var numRead = fromStream.Read(bytes, 0, count);
            if (numRead > 0)
            {
                var length = toStream.Position + numRead;
                if(length != toStream.Length)
                    toStream.SetLength(length);
                toStream.Write(bytes, 0, numRead);
            }
            return numRead;
        }

        public static int ReadFrom(this Stream toStream, Stream fromStream, int count)
        {
            byte[] bytes = new byte[count];

            var numRead = fromStream.Read(bytes, 0, count);
            if (numRead > 0)
            {
                toStream.Write(bytes, 0, numRead);
            }
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

            using (var gzStream = new GZipStream(memoryStream, CompressionMode.Decompress, true))
            {
                memoryStream.Position = 0;

                while (toStream.ReadFromSetLen(gzStream, (int)memoryStream.Length) > 0)
                {
                }

                gzStream.Close();
            }

            return toStream;
        }



    }
}

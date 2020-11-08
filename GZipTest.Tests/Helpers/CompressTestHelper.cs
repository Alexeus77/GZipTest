using System;
using System.Linq;
using System.IO;
using GZipTest.Streaming;

namespace GZipTest.Tests
{
    static class CompressTestHelper
    {
        private static Random random = new Random();

        public static MemoryStream RandomStream(int length)
        {
            var buffer = new byte[length];
            random.NextBytes(buffer);

            return new MemoryStream(buffer);
        }

        public static MemoryStream GetStreamOfBytes(int length, byte b)
        {
            var buffer = Enumerable.Repeat(b, length).ToArray();
            
            return new MemoryStream(buffer);
        }

        public static bool CompareBytes(this MemoryStream stream1, MemoryStream stream2)
        {
            return CompareBytes(stream1.ToArray(), stream2.ToArray());
        }

        public static bool CompareBytes(byte[] byte1, byte[] byte2)
        {
            if (byte1.Length != byte2.Length)
                return false;

            for (int i = 0; i < byte1.Length; i++)
            {
                if (byte1[i] != byte2[i])
                    return false;
            }

            return true;
        }

        public static void DecompressLinear(this Stream toStream, Stream fromStream)
        {
            int numRead = 0;
            int chunckSize;
            do
            {
                MemoryStream memBytes = new MemoryStream();

                //read length of block
                chunckSize = (int)fromStream.ReadLong();

                if (chunckSize > 0)
                {

                    numRead = memBytes.ReadFromSetLen(fromStream, chunckSize);

                    if (numRead > 0)
                    {
                        var decompressed = memBytes.DeCompress(new MemoryStream());
                        decompressed.Position = 0;

                        var position = decompressed.ReadLong();

                        decompressed.WriteTo(toStream, decompressed.Position, position);

                        DebugDiagnostics.WriteLine($"{chunckSize} : {position} : {toStream.Position}");
                    }

                }

            }
            while (chunckSize > 0 && numRead > 0);
        }
    }
}

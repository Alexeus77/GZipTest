using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.IO;
using static GZipTest.Compression.Process;

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
    }
}

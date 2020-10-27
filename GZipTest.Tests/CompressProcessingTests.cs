using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using static GZipTest.Compression.Process;
using static GZipTest.Tests.CompressTestHelper;
using System.IO.Compression;
using GZipTest.Streaming;
using GZipTest.Buffering;

namespace GZipTest.Tests
{
    [TestClass()]
    public class CompressProcessingTests
    {

        [TestMethod]
        public void CompressDecompressLinear()
        {
            var randomStream = CompressTestHelper.RandomStream(1024*1024*3);

            randomStream.Position = 0;

            var toStream = new MemoryStream();

            var readBuffer = new ReadBufferedStream();
            var compressBuffer = new ReadBufferedStream();

            Compression.CompressorProcedures.ReadFromStreamToBuffer(randomStream, readBuffer, ()=> { });
            Compression.CompressorProcedures.CompressBufferDataToStream(compressBuffer, readBuffer, () => { });
            Compression.CompressorProcedures.WriteCompressedBufferToStream(toStream, new ReadBufferedStream[] { compressBuffer }, () => { });

            toStream.Position = 0;

            var toStream2 = new MemoryStream();
            var readBuffer2 = new ReadBufferedStream();
            var compressBuffer2 = new ReadBufferedStream();

            Compression.CompressorProcedures.ReadFromCompressedStreamToBuffer(toStream, readBuffer2, () => { });
            Compression.CompressorProcedures.DecompressFromStreamToBuffer(compressBuffer2, readBuffer2, () => { });
            Compression.CompressorProcedures.WriteDecompressedToStream(toStream2, new ReadBufferedStream[] { compressBuffer2 }, () => { });

            Assert.IsTrue(CompareBytes(randomStream.ToArray(), toStream2.ToArray()));

        }

        [TestMethod]
        public void Compress()
        {
            var randomStream = CompressTestHelper.RandomStream(1024 * 1024 * 2);
            
            var toStream = new MemoryStream();

            Compression.Process.Compress(randomStream, toStream);

            toStream.Position = 0;

            var toStream2 = new MemoryStream();
            
            Compression.Process.Decompress(toStream, toStream2);

            Assert.IsTrue(CompareBytes(randomStream.ToArray(), toStream2.ToArray()));

            //Assert.IsTrue(toStream.Length > toStream2.Length && toStream.Length < (toStream2.Length + 1024 * 1024));
        }

        [TestMethod]
        public void Compress2()
        {
            var randomStream = CompressTestHelper.RandomStream(1024 * 1024 * 3);

            var toStream = new MemoryStream();

            using (var gz = new GZipStream(toStream, CompressionMode.Compress, true))
            {
                randomStream.WriteTo(gz);
                gz.Close();
            }

            

            var decompress = new MemoryStream(toStream.ToArray());
            
            var toStream2 = new MemoryStream();
            int numRead;

            using (var gz = new GZipStream(decompress, CompressionMode.Decompress))
            {
                while ((numRead = toStream2.ReadFrom(gz, 1024)) > 0)
                {
                }

                gz.Close();
            }

            Assert.IsTrue(CompareBytes(randomStream.ToArray(), toStream2.ToArray()));
        }

        private static bool CompareBytes(byte[] byte1, byte[] byte2)
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
    }
}
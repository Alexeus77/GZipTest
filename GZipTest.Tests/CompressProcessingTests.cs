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


namespace GZipTest.Tests
{
    [TestClass()]
    public class CompressProcessingTests

    {


        //[TestMethod()]
        public void CompressDecompressFile()
        {


            string source = @"C:\\ISO\\1\\VS2012.iso";
            CompressFile(source, source + ".gz");

            DeCompressFile(source + ".gz");
            // Assert.IsTrue(FileEquals(source, source + ".gz.iso"));
        }

        [TestMethod]
        public void CompressAndDecompressResourceTest()
        {
            foreach (var resName in GetCompressedResourcesNames())
            {
                for (int i = 0; i < 5; i++)
                {
                    Stream decompressed = GetDecompressedResource(resName, out _);
                    decompressed.Position = 0;

                    Stream compressedToTest = new MemoryStream();
                    Compress(decompressed, compressedToTest);

                    compressedToTest.Position = 0;

                    Stream decompressedToTest = new MemoryStream();
                    Decompress(compressedToTest, decompressedToTest);

                    Assert.IsTrue(decompressed.Length == decompressedToTest.Length,
                        $"Decompressed streams lengths are not equal. Iteration {i}");

                    Assert.IsTrue(CompareStreams(decompressed, decompressedToTest),
                        $"Decompressed streams contents are not equal. Iteration {i}");

                }
            }
        }

        
        private Stream GetDecompressedResource(string resourceName, out Stream resStream)
        {
            resStream = LoadTestResource(resourceName);

            Stream decompressedMemoryLinear = new MemoryStream();
            CompressTestHelper.DeCompressLinear(resStream, decompressedMemoryLinear);

            return decompressedMemoryLinear;

        }

        private bool CompareStreams(Stream stream1, Stream stream2)
        {
            if (stream1.Length != stream2.Length)
                return false;

            int byte1;
            int byte2;

            stream1.Position = 0;
            stream2.Position = 0;

            while ((byte1 = stream1.ReadByte()) != -1 && (byte2 = stream2.ReadByte()) != -1)
            {
                if (byte1 != byte2)
                    return false;
            }

            return stream1.Position == stream2.Position;
        }

        private Stream LoadTestResource(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        private IEnumerable<string> GetCompressedResourcesNames()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(name => Path.GetExtension(name) == ".gz");
        }
    }
}
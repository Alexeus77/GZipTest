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


        [TestMethod()]
        public void CompressDecompressFile()
        {
            string source = @"C:\\ISO\\1\\Alice_in_Wonderland.pdf";
            GZipTest.Tests.CompressTestHelper.
            CompressFile(source);
            //CompressFileLinear(source);

            //Assert.IsTrue(FileEquals(source + ".gz", source + ".2.gz"));

            //DeCompressFile(source + ".2.gz");
            DeCompressFile(source + ".gz");
            Assert.IsTrue(FileEquals(source, source + ".gz.iso"));
        }

        [TestMethod]
        public void CompressAndDecompressResourceTest()
        {
            //foreach (var resName in GetCompressedResourcesNames())

            var resName = GetCompressedResourcesNames().Where(n => n.Contains("Starter")).
            First();

            {
                Stream resStream;
                Stream decompressed = GetDecompressedResource(resName, out resStream);
                decompressed.Position = 0;

                Stream compressedToTest = new MemoryStream();
                Compress(decompressed, compressedToTest);

                compressedToTest.Position = 0;

                Stream decompressedToTest = new MemoryStream();
                Decompress(compressedToTest, decompressedToTest);

                Assert.IsTrue(CompareStreams(decompressed, decompressedToTest));

                //using (Stream w = new FileStream(@"C:\ISO\1\1.pdf", FileMode.Create, FileAccess.Write))
                //{
                //    (decompressed as MemoryStream).WriteTo(w);
                //}

            }
        }

        [TestMethod]
        public void CompressResourcesTest()
        {
            foreach (var resName in GetCompressedResourcesNames())
                CompressResourceTest(resName);
        }
        private void CompressResourceTest(string resName)
        {
            Stream resStream;
            Stream decompressed = GetDecompressedResource(resName, out resStream);
            decompressed.Position = 0;

            Stream compressedToTest = new MemoryStream();
            Compress(decompressed, compressedToTest);

            Assert.IsTrue(CompareStreams(resStream, compressedToTest));
        }

        [TestMethod]
        public void DecompressResourcesTest()
        {
            foreach (var resName in GetCompressedResourcesNames())
                DeCompressResourceTest(resName);
        }

        private void DeCompressResourceTest(string resName)
        {
            Stream resStream = LoadTestResource(resName);
            Stream decompressed = GetDecompressedResource(resName, out resStream);

            var decompressedToTest = new MemoryStream();

            resStream.Position = 0;
            Decompress(resStream, decompressedToTest);

            Assert.IsTrue(CompareStreams(decompressed, decompressedToTest));
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

            int byte1 = 0;
            int byte2 = 0;

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
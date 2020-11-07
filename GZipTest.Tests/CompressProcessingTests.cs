using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static GZipTest.Tests.CompressTestHelper;
using GZipTest.Streaming;
using GZipTest.Buffering;
using GZipTest.Compression;
using GZipTest.Tests.TestStreams;
using GZipTest.Tests.Tasks;
using GZipTest.Tasks;

namespace GZipTest.Tests
{
    [TestClass()]
    public class CompressProcessingTests
    {

        ICompressor compressor = null;

        [TestInitialize]
        public void Init()
        {
            Settings.CompressorsCount = 8;

            compressor = new Compressor();
        }

        [TestMethod]
        public void Compress_EndlessStream()
        {
            var endless = new EndlessStream((long)(Settings.BufferSize * 1E2));
            var voidStream = new VoidStream();

            compressor.Compress(endless, voidStream);
        }

        //[TestMethod]
        //public void Compress_EndlessStreamToFile()
        //{
        //    var destFile = @"C:\Users\admin\source\repos\GZipTest\TestCmd\test.blob.gz2";

        //    File.Delete(destFile);

        //    var endlessStream = new EndlessStream((long)1E12);

        //    using (var writeStream = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write))
        //    {
        //        compressor.Compress(endlessStream, writeStream);
        //    }

        //}

        //[TestMethod]
        //public void DeCompress_FileToVoidStream()
        //{
        //    var sourceFile = @"C:\Users\admin\source\repos\GZipTest\TestCmd\test.blob.gz2";

        //    var voidStream = new VoidStream();

        //    using (var readStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //    {
        //        compressor.Decompress(readStream, voidStream);
        //    }
        //}

        [TestMethod]
        public void Linear_CheckEqual()
        {

            var randomStream = RandomStream(Settings.BufferSize * 2);

            randomStream.Position = 0;
            var compressedStream = new MemoryStream();

            compressor = new Compressor(new LinearTasker(), Settings.BufferSize, 1, 0);

            compressor.Compress(randomStream, compressedStream);

            compressedStream.Position = 0;

            var decompressedStream = new MemoryStream();

            compressor = new Compressor(new LinearTasker(), Settings.BufferSize, 1, 0);

            compressor.Decompress(compressedStream, decompressedStream);

            Assert.IsTrue(randomStream.CompareBytes(decompressedStream));

        }

        [TestMethod]
        public void ReadFromStreamToBuffer_CheckCounters()
        {

            //arrange
            const int bufAmp = 3;

            const int expectedBufCount = 4;
            const int expectedReleasedCount = 0;
            
            var stream = new MemoryStream(new byte[Settings.BufferSize * bufAmp]);
            var buffStream = new Buffers(Settings.BufferSize);

            //act
            buffStream.ReadFromStream(stream);

            //assert
            Assert.AreEqual(expectedBufCount, buffStream.BuffersCount, "Buffer count");
            Assert.AreEqual(expectedReleasedCount, buffStream.ReleasedCount, "Released count");            
        }

        [TestMethod]
        public void CompressBufferDataToStream_CheckCounters()
        {
            //arrange
            const int bufAmp = 3;

            var buffStream = new Buffers(Settings.BufferSize);
            var buffStreamCompressed = new Buffers(Settings.BufferSize);
            var randomStream = RandomStream(Settings.BufferSize * bufAmp);
            buffStream.ReadFromStream(randomStream);
                        
            //act
            buffStream.Compress(buffStreamCompressed);

            //assert

            Assert.AreEqual(4, buffStreamCompressed.BuffersCount);
            Assert.AreEqual(4, buffStream.ReleasedCount);
        }
        [TestMethod]
        public void Parallel_CheckEqual()
        {
            MemoryStream randomStream = RandomStream(Settings.BufferSize * 5);

            var compressedStream = new MemoryStream();
            randomStream.Position = 0;

            compressor.Compress(randomStream, compressedStream);

            var decompressedStream = new MemoryStream();

            compressedStream.Position = 0;

            compressor.Decompress(compressedStream, decompressedStream);

            Assert.IsTrue(randomStream.CompareBytes(decompressedStream));

            //CompareBytes(randomStream.ToArray(), decompressedStream.ToArray());
        }

        [TestMethod]
        public void Compress_ThenDecompressLinear_CheckEqual()
        {
            var randomStream = RandomStream(Settings.BufferSize * 125);

            var compressedStream = new MemoryStream();
            randomStream.Position = 0;

            compressor.Compress(randomStream, compressedStream);

            var decompressedStream = new MemoryStream();
            
            compressedStream.Position = 0;

            decompressedStream.DecompressLinear(compressedStream);
            
            Assert.IsTrue(randomStream.CompareBytes(decompressedStream));

            //CompareBytes(randomStream.ToArray(), decompressedStream.ToArray());
        }

        [TestMethod]
        public void Decompress_СompressedLinear_CheckEqual()
        {
            var randomStream = RandomStream(Settings.BufferSize *125);

            randomStream.Position = 0;
            var compressed = new MemoryStream();

            var readBuffer = new Buffers(Settings.BufferSize);
            var compressBuffer = new Buffers(Settings.BufferSize);


            readBuffer.ReadFromStream(randomStream);
            readBuffer.Compress(compressBuffer);
            compressed.WriteFromCompressedBuffers(new Buffers[] { compressBuffer });

            compressed.Position = 0;

            var decompressed = new MemoryStream();

            compressor.Decompress(compressed, decompressed);
                        
            Assert.IsTrue(randomStream.CompareBytes(decompressed));
        }

    }
}
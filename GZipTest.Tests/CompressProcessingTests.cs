using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static GZipTest.Tests.CompressTestHelper;
using GZipTest.Streaming;
using GZipTest.Buffering;
using GZipTest.Compression;
using GZipTest.Tests.TestStreams;

namespace GZipTest.Tests
{
    [TestClass()]
    public class CompressProcessingTests
    {

        [TestInitialize]
        public void Init()
        {
            Settings.CompressorsCount = 8;
        }

        [TestMethod]
        public void Compress_EndlessStream()
        {
            var endless = new EndlessStream((long)(Settings.BufferSize * 1E2));
            var voidStream = new VoidStream();

            Process.Compress(endless, voidStream);
        }

        [TestMethod]
        public void Compress_EndlessStreamToFile()
        {
            var destFile = @"C:\Users\admin\source\repos\GZipTest\TestCmd\test.blob.gz2";

            File.Delete(destFile);

            var endlessStream = new EndlessStream((long)1E12);

            using (var writeStream = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write))
            {
                Process.Compress(endlessStream, writeStream);
            }

        }

        [TestMethod]
        public void DeCompress_FileToVoidStream()
        {
            var sourceFile = @"C:\Users\admin\source\repos\GZipTest\TestCmd\test.blob.gz2";

            var voidStream = new VoidStream();

            using (var readStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Process.Decompress(readStream, voidStream);
            }
        }

        [TestMethod]
        public void Linear()
        {

            var randomStream = RandomStream(Settings.BufferSize * 25);

            randomStream.Position = 0;
            var toStream = new MemoryStream();

            var readBuffer = new Buffers(Settings.BufferSize);
            var compressBuffer = new Buffers(Settings.BufferSize);

            void emptyAction() { }

            readBuffer.ReadFromStream(randomStream, emptyAction);
            readBuffer.Compress(compressBuffer, emptyAction);
            toStream.WriteFromCompressedBuffers(new Buffers[] { compressBuffer }, emptyAction);


            toStream.Position = 0;

            var decompressedStream = new MemoryStream();
            var readCompressedBuffer = new Buffers(Settings.BufferSize);
            var decompressedBuffer = new Buffers(Settings.BufferSize);

            readCompressedBuffer.ReadFromCompressedStream(toStream, emptyAction);
            readCompressedBuffer.Decompress(decompressedBuffer, emptyAction);
            decompressedStream.WriteFromDecompressedBuffers(new Buffers[] { decompressedBuffer }, emptyAction);

            Assert.IsTrue(randomStream.CompareBytes(decompressedStream));

        }

        [TestMethod]
        public void ReadFromStreamToBuffer_CheckCounters()
        {

            //arrange
            const int bufAmp = 3;

            const int expectedBufCount = 4;
            const int expectedReleasedCount = 0;
            const int expectedCycles = 4;

            var stream = new MemoryStream(new byte[Settings.BufferSize * bufAmp]);
            var buffStream = new Buffers(Settings.BufferSize);

            int cycles = 0;
            void cyclesAction() { cycles++; }

            var checkStream = new MemoryStream();

            //act
            buffStream.ReadFromStream(stream, cyclesAction);

            //assert
            Assert.AreEqual(expectedBufCount, buffStream.BuffersCount, "Buffer count");
            Assert.AreEqual(expectedReleasedCount, buffStream.ReleasedCount, "Released count");
            Assert.AreEqual(expectedCycles, cycles);
        }

        [TestMethod]
        public void CompressBufferDataToStream_CheckCounters()
        {
            //arrange
            const int bufAmp = 3;

            var buffStream = new Buffers(Settings.BufferSize);
            var buffStreamCompressed = new Buffers(Settings.BufferSize);

            int cycles = 0;
            void emptyAction() { }
            void signalAction() { cycles++; }

            var randomStream = RandomStream(Settings.BufferSize * bufAmp);
            buffStream.ReadFromStream(randomStream, emptyAction);

            var compressedStream = new MemoryStream();

            //act
            buffStream.Compress(buffStreamCompressed, signalAction);


            //assert

            Assert.AreEqual(4, buffStreamCompressed.BuffersCount);
            Assert.AreEqual(4, buffStream.ReleasedCount);
        }
        [TestMethod]
        public void Compress_ThenDecompress_CheckEqual()
        {
            MemoryStream randomStream = RandomStream(Settings.BufferSize * 5);

            var compressedStream = new MemoryStream();
            randomStream.Position = 0;

            Process.Compress(randomStream, compressedStream);

            var decompressedStream = new MemoryStream();

            compressedStream.Position = 0;

            decompressedStream.DecompressLinear(compressedStream);

            Assert.IsTrue(randomStream.CompareBytes(decompressedStream));

            //CompareBytes(randomStream.ToArray(), decompressedStream.ToArray());
        }

        [TestMethod]
        public void MyTestMethod2()
        {
            for (int i = 0; i < 10; i++)
            {
                Compress_ThenDecompressLinear_CheckEqual();
            }
        }

        [TestMethod]
        public void Compress_ThenDecompressLinear_CheckEqual()
        {
            var randomStream = RandomStream(Settings.BufferSize * 125);

            var compressedStream = new MemoryStream();
            randomStream.Position = 0;

            Process.Compress(randomStream, compressedStream);

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

            void emptyAction() { }

            readBuffer.ReadFromStream(randomStream, emptyAction);
            readBuffer.Compress(compressBuffer, emptyAction);
            compressed.WriteFromCompressedBuffers(new Buffers[] { compressBuffer }, emptyAction);

            compressed.Position = 0;

            var decompressed = new MemoryStream();

            Process.Decompress(compressed, decompressed);
                        
            Assert.IsTrue(randomStream.CompareBytes(decompressed));
        }

    }
}
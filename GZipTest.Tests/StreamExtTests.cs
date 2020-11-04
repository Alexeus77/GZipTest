using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GZipTest.Streaming;

namespace GZipTest.Tests
{
    [TestClass()]
    public class StreamExtTests
    {
        [TestMethod]
        public void ReadLong_Value_Equals_WriteLong()
        {
            var value = long.MaxValue;
            
            var memory = new MemoryStream(8);
            memory.WriteLong(value);

            memory.Position = 0;

            var checkValue = memory.ReadLong();

            Assert.IsTrue(checkValue == value);
        }

        [TestMethod]
        public void ReadLong_OnUnWind_ResultsZero()
        {
            var value = long.MaxValue;

            var memory = new MemoryStream(8);
            memory.WriteLong(value);

            var checkValue = memory.ReadLong();

            Assert.IsTrue(checkValue == 0);
        }

        [TestMethod]
        public void Compress_ReuseOutputStream_SameResult()
        {
            
            var randomStream = CompressTestHelper.RandomStream(100);
            var compressed = randomStream.Compress(new MemoryStream());
            var compressedTest = new MemoryStream();

            for (int i = 0; i < 100; i++)
            {
                compressedTest = randomStream.Compress(compressedTest);

                compressedTest.CompareBytes(compressed);
            }
        }
    }
}

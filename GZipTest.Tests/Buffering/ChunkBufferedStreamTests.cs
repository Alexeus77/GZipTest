using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTest.Buffering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GZipTest.Streaming;

namespace GZipTest.Buffering.Tests
{
    [TestClass()]
    public class ChunkBufferedStreamTests
    {
        [TestMethod()]
        public void Write_And_Read_ChunkedMemBuffer_Test()
        {
            var chunkedStream = new ChunkBufferedStream(0);

            const int chunkWriteSize = 1024 * 200;

            Random rnd = new Random();
            Byte[] bytes = GetByteArrayFilled(chunkWriteSize);

            //rnd.NextBytes(bytes);

            int ii = 0, count = Math.Min(bytes.Length - ii, rnd.Next(5, 9000));

            for (; ii + count <= bytes.Length && count > 0; ii += count, count = Math.Min(bytes.Length - ii, rnd.Next(5, 9000)))
                chunkedStream.Write(bytes, ii, count);

            Assert.AreEqual(bytes.Length, ChunkBufferedStream.ChunkedMemBuffer.WorkSize());

            //for (int i = 0; i < 8 * 1024; i += 1024)
            //    chunkedStream.Write(bytes, i, 1024);

            var writtenBuffers = ChunkBufferedStream.ChunkedMemBuffer.WorkBuffersCount;

            Byte[] testBytes = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i += 1024 * 4)
            {
                chunkedStream.Read(testBytes, i, 1024 * 4);
            }

            Assert.AreEqual(writtenBuffers, ChunkBufferedStream.ChunkedMemBuffer.ReleasedBuffersCount);

            Assert.IsTrue(CompareBytes(bytes, testBytes));
        }

        

        private static bool ReadFromStreamToBuffer(Stream fromStream, ChunkedMemBuffer toBuffer)
        {

            var memBytes = toBuffer.GetFree();

            var count = memBytes.Capacity;
            var position = fromStream.Position;
            var numRead = memBytes.ReadFrom(fromStream, count);

            if (numRead > 0)
            {


                memBytes.SetLength(numRead);
                toBuffer.Write(memBytes, position, 0);
            }
            
            return numRead > 0 && numRead == count;
        }

        private static bool WriteFromBufferToStream(Stream toStream, ChunkedMemBuffer fromBuffer)
        {
            long position = 0;

            var memBytes = fromBuffer.ReadForStream(out position, 0);
            if (memBytes != null)
            {
                memBytes.WriteTo(toStream);
                fromBuffer.Release(memBytes);
                return true;
            }
            else
                return false;
        }

        
        private byte[] GetByteArrayFilled(int size)
        {
            Byte[] bytes = new Byte[size];

            for (int i = 0; i < bytes.Length; i += 255)
            {
                for (byte j = 0; j < 255 && (i + j < bytes.Length); j++)
                {
                    bytes[i + j] = j;
                }
            }

            return bytes;
        }

        private bool CompareBytes(byte[] byte1, byte[] byte2)
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
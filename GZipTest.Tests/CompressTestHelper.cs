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
        public static void CompressFile(string fileToCompress, string archiveFile)
        {
            if (File.Exists(archiveFile))
                File.Delete(archiveFile);

            using (var readStream = new FileStream(fileToCompress, FileMode.Open, FileAccess.Read))
            {
                using (var writeStream = new FileStream(archiveFile, FileMode.CreateNew, FileAccess.Write))
                {
                    Compress(readStream, writeStream);
                }

            }
        }



        public static void CompressFileLinear(string fileToCompress)
        {
            string archiveName = fileToCompress + ".2.gz";

            if (File.Exists(archiveName))
                File.Delete(archiveName);

            using (var readStream = new FileStream(fileToCompress, FileMode.Open, FileAccess.Read))
            {
                using (var writeStream = new FileStream(archiveName, FileMode.CreateNew, FileAccess.Write))
                {
                    CompressTestHelper.CompressLinear(readStream, writeStream);
                }

            }
        }
        public static void DeCompressFile(string fileToDeCompress)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            string fileName = fileToDeCompress + ".iso";

            if (File.Exists(fileName))
                File.Delete(fileName);

            using (var readStream = new FileStream(fileToDeCompress, FileMode.Open, FileAccess.Read))
            {
                using (var writeStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
                {
                    Decompress(readStream, writeStream);
                }

            }
        }

        public static void DeCompressLinear(Stream stream, Stream decompressed)
        {

            byte[] bytes = new byte[1024 * 1024];
            int numRead;
            stream.Position = 0;
            using (var compressionStream = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                //read
                while ((numRead = compressionStream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    //decompress 
                    decompressed.Write(bytes, 0, numRead);
                }
            }
        }

        public static void CompressLinear(Stream stream, Stream compressed)
        {

            byte[] bytes = new byte[1024 * 8];
            int numRead;
            stream.Position = 0;
            using (var compressionStream = new GZipStream(compressed, CompressionMode.Compress, true))
            {
                //read
                while ((numRead = stream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    //decompress 
                    compressionStream.Write(bytes, 0, numRead);
                }
            }
        }

        public static bool FileEquals(string path1, string path2)
        {
            using (var readStream1 = new FileStream(path1, FileMode.Open, FileAccess.Read))
            {
                using (var readStream2 = new FileStream(path2, FileMode.Open, FileAccess.Read))
                {
                    if (readStream1.Length != readStream2.Length)
                        return false;

                    byte[] buff1 = new byte[1024 * 8];
                    byte[] buff2 = new byte[1024 * 8];

                    while (readStream1.Position != readStream1.Length && readStream2.Position != readStream2.Length)
                    {
                        ReadBuffer(readStream1, buff1);
                        ReadBuffer(readStream2, buff2);

                        if (!CompareBytes(buff1, buff2))
                            return false;

                        //if (readStream1.ReadByte() != readStream2.ReadByte())
                        //    return false;
                    }
                }
            }

            return true;
        }

        private static void ReadBuffer(Stream stream, byte[] buff)
        {
            stream.Read(buff, 0, (int)Math.Min(buff.Length, stream.Length - stream.Position));
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

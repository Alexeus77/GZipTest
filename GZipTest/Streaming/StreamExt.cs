using System.IO;

namespace GZipTest.Streaming
{
    static class StreamExt
    {
        
        public static int ReadFrom(this MemoryStream memBytes, Stream stream, int count)
        {
            byte[] bytes = new byte[count];

            var numRead = stream.Read(bytes, 0, count);
            memBytes.Write(bytes, 0, numRead);

            return numRead;
        }

        public static void WriteHeader(this Stream stream, byte streamId, long blockLength)
        {
            //write stream number of the block
            stream.WriteByte(streamId);

            //write block length
            var lengthBytes = System.BitConverter.GetBytes((short)blockLength);
            stream.Write(lengthBytes, 0, lengthBytes.Length);
        }

        public static void ReadHeader(this Stream stream, out byte streamId, out long blockLength)
        {
            ///read block length

            //read stream number of the block
            streamId = (byte)stream.ReadByte();

            var lengthBytes = new byte[sizeof(short)];
            stream.Read(lengthBytes, 0, lengthBytes.Length);
            blockLength = (long)System.BitConverter.ToInt16(lengthBytes, 0);
        }

    }
}

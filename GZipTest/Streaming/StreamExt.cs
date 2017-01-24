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

        public static void WriteHeader(this Stream stream, byte streamId, long blockPosition, long blockLength)
        {
            //write stream number of the block
            stream.WriteByte(streamId);

            //write block position
            var positionBytes = System.BitConverter.GetBytes(blockPosition);
            stream.Write(positionBytes, 0, positionBytes.Length);

            //write block length
            var lengthBytes = System.BitConverter.GetBytes((short)blockLength);
            stream.Write(lengthBytes, 0, lengthBytes.Length);
        }

        public static bool ReadHeader(this Stream stream, out byte streamId, out long blockPosition, out long blockLength)
        {
            if (stream.Length - stream.Position > 1 + sizeof(short))
            {

                ///read block length

                //read stream number of the block
                streamId = (byte)stream.ReadByte();

                var positionBytes = new byte[sizeof(long)];
                stream.Read(positionBytes, 0, positionBytes.Length);
                blockPosition = System.BitConverter.ToInt64(positionBytes, 0);

                var lengthBytes = new byte[sizeof(short)];
                stream.Read(lengthBytes, 0, lengthBytes.Length);
                blockLength = (long)System.BitConverter.ToInt16(lengthBytes, 0);

                return true;
            }

            streamId = 0;
            blockLength = 0;
            blockPosition = 0;
            return false;
        }

    }
}

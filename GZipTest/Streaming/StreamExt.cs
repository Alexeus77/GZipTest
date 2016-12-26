using System.IO;

namespace GZipTest.Streaming
{
    static class StreamExt
    {
        
        public static int ReadFrom(this MemoryStream memBytes, Stream stream)
        {
            byte[] bytes = new byte[(int)memBytes.Capacity];

            var numRead = stream.Read(bytes, 0, bytes.Length);
            memBytes.Write(bytes, 0, numRead);

            return numRead;
        }
        
    }
}

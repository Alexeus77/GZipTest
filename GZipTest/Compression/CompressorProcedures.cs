using System.IO;
using System.IO.Compression;
using GZipTest.Buffering;
using GZipTest.Streaming;

namespace GZipTest.Compression
{
    static class CompressorProcedures
    {
        #region compress
       
        public static bool CompressAllToChunkedStream(GZipStream gzip, IChunkedMemBuffer fromBuffer)
        {

            while (CompressToChunkedStream(gzip, fromBuffer)) { }

            return true;
        }


        public static bool CompressToChunkedStream(GZipStream toStream, IChunkedMemBuffer fromBuffer)
        {
            var memBytes = fromBuffer.Read();
            if (memBytes != null)
            {
                memBytes.Position = 0;
                memBytes.WriteTo(toStream);
                fromBuffer.Release(memBytes);
                return true;
            }
            else
                return false;
        }

        public static bool WriteAllCompressedBufferToStream(Stream toStream, IChunkedMemBuffer fromBuffer)
        {
            while (WriteFromBufferToStream(toStream, fromBuffer)) { }
            return true;
        }

        private static bool WriteCompressedBufferToStream(Stream toStream, IChunkedMemBuffer fromBuffer)
        {
            var memBytes = fromBuffer.Read();
            if (memBytes != null)
            {
                memBytes.Position = 0;
                memBytes.WriteTo(toStream);
                fromBuffer.Release(memBytes);
                return true;
            }
            else
                return false;
        }

        #endregion compress

        #region decompress
        //public static bool DecompressFromStreamBuffer(GZipStream gzip, IChunkedMemBuffer buff)
        //{
        //    ReadFromStreamToBufferAll(gzip, buff);   
        //    return true;
        //}

        //private static bool DecomressAllСhunkedStreamToBuffer(GZipStream fromStream, IChunkedMemBuffer toBuffer)
        //{
        //    while (ReadFromStreamToBuffer(fromStream, toBuffer)) { }
        //    return true;
        //}


        //private static int DecomressСhunkedStreamToBuffer(GZipStream fromStream, IChunkedMemBuffer toBuffer)
        //{
        //    var memBytes = toBuffer.GetFree();
        //    System.Diagnostics.Debug.Assert(memBytes.Position == 0);
        //    var numRead = memBytes.ReadFrom(fromStream);

        //    if (numRead > 0)
        //    {
        //        memBytes.SetLength(numRead);
        //        toBuffer.Write(memBytes);
        //    }

        //    return numRead;
        //}

        public static bool ReadFromCompressedStreamToBufferAll(GZipStream fromStream, IChunkedMemBuffer buffer)
        {
            while (ReadFromStreamToBuffer(fromStream, buffer)) { }
            return true;
        }

        #endregion decompress

        public static bool WriteFromBufferToStreamAll(Stream toStream, IChunkedMemBuffer buffer)
        {
            while (WriteFromBufferToStream(toStream, buffer)) { }
            return true;
        }
        
        

        public static bool ReadFromStreamToBufferAll(Stream fromStream, IChunkedMemBuffer buffer)
        {
            while (ReadFromStreamToBuffer(fromStream, buffer)) { }
            return true;
        }


        private static bool ReadFromStreamToBuffer(Stream fromStream, IChunkedMemBuffer toBuffer)
        {

             var memBytes = toBuffer.GetFree();
            var count = memBytes.Capacity;
            
            var numRead = memBytes.ReadFrom(fromStream);
            
            if (numRead > 0)
            {
                memBytes.SetLength(numRead);
                toBuffer.Write(memBytes);
            }
            return numRead > 0 && numRead == count;
        }

        private static bool WriteFromBufferToStream(Stream toStream, IChunkedMemBuffer fromBuffer)
        {
            var memBytes = fromBuffer.Read();
            if (memBytes != null)
            {
                memBytes.WriteTo(toStream);
                fromBuffer.Release(memBytes);
                return true;
            }
            else
                return false;
        }
  
        
       

    }
}

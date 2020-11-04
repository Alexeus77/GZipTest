using System;

namespace GZipTest
{
    static class Settings
    {
        public static int BufferSize { get; set; }
        public static int CompressorsCount { get; set; }
        public static int MaxBuffers { get; set; }

        static Settings()
        {
            BufferSize = 1024 * 1024;
            CompressorsCount = Environment.ProcessorCount;
            MaxBuffers = 200;
        }
    }
}

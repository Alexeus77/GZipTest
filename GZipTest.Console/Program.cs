using System;

namespace GZipTest
{
    class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                    (new DllResourceLoader()).AssemblyResolveFromEmbeddedResource;
        }

        static int Main(string[] args)
        {
            return (new Runner(new Compression.Compressor(), new Logger())).Run(args);
        }

    }
}

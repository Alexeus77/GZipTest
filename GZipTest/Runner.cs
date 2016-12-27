using System;
using System.IO;
using static GZipTest.Compression.Process;

namespace GZipTest
{
    
    public class Runner
    {
        const string archiveExt = "gz";

        public void Main(string[] args)
        {
            var arguments = new ArgumentsParser(args);

            if (arguments.Mode == ArgumentsParser.enMode.Undefined)
            {
                throw new ArgumentException("Specify either compress or decompress as a parameter.");
            }

            if (arguments.SourceFile == null)
            {
                throw new ArgumentException("Specify source file parameter.");
            }

            //if (arguments.TargetFile == null)
            //{
            //    throw new ArgumentException("Specify target file parameter.");
            //}


            Process(arguments.SourceFile, arguments.TargetFile, arguments.Mode);
        }

        public void Process(string sourceFile, string targetFile, ArgumentsParser.enMode mode)
        {
            CompressDecompress(sourceFile, targetFile, mode == ArgumentsParser.enMode.Compress ?
                new Action<Stream, Stream>(Compress) : new Action<Stream, Stream>(Decompress));
        }


        public void CompressDecompress(string sourceFile, string destFile, Action<Stream, Stream> compressDecompressAction)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException($"Source file '{sourceFile}' not found.");

            if (string.IsNullOrEmpty(destFile))
                destFile = $"{sourceFile}.{archiveExt}";

                
                if (File.Exists(destFile))
                    File.Delete(destFile);

            using (var readStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                using (var writeStream = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write))
                {
                    compressDecompressAction(readStream, writeStream);
                }

            }
        }
    }
}

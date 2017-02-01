using System;
using System.IO;
using static GZipTest.Compression.Process;
using System.Diagnostics;

namespace GZipTest
{
    
    public class Runner 
    {
        const string archiveExt = ".gz2";
        
        public void Start(string[] args)
        {
            try
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
            catch (Exception e) when (e is ArgumentException || e is InvalidDataException ||
                e is FileNotFoundException || e is IOException)
            {
                throw new Exceptions.CatchedException(e);
            }
        }

        public void Process(string sourceFile, string targetFile, ArgumentsParser.enMode mode)
        {
            if (string.IsNullOrEmpty(targetFile))
                targetFile = GetTargetFileNameFromSource(sourceFile, mode);

            if (targetFile == null)
                throw new ArgumentException("Specify target file parameter.");
            

            CompressDecompress(sourceFile, targetFile, mode == ArgumentsParser.enMode.Compress ?
                new Action<Stream, Stream>(Compress) : new Action<Stream, Stream>(Decompress));
        }

        private string GetTargetFileNameFromSource(string sourceFile, ArgumentsParser.enMode mode)
        {
            return mode == ArgumentsParser.enMode.Compress ?
                    $"{sourceFile}{archiveExt}" :
                    sourceFile.EndsWith(archiveExt, StringComparison.OrdinalIgnoreCase) ?
                    sourceFile.Substring(0, sourceFile.LastIndexOf(archiveExt, StringComparison.OrdinalIgnoreCase)) :
                    null;
        }


        public void CompressDecompress(string sourceFile, string destFile, Action<Stream, Stream> compressDecompressAction)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException($"Source file '{sourceFile}' not found.");

            if (File.Exists(destFile))
            {
                Console.WriteLine($"Destination file '{destFile}' already exist. Overwrite? (y/n)");
                if (Console.Read() == 'y')
                    File.Delete(destFile);
                else
                    return;
            }

            Console.WriteLine($"Started processing.\nSource file '{sourceFile}'.\nDestination file '{destFile}'");

            Stopwatch sw = new Stopwatch();

            sw.Start();

            using (var readStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var writeStream = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write))
                {
                    compressDecompressAction(readStream, writeStream);
                }

            }

            Console.WriteLine($"Completed in {(decimal)sw.ElapsedMilliseconds / 1000} second(s).");
        }
    }
}

using System;
using System.IO;
using System.Diagnostics;
using GZipTest.Compression;
using System.Runtime.ConstrainedExecution;

namespace GZipTest
{
    public class Runner 
    {
        const string archiveExt = ".gz2";
        private readonly ICompressor compressor;
        private readonly ILogger logger;

        public Runner(ICompressor compressor, ILogger logger)
        {
            this.compressor = compressor;
            this.logger = logger;
        }

        public int Run(string[] args)
        {
            try
            {
                Start(args);
                return 0;
            }
            catch (Exceptions.CatchedException ae)
            {
                logger.Log(ae.InnerException.Message);
            }
            catch (Tasks.TaskerAggregateException taskException)
            {
                foreach (var e in taskException.InnerExceptions)
                {
                    logger.Log($"Error {e.Message} {e.Source} \n {e.StackTrace}");
                }
            }
            catch (Exception e)
            {

                var msg = $"Unexpected error occured during processing the command. Error {e.Message} {e.Source}.";
                logger.Log(msg);

                System.Diagnostics.Debug.WriteLine($"{msg} {e.StackTrace}");
            }

            return 1;
        }

        public void Start(string[] args)
        {
            try
            {

                var arguments = new ArgumentsParser(args);

                if (arguments.Mode == ArgumentsParser.RunMode.Undefined)
                {
                    throw new ArgumentException("Specify either compress or decompress as a parameter.");
                }

                if (arguments.SourceFile == null)
                {
                    throw new ArgumentException("Specify source file parameter.");
                }


                Process(compressor, arguments.SourceFile, arguments.TargetFile, arguments.Mode);
            }
            catch (Exception e) when (e is ArgumentException || e is InvalidDataException ||
                e is FileNotFoundException || e is IOException)
            {
                throw new Exceptions.CatchedException(e);
            }
        }

        public void Process(ICompressor compressor, string sourceFile, string targetFile, ArgumentsParser.RunMode mode)
        {
            if (string.IsNullOrEmpty(targetFile))
                targetFile = GetTargetFileNameFromSource(sourceFile, mode);

            if (targetFile == null)
                throw new ArgumentException("Specify target file parameter.");
            

            CompressDecompress(sourceFile, targetFile, mode == ArgumentsParser.RunMode.Compress ?
                new Action<Stream, Stream>(compressor.Compress) : 
                new Action<Stream, Stream>(compressor.Decompress));
        }

        private string GetTargetFileNameFromSource(string sourceFile, ArgumentsParser.RunMode mode)
        {
            return mode == ArgumentsParser.RunMode.Compress ?
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
                logger.Log($"Destination file '{destFile}' already exist. Overwrite? (y/n)");
                if (Console.Read() == 'y')
                    File.Delete(destFile);
                else
                    return;
            }

            logger.Log($"Started processing.\nSource file '{sourceFile}'.\nDestination file '{destFile}'");

            Stopwatch sw = new Stopwatch();

            sw.Start();

            using (var readStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var writeStream = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write))
                {
                    compressDecompressAction(readStream, writeStream);
                }
            }

            logger.Log($"Completed in {(decimal)sw.ElapsedMilliseconds / 1000} second(s).");
        }
    }
}

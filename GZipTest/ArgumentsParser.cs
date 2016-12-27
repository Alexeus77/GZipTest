using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GZipTest
{
    public class ArgumentsParser
    {
        public static string compress = "compress";
        public static string decompress = "decompress";

        public enum enMode
        {
            Undefined,
            Compress,
            Decompress
        }

        public ArgumentsParser(string[] args)
        {
            
            Mode = args.Contains(ArgumentsParser.compress, StringComparer.OrdinalIgnoreCase) ? enMode.Compress :
                args.Contains("decompress", StringComparer.OrdinalIgnoreCase) ? enMode.Decompress : enMode.Undefined;

            var files = args.Where(arg => !arg.Equals("compress", StringComparison.OrdinalIgnoreCase)
               && !arg.Equals("decompress", StringComparison.OrdinalIgnoreCase));

            SourceFile = files.FirstOrDefault();
            TargetFile = files.Skip(1).FirstOrDefault();
        }

        public string SourceFile { get; private set; }
        public string TargetFile { get; private set; }
        public enMode Mode { get; private set; }
        
    }
}

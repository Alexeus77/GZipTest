using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GZipTest
{
    public class ArgumentsParser
    {
        public enum enMode
        {
            Undefined,
            Compress,
            Decompress
        }

        public ArgumentsParser(string[] args)
        {
            
            Mode = args.Contains("compress", StringComparer.InvariantCultureIgnoreCase) ? enMode.Compress :
                args.Contains("decompress", StringComparer.InvariantCultureIgnoreCase) ? enMode.Decompress : enMode.Undefined;

            var files = args.Where(arg => !arg.Equals("compress", StringComparison.InvariantCultureIgnoreCase)
               && !arg.Equals("decompress", StringComparison.InvariantCultureIgnoreCase));

            SourceFile = files.FirstOrDefault();
            TargetFile = files.LastOrDefault();
        }

        public string SourceFile { get; private set; }
        public string TargetFile { get; private set; }
        public enMode Mode { get; private set; }
    }
}

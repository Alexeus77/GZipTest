using System;
using System.Collections.Generic;


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

            List<string> arguments = new List<string>(args);

            
            Mode = arguments.Contains(ArgumentsParser.compress) ? enMode.Compress :
                arguments.Contains(ArgumentsParser.decompress) ? enMode.Decompress : enMode.Undefined;

            var files = arguments.Exclude(ArgumentsParser.compress).Exclude(ArgumentsParser.decompress);
               
            SourceFile = files.Count > 0 ? files[0] : null;
            TargetFile = files.Count > 1 ? files[1] : null;
        }

        public string SourceFile { get; private set; }
        public string TargetFile { get; private set; }
        public enMode Mode { get; private set; }
        
    }

    public static class StringArrExt
    {
        public static bool Contains(this List<string> strings, string stringToSearch)
        {
            foreach (var str in strings)
                if (str.Equals(stringToSearch, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public static List<string> Exclude(this List<string> strings, string stringToExclude)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i].Equals(stringToExclude, StringComparison.OrdinalIgnoreCase))
                    strings.RemoveAt(i);
            }

            return strings;
        }

        
    }
}

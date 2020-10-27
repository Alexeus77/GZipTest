using System;
using System.Diagnostics;

namespace GZipTest
{
    static class DebugDiagnostics
    {
        [Conditional("DEBUGOUTPUT2")]
        public static void WriteLine2(string message)
        {
            Console.WriteLine(message);
        }

        [System.Diagnostics.Conditional("DEBUGOUTPUT1")]
        public static void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }


        [System.Diagnostics.Conditional("DEBUGOUTPUT3")]
        public static void WriteLine3(string message)
        {
            Debug.WriteLine(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void ConsoleWriteLine(string message)
        {
            Console.WriteLine(message);
        }


    }
}

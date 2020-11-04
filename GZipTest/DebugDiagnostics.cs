using System;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    static class DebugDiagnostics
    {
        [Conditional("DEBUGOUTPUT")]
        public static void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }

        [Conditional("DEBUGOUTPUT")]
        public static void ThreadMessage(string message)
        {
            WriteLine($"{Thread.CurrentThread.Name}: {message}");
        }

        [Conditional("DEBUGOUTPUT")]
        public static void ConsoleWriteLine(string message)
        {
            Console.WriteLine(message);
        }


    }
}

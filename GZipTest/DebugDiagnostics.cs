using System;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    static class DebugDiagnostics
    {
        static int i = 0;

        private static PerformanceCounter cpuCounter;
        private static PerformanceCounter ramCounter;

        static DebugDiagnostics()
        {
            cpuCounter = new PerformanceCounter(
            "Processor",
            "% Processor Time",
            "_Total",
            true
            );

            ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
        }
        
        [Conditional("DEBUGOUTPUT")]
        public static void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }

        [Conditional("DEBUGOUTPUT")]
        public static void ThreadMessage(string message)
        {
            WriteLine($"{Thread.CurrentThread.Name}: {message}");

            i++;

            if (i % 200 == 0)
            {
                ConsoleWriteLine($"GC allocated: {GC.GetTotalMemory(false) / (1024*1024)} Mb");
                WriteCounters();
            }
        }

        [Conditional("DEBUGOUTPUT1")]
        private static void WriteCounters()
        {
            ConsoleWriteLine($"CPU now: {cpuCounter.NextValue()} %");
            ConsoleWriteLine($"RAM now: {ramCounter.NextValue()} Mb");
        }

        [Conditional("DEBUGOUTPUT")]
        public static void ConsoleWriteLine(string message)
        {
            Console.WriteLine(message);
        }

    }
}

using System;
using System.Diagnostics;

namespace GZipTest
{
    class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                    (new DllResourceLoader()).AssemblyResolveFromEmbeddedResource;
        }


        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();
                new Runner().Main(args);
                Console.WriteLine($"Completed in {(decimal)sw.ElapsedMilliseconds / 1000} second(s).");
            }
            catch (Exceptions.CatchedException ae)
            {
                Console.WriteLine(ae.InnerException.Message);
            }
            catch (Tasks.TaskerAggregateException taskException)
            {
                foreach(var e in taskException.InnerExceptions)
                {
                    Console.WriteLine($"Error {e.Message} {e.Source}.");
                }
            }
            catch (Exception e)
            {

                var msg = "Unexpected error occured during processing the command. " +
                    $"Error {e.Message} {e.Source}.";
                Console.WriteLine(msg);

                System.Diagnostics.Debug.WriteLine($"{msg} {e.StackTrace}");
            }
        }

    }
}

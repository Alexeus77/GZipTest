using System;


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
               
                new Runner().Start(args);
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

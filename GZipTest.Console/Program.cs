using System;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Runner().Main(args);
            }
            catch(Exception e)
            {

                var msg = "Unexpected error occured during processing the command" +
                    $"Error {e.Message}, {e.Source}.";
                Console.WriteLine(msg);

                System.Diagnostics.Debug.WriteLine($"{msg} {e.StackTrace}");
            }
        }

    }
}

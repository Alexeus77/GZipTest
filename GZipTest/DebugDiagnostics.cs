using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest
{
    static class DebugDiagnostics
    {
        [System.Diagnostics.Conditional("DEBUGOUTPUT2")]
        public static void WriteLine2(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        [System.Diagnostics.Conditional("DEBUGOUTPUT1")]
        public static void WriteLine(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        
    }
}

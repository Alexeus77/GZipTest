using System;

namespace GZipTest.Exceptions
{
    public class CatchedException : Exception
    {
        public CatchedException(Exception innerException) : base("", innerException)
        {
            
        }
    }
}

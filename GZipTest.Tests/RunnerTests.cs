using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass()]
    public class RunnerTests
    {
        string[] paramsWithoutMode = new string[] { "" };
        string[] paramsCompressModeWithoutFile = new string[] { "cOmpress" };
        string[] paramsDeCompressModeWithoutFile = new string[] { "dEcompress" };
        string[] paramsDeCompressModeWithoutTargetFile = new string[] { "dEcompress", "source" };
        string[] paramsDeCompressModeSourceFileNotFound = new string[] { "dEcompress", "source", "target" };

        static Runner runner;

        [ClassInitializeAttribute]
        public static void ClassInit(TestContext context)
        {
            runner = new Runner(new Compression.Compressor(), new Logger());
        }

        [TestMethod()]
        [ExpectedException(typeof(Exceptions.CatchedException))]
        public void Expect_ArgumentException_ModeNotSpecifiedTest()
        {
            runner.Start(paramsWithoutMode);
        }

        [TestMethod()]
        [ExpectedException(typeof(Exceptions.CatchedException))]
        public void Expect_ArgumentException_FileNotSpecifiedTest()
        {
            
            runner.Start(paramsCompressModeWithoutFile);
        }

       
        [ExpectedException(typeof(Exceptions.CatchedException))]
        public void Expect_ArgumentException_TargetFileNotSpecifiedTest()
        {
            
            runner.Start(paramsDeCompressModeWithoutTargetFile);
        }

        [TestMethod()]
        [ExpectedException(typeof(Exceptions.CatchedException))]
        public void Expect_ArgumentException_SourceFileNotFound()
        {
            
            runner.Start(paramsDeCompressModeSourceFileNotFound);
        }

    }
}
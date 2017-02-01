using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest.Tests
{
    [TestClass()]
    public class ExecutionTests
    {
        string[] paramsWithoutMode = new string[] { "" };
        string[] paramsCompressModeWithoutFile = new string[] { "cOmpress" };
        string[] paramsDeCompressModeWithoutFile = new string[] { "dEcompress" };
        string[] paramsDeCompressModeWithoutTargetFile = new string[] { "dEcompress", "source" };
        string[] paramsDeCompressModeSourceFileNotFound = new string[] { "dEcompress", "source", "target" };

        static GZipTest.Runner exec;

        [ClassInitializeAttribute]
        public static void ClassInit(TestContext context)
        {
            exec = new Runner();
        }

        [TestMethod()]
        [ExpectedException(typeof(Exceptions.CatchedException))]
        public void Expect_ArgumentException_ModeNotSpecifiedTest()
        {
            exec.Start(paramsWithoutMode);
        }

        [TestMethod()]
        [ExpectedException(typeof(Exceptions.CatchedException))]
        public void Expect_ArgumentException_FileNotSpecifiedTest()
        {
            
            exec.Start(paramsCompressModeWithoutFile);
        }

       
        [ExpectedException(typeof(Exceptions.CatchedException))]
        public void Expect_ArgumentException_TargetFileNotSpecifiedTest()
        {
            
            exec.Start(paramsDeCompressModeWithoutTargetFile);
        }

        [TestMethod()]
        [ExpectedException(typeof(Exceptions.CatchedException))]
        public void Expect_ArgumentException_SourceFileNotFound()
        {
            
            exec.Start(paramsDeCompressModeSourceFileNotFound);
        }

    }
}
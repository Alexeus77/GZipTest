using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        public static void ClassInit()
        {
            exec = new Runner();
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void Expect_ArgumentException_ModeNotSpecifiedTest()
        {
            exec.Main(paramsWithoutMode);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void Expect_ArgumentException_FileNotSpecifiedTest()
        {
            
            exec.Main(paramsCompressModeWithoutFile);
        }

       
        [ExpectedException(typeof(ArgumentException))]
        public void Expect_ArgumentException_TargetFileNotSpecifiedTest()
        {
            
            exec.Main(paramsDeCompressModeWithoutTargetFile);
        }

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Expect_ArgumentException_SourceFileNotFound()
        {
            
            exec.Main(paramsDeCompressModeSourceFileNotFound);
        }

    }
}
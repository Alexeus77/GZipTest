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
        string[] paramsDeCompressModeWithoutTargetFile = new string[] { "dEcompress source" };
        string[] paramsDeCompressModeSourceFileNotFound = new string[] { "dEcompress source target" };

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Expect_ArgumentException_ModeNotSpecifiedTest()
        {
            Runner exec = new Runner();
            exec.Main(paramsWithoutMode);
        }

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Expect_ArgumentException_FileNotSpecifiedTest()
        {
            Runner exec = new Runner();
            exec.Main(paramsCompressModeWithoutFile);
        }

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Expect_ArgumentException_TargetFileNotSpecifiedTest()
        {
            Runner exec = new Runner();
            exec.Main(paramsDeCompressModeWithoutTargetFile);
        }

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Expect_ArgumentException_SourceFileNotFound()
        {
            Runner exec = new Runner();
            exec.Main(paramsDeCompressModeSourceFileNotFound);
        }

    }
}
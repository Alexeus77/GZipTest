using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTest.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest.Tasks.Tests
{
    [TestClass()]
    public class TaskerTests
    {
        [TestMethod()]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Expect_DivideByZeroException()
        {
            try
            {
                var tasker = new Tasker();
                tasker.Run(SuspendThread, "", "", null).
                    ThenRun(SuspendThread, "", "", null).
                    ThenRun(Devide, 1, 0, null).Start().WaitAll();
            }
            catch(TaskerAggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    if (e.GetType() == typeof(DivideByZeroException))
                        throw e;
            }

        }

        private bool SuspendThread(string s1, string s2)
        {
            System.Threading.Thread.Sleep(1000);
            return true;
        }

        private bool Devide(int i1, int i2)
        {
            var i = i1 / i2;
            return true;
        }

    }
}
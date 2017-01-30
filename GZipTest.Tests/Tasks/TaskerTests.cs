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
                tasker.Run(SuspendThread, "", "").
                    ThenQueue(SuspendThread, "", "").
                    ThenQueueWithContinue(SuspendThread, 1, 0, Devide).StartAsync().WaitAll();
            }
            catch(TaskerAggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    if (e.GetType() == typeof(DivideByZeroException))
                        throw e;
            }

        }

        private void SuspendThread(string s1, string s2)
        {
            System.Threading.Thread.Sleep(1000);
        }

        private void SuspendThread(int i1, int i2)
        {
            System.Threading.Thread.Sleep(1000);
        }

        private void Devide(int i1, int i2)
        {
            var i = i1 / i2;
        }

    }
}
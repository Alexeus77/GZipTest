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
        public void Run_Tasker()
        {
            try
            {
                var strIn = "LAZY FOX";
                var strOut = new StringBuilder();

                var queue1 = new Queue<char>();
                var queue2 = new Queue<char>();
                ITasker tasker = new Tasker();
                tasker.Queue(First, queue1, strIn).
                    ThenQueueForEach(Second, new Queue<char>[] { queue1 }, queue2, null).
                    Queue(Third, queue2, strOut).
                    StartAsync().
                    WaitAll();

                //Assert.AreEqual(strIn, strOut.ToString());
            }
            catch(TaskerAggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    if (e.GetType() == typeof(DivideByZeroException))
                        throw e;
            }

        }

        private void First(Queue<char> queue, string str, Action signalAction)
        {
            for (int i = 0; i < str.Length; i++)
            {
                queue.Enqueue(str[i]);
                signalAction();
            }
        }

        private void Second(Queue<char> queue1, Queue<char> queue2, Action signalAction)
        {
            char c = queue1.Dequeue();
            queue2.Enqueue(c);
            signalAction();   
        }

        private void Third(Queue<char> queue2, StringBuilder str, Action signalAction)
        {
            char c = queue2.Dequeue();
            str.Append(c);
        }

    }
}
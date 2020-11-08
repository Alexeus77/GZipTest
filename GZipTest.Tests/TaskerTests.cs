using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTest.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace GZipTest.Tasks.Tests
{
    [TestClass()]
    public class TaskerTests
    {
        [TestMethod()]
        public void Run_Tasker()
        {

            var strIn = "123456789";
            var strOut = new StringBuilder(strIn.Length);

            var queue1 = new ConcurrentQueue<char>();
            var queues = Enumerable.Range(1, 4).Select(i => new ConcurrentQueue<char>()).ToArray();

            ITasker tasker = new Tasker();

            tasker.Queue(First, queue1, new Queue<char>(strIn.ToCharArray())).
                ThenQueueForEach(Second, queue1, queues).
                Queue(Third, queues, strOut).
                StartAsync().
                WaitAll();

            var arrayResult = strOut.ToString().ToCharArray();
            Array.Sort(arrayResult);

            Assert.AreEqual(strIn, new string(arrayResult));

        }

        private void First(ConcurrentQueue<char> queue, Queue<char> queueIn)
        {
            while (queueIn.Count != 0)
                queue.Enqueue(queueIn.Dequeue());
            
        }

        private void Second(ConcurrentQueue<char> queueIn, ConcurrentQueue<char> queueOut)
        {
            char c;

            while (queueIn.TryDequeue(out c))
                queueOut.Enqueue(c);

        }

        private void Third(ConcurrentQueue<char>[] queueIn, StringBuilder str)
        {
            char c;

            for (int i = 0; i < queueIn.Length; i++)
            {
                while (queueIn[i].TryDequeue(out c))
                    str.Append(c);

            }
        }

    }
}
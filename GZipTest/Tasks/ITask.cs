using System;
using System.Threading;

namespace GZipTest.Tasks
{
    partial class Tasker
    {
        private interface ITask
        {
            bool FinishedFlag { get; }
            Func<bool> Finished { get; set; }
            ManualResetEvent FinishedEvent { get; }
            Exception Exception { get; }
            void Start();
            void StartSync();
            Func<bool> WaitLoopCompleted { get; }
        }
    }
}

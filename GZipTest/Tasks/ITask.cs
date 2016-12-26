using System;
using System.Threading;

namespace GZipTest.Tasks
{
    partial class Tasker
    {
        private interface ITask
        {
            bool Finished();
            ManualResetEvent FinishedEvent { get; }
            Exception Exception { get; }
            void Start();
            void StartSync();

        }
    }
}

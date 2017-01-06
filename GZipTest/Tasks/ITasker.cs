using System;

namespace GZipTest.Tasks
{
    public interface ITasker
    {
        ITasker ThenRun<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2);
        ITasker ThenRunWithContinue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, Action continueWith);
        ITasker Start();
        void StartSequential();
        void WaitAll();
    }
}
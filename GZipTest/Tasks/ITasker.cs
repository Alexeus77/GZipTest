using System;

namespace GZipTest.Tasks
{
    public interface ITasker
    {
        ITasker ThenRun<T1, T2>(Func<T1, T2, bool> action, T1 param1, T2 param2, Func<bool> suspendCondition);
        ITasker ThenRunWithContinue<T1, T2>(Func<T1, T2, bool> action, T1 param1, T2 param2, Func<bool> suspendCondition, Action continueWith);
        ITasker Start();
        void StartSequential();
        void WaitAll();
    }
}
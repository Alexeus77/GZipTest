using System;
using System.Collections.Generic;

namespace GZipTest.Tasks
{
    public interface ITasker
    {
        ITasker ThenRun<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2);
        ITasker ThenRunWithContinue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, Action<T1, T2> continueWith);
        ITasker ThenRunForEach<T1, T2>(IEnumerable<T1> objects, Action<T1, T2> action1, Action<T1> continueWith, 
            Func<bool> suspendAction, T2 param2);
        ITasker Start();
        void StartSequential();
        void WaitAll();
    }
}
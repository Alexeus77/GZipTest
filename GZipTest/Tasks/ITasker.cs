using System;
using System.Collections.Generic;

namespace GZipTest.Tasks
{
    public interface ITasker
    {
        ITasker ThenQueue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2);
        ITasker ThenQueueWithContinue<T1, T2>(Action<T1, T2> action, 
            T1 param1, T2 param2, Action<T1, T2> continueWith);
        ITasker ThenRunWithContinueSync<T1, T2>(Action<T1, T2> action,
            T1 param1, T2 param2, Action<T1, T2> continueWith);
        ITasker ThenRunSync<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2);


        ITasker ThenQueueForEach<T1, T2>(IEnumerable<T1> objects, 
            Action<T1, T2> action1, Action<T1> continueWith, 
            Func<int, bool> suspendAction, T2 param2);
        ITasker StartAsync();
        void StartSequential();
        void WaitAll();
    }
}
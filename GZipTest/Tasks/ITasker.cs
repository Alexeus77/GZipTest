using System;
using System.Collections.Generic;

namespace GZipTest.Tasks
{
    public interface ITasker
    {
       
        ITasker StartAsync();
        void StartSequential();
        void WaitAll();
        ITasker ThenQueueForEach<T1, T2>(Action<T1, T2, Action> action1, IEnumerable<T1> objects, T2 param2, Action<T1> continueWith);
        ITasker Queue<T1, T2>(Action<T1, T2, Action> action, T1 param1, T2 param2);
        //ITasker ThenRunWithContinueSync<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, Action<T1, T2> continueWith);
        //ITasker ThenQueueWithContinue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, Action<T1, T2> continueWith);
        //ITasker ThenRunSync<T1, T2>(Action<T1, T2, Action> action, T1 param1, T2 param2);
    }
}
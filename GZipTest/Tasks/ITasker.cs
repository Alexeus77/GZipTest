using System;
using System.Collections.Generic;

namespace GZipTest.Tasks
{
    public interface ITasker
    {
       
        ITasker StartAsync();
        ITasker StartSequential();
        void WaitAll();
        
        ITasker ThenQueueForEach<T1, T2>(Action<T1, T2> action, T1 param1, IEnumerable<T2> params2);
        ITasker Queue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2);
        ITasker ThenRunSync<T1, T2>(Action<T1, T2> action, T1 param, T2 param2);

        void ClearTasks();
    }
}
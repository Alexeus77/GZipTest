using System;
using System.Collections.Generic;

namespace GZipTest.Tasks
{
    public interface ITasker
    {
       
        ITasker StartAsync();
        void StartSequential();
        void WaitAll();
        ITasker ThenQueueForEach<T>(Action<T, Action> action, IEnumerable<T> objects);
        ITasker Queue<T>(Action<T, Action> action, T param1);
        ITasker ThenRunSync<T>(Action<T, Action> action, T param);
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GZipTest.Tasks
{

    public partial class Tasker : ITasker
    {
        private readonly LinkedList<ITask> taskQueue = new LinkedList<ITask>();

        public virtual ITasker StartAsync()
        {
            foreach (ITask task in taskQueue)
                task.Start();

            return this as ITasker;
        }

        public ITasker StartSequential()
        {
            foreach (ITask task in taskQueue)
                task.StartSync();

            return this as ITasker;
        }

        void SignalError()
        {
            foreach (ITask task in taskQueue)
                task.FinishedEvent.Set();
        }

        void ITasker.WaitAll()
        {
            foreach (ITask task in taskQueue)
                task.FinishedEvent.WaitOne();

            List<Exception> exceptions = new List<Exception>();

            foreach (ITask task in taskQueue)
                if (task.Exception != null)
                    exceptions.Add(task.Exception);

            if (exceptions.Count > 0)
            {
                TraceExceptions(exceptions);
                throw new TaskerAggregateException("Tasks processing exception. See innerException for details.", exceptions);
            }


        }

        [Conditional("DEBUG")]
        void TraceExceptions(List<Exception> exceptions)
        {
            foreach (var e in exceptions)
            {
                Debug.WriteLine(e.Source);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        ITasker ITasker.ThenQueueForEach<T1, T2>(Action<T1, T2> action, T1 param1, IEnumerable<T2> params2)
        {
            var previousTasks = taskQueue.ToArray();
            var taskNum = 1;

            foreach (var obj in params2)
            {
                Queue(action, param1,  obj, previousTasks, $"{action.Method.Name}#{taskNum++}");
            }

            return this as ITasker;
        }

        ITasker ITasker.Queue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            Queue(action, param1, param2, taskQueue.ToArray());
            return this as ITasker;
        }

        ITasker ITasker.ThenRunSync<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            (this as ITasker).Queue(action, param1, param2);

            taskQueue.Last.Value.StartSync();

            return this as ITasker;
        }

        private ITasker Queue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, ITask[] previousTasks, string name = null)
        {
            var task = new Task<T1, T2>(action, param1, param2)
            {
                Name = name ?? action.Method.Name
            };
            var node = taskQueue.AddLast(task);
            if (previousTasks.Length > 0)
                task.PreviousTaskIsCompleted = () => previousTasks.All(x => x.FinishedFlag);

            task.SignalError = SignalError;

            return this as ITasker;
        }

        public void ClearTasks()
        {
            taskQueue.Clear();
        }
    }
}

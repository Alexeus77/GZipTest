
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

        ITasker ITasker.StartAsync()
        {
            foreach (ITask task in taskQueue)
                task.Start();

            return this as ITasker;
        }

        void ITasker.StartSequential()
        {
            foreach (ITask task in taskQueue)
                task.StartSync();

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

        ITasker ITasker.ThenQueueForEach<T>(Action<T, Action> action, IEnumerable<T> objects)
        {
            var previousTasks = taskQueue.ToArray();
            var taskNum = 1;

            foreach (var obj in objects)
            {
                Queue(action, obj, previousTasks, $"{action.Method.Name}#{taskNum++}");
            }

            return this as ITasker;
        }

        ITasker ITasker.Queue<T>(Action<T, Action> action, T param)
        {
            Queue(action, param, taskQueue.ToArray());
            return this as ITasker;
        }

        private ITasker Queue<T>(Action<T, Action> action, T param, ITask[] previousTasks, string name = null)
        {
            var task = new Task<T>(action, param)
            {
                Name = name ?? action.Method.Name
            };
            var node = taskQueue.AddLast(task);
            if (previousTasks.Length > 0)
                task.PreviousTaskIsCompleted = () => previousTasks.All(x => x.FinishedFlag);
                
            task.SignalError = SignalError;

            return this as ITasker;
        }

        ITasker ITasker.ThenRunSync<T>(Action<T, Action> action, T param)
        {
            (this as ITasker).Queue(action, param);

            taskQueue.Last.Value.StartSync();

            return this as ITasker;
        }

    }
}

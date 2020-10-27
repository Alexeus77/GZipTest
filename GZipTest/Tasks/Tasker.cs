
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GZipTest.Tasks
{

    public partial class Tasker : ITasker
    {
        LinkedList<ITask> taskQueue = new LinkedList<ITask>();

        ITasker ITasker.ThenQueueForEach<T1, T2>(Action<T1, T2, Action> action1, IEnumerable<T1> objects, T2 param2, Action<T1> continueWith)
        {
            
            var previousTask = taskQueue.Last?.Value;
            var taskNum = 1;

            foreach (var obj in objects)
            {
                Queue(action1, obj, param2, previousTask, $"{action1.Method.Name}#{taskNum++}");
            }
             
            return this as ITasker;
        }

        private ITasker Queue<T1, T2>(Action<T1, T2, Action> action, T1 param1, T2 param2, ITask previous, string name = null)
        {
            var task = new Task<T1, T2>(action, param1, param2);
            task.Name = name == null ? action.Method.Name : name;
            var node = taskQueue.AddLast(task);
            if (previous != null)
            {
                task.PreviousTaskIsCompleted = previous.Finished;
                task.CanLoop = previous.WaitLoopCompleted;
            }
            task.SignalError = SignalError;

            return this as ITasker;
        }

        ITasker ITasker.Queue<T1, T2>(Action<T1, T2, Action> action, T1 param1, T2 param2)
        {
            Queue(action, param1, param2, taskQueue.Last?.Value);
            return this as ITasker;
        }
               

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

       
    }
}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GZipTest.Tasks
{

    public partial class Tasker : ITasker
    {
        LinkedList<ITask> taskQueue = new LinkedList<ITask>();

        public ITasker Run<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            Task<T1, T2> task = new Task<T1, T2>(action, param1, param2);
            taskQueue.AddFirst(task);
            task.SignalError = SignalError;

            return this as ITasker;
        }

        ITasker ITasker.ThenRun<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            Task<T1, T2> task = new Task<T1, T2>(action, param1, param2);
            task.SignalError = SignalError;

            var node = taskQueue.AddLast(task);
            task.PreviousFinished = node.Previous.Value.Finished;

            return this as ITasker;
        }

        ITasker ITasker.ThenRunWithContinue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2,
            Action<T1, T2> continueWith)
        {
            Task<T1, T2> task = new Task<T1, T2>(action, param1, param2, continueWith);
            var node = taskQueue.AddLast(task);
            task.PreviousFinished = node.Previous.Value.Finished;
            task.SignalError = SignalError;

            return this as ITasker;
        }


        ITasker ITasker.Start()
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

        public bool SuspendAction()
        {
            Thread.Sleep(50);
            return !previousFinishedForSuspend();
        }

        Func<bool> previousFinishedForSuspend = () => { return true; };

        public ITasker ThenRunForEach<T1, T2>(IEnumerable<T1> objects, Action<T1, T2> action1, Action<T1> continueWith, 
            Func<bool> suspendOuterAction, T2 param2)
        {
            Func<bool> previousFinished = () => { return true; };
            if (taskQueue.Count > 0)
                previousFinished = taskQueue.Last.Value.Finished;

            
            previousFinishedForSuspend = previousFinished;

            int taskNum = 1;

            foreach (var obj in objects)
            {
                Task<T1, T2> task = new Task<T1, T2>(action1, obj, param2, continueWith);
                task.Name = $"{action1.Method.Name}#{taskNum++}";

                var node = taskQueue.AddLast(task);
                if(taskNum > 1)
                    task.Finished = 
                        () => 
                        { return task.FinishedFlag && node.Previous.Value.Finished(); };
                task.PreviousFinished = previousFinished;
                task.SignalError = SignalError;
            }

            return this as ITasker;
        }
    }
}

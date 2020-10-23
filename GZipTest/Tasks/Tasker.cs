
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GZipTest.Tasks
{

    public partial class Tasker : ITasker
    {
        LinkedList<ITask> taskQueue = new LinkedList<ITask>();

        public ITasker Queue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            return (this as ITasker).ThenQueueWithContinue(action, param1, param2, null);
        }

        ITasker ITasker.ThenQueue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            return (this as ITasker).ThenQueueWithContinue(action, param1, param2, null);
        }

        ITasker ITasker.ThenQueueWithContinue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2,
            Action<T1, T2> continueWith)
        {
            Task<T1, T2> task = new Task<T1, T2>(action, param1, param2, continueWith);
            var node = taskQueue.AddLast(task);
            if (node.Previous != null)
            {
                task.PreviousTaskIsCompleted = node.Previous.Value.Finished;
                task.CanLoop = node.Previous.Value.WaitActionCompleted;
            }
            task.SignalError = SignalError;

            return this as ITasker;
        }

        ITasker ITasker.ThenRunWithContinueSync<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2,
            Action<T1, T2> continueWith)
        {
            (this as ITasker).ThenQueueWithContinue(action, param1, param2, continueWith);

            taskQueue.Last.Value.StartSync();

            return this as ITasker;
        }

        ITasker ITasker.ThenRunSync<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            return (this as ITasker).ThenRunWithContinueSync(action, param1, param2, null);
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

        AutoResetEvent suspendBuffer = new AutoResetEvent(false);


        public static bool SuspendAction(int sleepMiliSeconds)
        {
            if (!previousFinishedForSuspend())
            {
                Thread.Sleep(sleepMiliSeconds);
                return true;
            }

            return false;
        }

        static Func<bool> previousFinishedForSuspend = () => { return true; };

        ITasker ITasker.ThenQueueForEach<T1, T2>(IEnumerable<T1> objects, Action<T1, T2> action1, Action<T1> continueWith, 
            Func<int, bool> suspendOuterAction, T2 param2)
        {
            Func<bool> previousFinished = () => { return true; };
            if (taskQueue.Count > 0)
                previousFinished = taskQueue.Last.Value.Finished;

            
            previousFinishedForSuspend = previousFinished;

            int taskNum = 1;

            foreach (var obj in objects)
            {
                var task = new Task<T1, T2>(action1, obj, param2, continueWith);
                task.Name = $"{action1.Method.Name}#{taskNum++}";

                var node = taskQueue.AddLast(task);
                if(taskNum > 1)
                    task.Finished = 
                        () => 
                        { return task.FinishedFlag && node.Previous.Value.Finished(); };
                task.PreviousTaskIsCompleted = previousFinished;
                task.SignalError = SignalError;
            }

            return this as ITasker;
        }
    }
}

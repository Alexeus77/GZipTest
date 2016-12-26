
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GZipTest.Tasks
{

    public partial class Tasker : ITasker
    {
        LinkedList<ITask> taskQueue = new LinkedList<ITask>();
        
        public ITasker Run<T1, T2>(Func<T1, T2, bool> action, T1 param1, T2 param2,
            Func<bool> suspendCondition)
        {
            Task<T1, T2> task = new Task<T1, T2>(action, param1, param2, suspendCondition);
            taskQueue.AddFirst(task);
            task.SignalError = SignalError;

            return this as ITasker;
        }

        ITasker ITasker.ThenRun<T1, T2>(Func<T1, T2, bool> action, T1 param1, T2 param2,
            Func<bool> suspendCondition)
        {
            return (this as ITasker).ThenRunWithContinue(action, param1, param2, suspendCondition, null);
        }

        ITasker ITasker.ThenRunWithContinue<T1, T2>(Func<T1, T2, bool> action, T1 param1, T2 param2,
            Func<bool> suspendCondition, Action continueWith)
        {
            Task<T1, T2> task = new Task<T1, T2>(action, param1, param2, suspendCondition, continueWith);
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

        
    }
}

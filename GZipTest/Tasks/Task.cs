using System;
using System.Threading;

namespace GZipTest.Tasks
{
    public partial class Tasker
    {
        private class Task<T1, T2> : ITask
        {
            Func<T1, T2, bool> _action;
            Func<bool> _previousFinished;
            T1 _param1;
            T2 _param2;
            bool _finished;


            public ManualResetEvent FinishedEvent { get; } = new ManualResetEvent(false);
            public Exception Exception { get; private set; }

            public Action SignalError { get; set; }
            private Func<bool> SuspendCondition { get; set; }
            private Action ContinueWith { get; set; }
 

            public bool Finished() { return _finished;  }

            public Func<bool> PreviousFinished
            {
                private get
                {
                    return _previousFinished;
                }

                set
                {
                    _previousFinished = value;
                }
            }

            public Task(Func<T1, T2, bool> action, T1 param1, T2 param2,
                Func<bool> suspendCondition, Action continueWith = null)
            {
                _action = action;
                _param1 = param1;
                _param2 = param2;
                SuspendCondition = suspendCondition;
                ContinueWith = continueWith;
            }

            public void Start()
            {
                Thread thread = new Thread(this.DoWork);
                thread.IsBackground = true;
                thread.Name = _action.Method.Name;
                thread.Start();
            }

            public void StartSync()
            {
                _action.Invoke(_param1, _param2);
            }

            private void DoWork()
            {
                try
                {
                    bool finished = false;
                    while (!finished || PreviousFinished != null && !PreviousFinished())
                    {
                        finished = _action(_param1, _param2) && (PreviousFinished == null || PreviousFinished());
                        if (SuspendCondition != null && SuspendCondition())
                            Thread.Sleep(100);
                    }

                    ContinueWith?.Invoke();
                }
                catch (Exception exception)
                {
                    exception.Source = _action.Method.Name + ": " + exception.Source;
                    this.Exception = exception;
                }
                finally
                {
                    _finished = true;

                    if (Exception != null)
                        try { SignalError(); }
                        catch { } //ignore possible error in callback;

                    
                    FinishedEvent.Set();
                }
            }
        }
    }
}

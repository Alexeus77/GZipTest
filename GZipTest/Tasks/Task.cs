using System;
using System.Threading;

namespace GZipTest.Tasks
{
    public partial class Tasker
    {
        private class Task<T1, T2> : ITask
        {
            Action<T1, T2> _action;
            Func<bool> _previousFinished;
            T1 _param1;
            T2 _param2;
            bool _finished;
            
            public ManualResetEvent FinishedEvent { get; } = new ManualResetEvent(false);
            public Exception Exception { get; private set; }
            public Action SignalError { get; set; }
            private Action<T1> ContinueWith { get; set; }

            public bool Finished() { return _finished;  }

            public string Name { get; set; }

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

            public Task(Action<T1, T2> action, T1 param1, T2 param2,
                Action<T1> continueWith = null)
            {
                _action = action;
                _param1 = param1;
                _param2 = param2;
                ContinueWith = continueWith;
            }

            public void Start()
            {
                Thread thread = new Thread(this.DoWork);
                thread.IsBackground = true;
                thread.Name =  Name ?? _action.Method.Name;
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
                    //iterate action while previous action in chain is not completed
                    do
                    {
                        _action(_param1, _param2);
                    } while (PreviousFinished != null && !PreviousFinished());

                    //invoke additional action if specified
                    ContinueWith?.Invoke(_param1);
                }
                catch (Exception exception)
                {
                    exception.Source = $"{_action.Method.Name}: {exception.Source}. Thread: {Thread.CurrentThread.ManagedThreadId}";
                    this.Exception = exception;
                }
                finally
                {
                    _finished = true;

                    if (Exception != null)
                        //signal error to for other threads in chain
                        try { SignalError(); }
                        catch { } //ignore possible error in callback;

                    //setup completed for awaiting procedure
                    FinishedEvent.Set();
                }
            }
        }
    }
}

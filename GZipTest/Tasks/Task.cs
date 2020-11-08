using System;
using System.Threading;

namespace GZipTest.Tasks
{
    public partial class Tasker
    {
        private class Task<T1, T2> : ITask
        {
            
            private readonly Action<T1, T2> _action;
            private readonly T1 _param1;
            private readonly T2 _param2;
            volatile bool _finished;
            private readonly object _lockObject = new object();
            private readonly Thread _thread;
            
            public string Name { get; set; } = null;
            public ManualResetEvent FinishedEvent { get; } = new ManualResetEvent(false);
            public Exception Exception { get; private set; }
            public Action SignalError { get; set; }
            public Func<bool> Finished { get; set; }
            public bool FinishedFlag
            {
                get
                {
                    lock (_lockObject) { return _finished; }
                }
                private set
                {
                    lock (_lockObject) { _finished = value; }
                }
            }

            public Func<bool> PreviousTaskIsCompleted { get; set; } = () => { return true; };

            private Task()
            {
                Finished = () => { return FinishedFlag; };
            }

            public Task(Action<T1, T2> action, T1 param1, T2 param2) : this()
            {
                _action = action;
                _param1 = param1;
                _param2 = param2;
                _thread = new Thread(this.DoWork)
                {
                    IsBackground = true
                };
            }

            public void Start()
            {
                _thread.Name = Name ?? _action.Method.Name;
                _thread.Start();
            }

            public void StartSync()
            {
                DoWork();
            }

            private void DoWork()
            {
                try
                {
                    //iterate action while previous action in chain is not completed
                    do
                    {
                        _action(_param1, _param2);

                    } while (!PreviousTaskIsCompleted());

                    _action(_param1, _param2);
                }

                catch (Exception exception)
                {
                    exception.Source = $"{Name}: {exception.Source}. Thread: {Thread.CurrentThread.Name}";
                    this.Exception = exception;
                }
                finally
                {
                    FinishedFlag = true;

                    if (Exception != null)
                        //signal error to for other threads in chain
                        try { SignalError(); }
                        catch { } //ignore possible error in callback;

                    //set completed flag
                    FinishedEvent.Set();
                }
            }

           
        }

        
    }
}

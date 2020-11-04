using System;
using System.Threading;

namespace GZipTest.Tasks
{
    public partial class Tasker
    {
        private class Task<T> : ITask
        {
            
            private readonly Action<T, Action> _action;
            private readonly T _param;

            volatile bool _finished;
            private readonly object _lockObject = new object();
            private readonly Thread _thread;
            private readonly AutoResetEvent actionCompletedEvent = new AutoResetEvent(false);

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


            public Func<bool> CanLoop { get; set; } = () => { return true; };
            public Func<bool> PreviousTaskIsCompleted { get; set; } = () => { return true; };

            

            public Func<bool> WaitLoopCompleted
            {
                get
                {
                    return () =>
                    {
                        actionCompletedEvent.WaitOne(500);
                        return true;
                    };
                }
            }

            private void SignalLoopCompleted()
            {
                actionCompletedEvent.Set();
            }
                

            private Task()
            {
                Finished = () => { return FinishedFlag; };
            }

            public Task(Action<T, Action> action, T param) : this()
            {
                _action = action;
                _param = param;

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
                    while(CanLoop())
                    {
                        _action(_param, SignalLoopCompleted);
                        if (PreviousTaskIsCompleted())
                        {
                            _action(_param, SignalLoopCompleted);
                            break;
                        }
                    } 

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

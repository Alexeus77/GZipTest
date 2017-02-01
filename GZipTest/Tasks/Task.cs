using System;
using System.Threading;

namespace GZipTest.Tasks
{
    public partial class Tasker
    {
        private class Task<T1, T2> : ITask
        {
            Action<T1, T2> _action;
            T1 _param1;
            T2 _param2;
            volatile bool _finished;
            object _lockObject = new object();

            public string Name { get; set; } = null;
            public ManualResetEvent FinishedEvent { get; } = new ManualResetEvent(false);
            public Exception Exception { get; private set; }
            public Action SignalError { get; set; }
            private Action<T1> ContinueWith { get; set; }
            private Action<T1, T2> ContinueWith2 { get; set; }
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



            public Func<bool> PreviousFinished { get; set; } = () => { return true; };
            
            private Task()
            {
                Finished = () => { return FinishedFlag; };
            }

            Thread _thread;

            private Task(Action<T1, T2> action, T1 param1, T2 param2, Thread thread)
            {
                Finished = () => { return FinishedFlag; };
            }

            public Task(Action<T1, T2> action, T1 param1, T2 param2) : this()
            {
                _action = action;
                _param1 = param1;
                _param2 = param2;
                
                _thread = new Thread(this.DoWork);
                _thread.IsBackground = true;
            }

            public Task(Action<T1, T2> action, T1 param1, T2 param2,
                Action<T1> continueWith = null) : this(action, param1, param2)
            {
                ContinueWith = continueWith;
            }

            public Task(Action<T1, T2> action, T1 param1, T2 param2,
                Action<T1, T2> continueWith = null) : this(action, param1, param2)
            {
                ContinueWith2 = continueWith;
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
                    } while (!PreviousFinished() && DoSuspend());

                   // _action(_param1, _param2);

                    //invoke additional action if specified
                    ContinueWith?.Invoke(_param1);
                    ContinueWith2?.Invoke(_param1, _param2);
                }
                catch (Exception exception)
                {
                    exception.Source = $"{Name}: {exception.Source}. Thread: {Thread.CurrentThread.ManagedThreadId}";
                    this.Exception = exception;
                }
                finally
                {
                    FinishedFlag = true;

                    if (Exception != null)
                        //signal error to for other threads in chain
                        try { SignalError(); }
                        catch { } //ignore possible error in callback;

                    //setup completed for awaiting procedure
                    FinishedEvent.Set();
                }
            }

            private bool DoSuspend()
            {
                Thread.Sleep(100);
                return true;
            }
        }

        
    }
}

using System;
using System.Threading;

namespace ThreadPool
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        public bool IsCompleted => Status == MyTaskStatus.Completed;
        public bool IsFaulted => Status == MyTaskStatus.Faulted;

        public TResult Result
        {
            get
            {
                while (!IsCompleted && !IsFaulted)
                {
                }

                if (IsFaulted)
                {
                    throw new AggregateException(_exception);
                }

                return _result;
            }
        }

        public MyTaskStatus Status => _status;
        public bool HasContinuation => Continuation != null;
        public IMyTask? Continuation { get; private set; }

        private readonly Func<TResult> _action;
        private TResult _result = default!;
        private volatile MyTaskStatus _status;
        private Exception _exception = null!;

        public MyTask(Func<TResult> action)
        {
            _action = action;
            _status = MyTaskStatus.Created;
        }

        public void Run()
        {
            try
            {
                _status = MyTaskStatus.Running;
                _result = _action.Invoke();
                _status = MyTaskStatus.Completed;
            }
            catch (Exception e)
            {
                _status = MyTaskStatus.Faulted;
                _exception = e;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
        {
            var nextTask = new MyTask<TNewResult>(() => continuation.Invoke(Result));
            Continuation = nextTask;
            return nextTask;
        }
    }
}
using System;
using System.Threading;

namespace Homework1
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private Func<TResult> _func;
        private IMyTask _continuation;
        private TResult _result;
        private Exception _exception;
        private volatile MyTaskState _state = MyTaskState.CREATED;

        public TResult Result
        {
            get
            {
                while (_state == MyTaskState.CREATED || _state == MyTaskState.RUNNING)
                {
                    // wait until completed
                    Thread.Sleep(10);
                }

                if (_state == MyTaskState.FAILED)
                {
                    throw new AggregateException(_exception);
                }

                return _result;
            }
        }

        public bool IsCompleted => _state == MyTaskState.COMPLETED;

        public MyTaskState State => _state;

        public bool HasContinuation => _continuation != null;

        public IMyTask Continuation => _continuation;

        public MyTask(Func<TResult> func)
        {
            _func = func;
        }

        public void Execute()
        {
            try
            {
                _state = MyTaskState.RUNNING;
                _result = _func();
            }
            catch (Exception exception)
            {
                _exception = exception;
                _state = MyTaskState.FAILED;
                return;
            }

            _state = MyTaskState.COMPLETED;
        }
        
        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
        {
            var continuation = new MyTask<TNewResult>(() => func(Result));
            _continuation = continuation;
            return continuation;
        }
    }
}

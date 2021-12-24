using System;

namespace MyThreading {
    public class MyTask<TResult> : IMyTask<TResult> {
        private Exception _taskExecutionException;
        private Func<TResult> _taskFunc;

        public bool HasContinuationTask => ContinuationTask != null;
        private bool HasFailed => _taskExecutionException != null;

        public bool IsCompleted => _isCompleted;
        private bool _isCompleted;

        public TResult Result {
            get {
                while (!_isCompleted && !HasFailed) { }

                if (HasFailed) {
                    throw new AggregateException(_taskExecutionException);
                }

                return _result;
            }
        }
        private TResult _result;

        public IMyTask ContinuationTask => _continuationTask;
        private IMyTask _continuationTask;

        public MyTask(Func<TResult> taskFunc) {
            _taskFunc = taskFunc;
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation) {
            var nextTask = new MyTask<TNewResult>(() => continuation.Invoke(Result));
            _continuationTask = nextTask;
            return nextTask;
        }

        public void Execute() {
            try {
                _result = _taskFunc.Invoke();
                _isCompleted = true;
            }
            catch (Exception ex) {
                _taskExecutionException = ex;
            }
        }
    }
}

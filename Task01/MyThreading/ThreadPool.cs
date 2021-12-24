using System;
using System.Collections.Generic;
using System.Threading;

namespace MyThreading {
    public class ThreadPool: IDisposable {
        #region Fields

        private object _lockObj = new object();
        private Queue<Thread> _threads;
        private Queue<IMyTask> _tasks;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;

        #endregion

        public ThreadPool(int threadsNum) {
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            _threads = new Queue<Thread>();
            _tasks = new Queue<IMyTask>();
            threadsNum = threadsNum < 1 ? 1 : threadsNum;

            for (int i=0; i < threadsNum; i++) {
                var thread = new Thread(() => StartThreadExecuting());

                _threads.Enqueue(thread);
                thread.Start();
            }
        }

        public void Enqueue(IMyTask taskToAdd) {
            if (taskToAdd == null) {
                throw new ArgumentNullException("You cannot add null as a task");
            }
            
            lock (_lockObj) {
                if (_cancelToken.IsCancellationRequested) {
                    throw new ObjectDisposedException("ThreadPool is disposed");
                }

                _tasks.Enqueue(taskToAdd);
                Monitor.PulseAll(_lockObj);
            }
        }

        private void StartThreadExecuting() {
            IMyTask currTask;
            
            while (true) {
                lock (_lockObj) {
                    
                    while (_tasks.Count == 0) {
                        if (_cancelToken.IsCancellationRequested) {
                            return;
                        }

                        Monitor.Wait(_lockObj);
                    }
                    
                    currTask = _tasks.Dequeue();
                    currTask.Execute();

                    if (currTask.HasContinuationTask) {
                        _tasks.Enqueue(currTask.ContinuationTask);
                    }
                }
            }
        }

        private void Dispose(bool disposing) {
            lock (_lockObj) {
                if (!_cancelToken.IsCancellationRequested) {
                    if (disposing) {
                        Monitor.PulseAll(_lockObj);
                    }

                    _cancelTokenSource.Cancel();
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ThreadPool() {
            Dispose(false);
        }
    }
}

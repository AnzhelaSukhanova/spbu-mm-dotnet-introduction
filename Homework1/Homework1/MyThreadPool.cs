using System;
using System.Collections.Generic;
using System.Threading;

namespace Homework1
{
    public class MyThreadPool : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Queue<IMyTask> _tasksQueue = new Queue<IMyTask>();
        private bool _disposed;
        private int _maxThreadsNum;
        private int _waitingThreadsNum;
        private int _threadsNum;

        public MyThreadPool(int maxThreadsNum)
        {
            if (maxThreadsNum < 1)
            {
                throw new ArgumentOutOfRangeException("Number of threads must not be less than 1");
            }

            _maxThreadsNum = maxThreadsNum;
        }

        public int MaxThreadsNum
        {
            get => _maxThreadsNum;
        }

        public int ThreadsNum
        {
            get => _threadsNum;
        }

        public void Enqueue(IMyTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("Task must be not null");
            }

            lock (_lock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("ThreadPool disposed");
                }

                if (_waitingThreadsNum == 0 && _threadsNum < _maxThreadsNum)
                {
                    _threadsNum++;
                    var newThread = new Thread(() => _LaunchThread(task));
                    newThread.Start();
                }
                else
                {
                    _tasksQueue.Enqueue(task);
                    Monitor.Pulse(_lock);
                }
            }
        }

        private void _LaunchThread(IMyTask initialTask)
        {
            var currTask = initialTask;

            while (true)
            {
                currTask.Execute();

                if (currTask.HasContinuation)
                {
                    Enqueue(currTask.Continuation);
                }

                lock (_lock)
                {
                    if (_tasksQueue.Count != 0)
                    {
                        currTask = _tasksQueue.Dequeue();
                    }
                    else
                    {
                        _waitingThreadsNum++;

                        while (_tasksQueue.Count == 0)
                        {
                            if (_disposed)
                            {
                                return;
                            }

                            Monitor.Wait(_lock);
                        }

                        _waitingThreadsNum--;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        if (_waitingThreadsNum > 0)
                        {
                            Monitor.PulseAll(_lock);
                        }
                    }

                    _disposed = true;
                }
            }
        }

        ~MyThreadPool()
        {
            Dispose(false);
        }
    }
}

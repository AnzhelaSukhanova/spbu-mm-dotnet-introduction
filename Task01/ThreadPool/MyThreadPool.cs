using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
    public class MyThreadPool : IDisposable
    {
        public int MaxThreadsNum { get; }
        private readonly Queue<IMyTask> _tasks = new();
        private readonly Object _lock = new();
        private CancellationToken? _disposed;
        private readonly List<Thread> _threads = new();

        public int ActiveThreads
        {
            get
            {
                return _threads.FindAll((thread) =>
                    thread.ThreadState != ThreadState.WaitSleepJoin
                    && thread.ThreadState != ThreadState.Suspended
                ).Count;
            }
        }

        public MyThreadPool(int maxThreadsNum)
        {
            MaxThreadsNum = maxThreadsNum;
            for (int i = 0; i < MaxThreadsNum; ++i)
            {
                var thread = new Thread(() =>
                {
                    while (true)
                    {
                        IMyTask task;
                        lock (_lock)
                        {
                            while (_tasks.Count == 0)
                            {
                                if (_disposed != null) return;
                                Monitor.Wait(_lock);
                            }

                            task = _tasks.Dequeue();
                        }

                        task.Run();
                        if (task.HasContinuation)
                        {
                            Enqueue(task.Continuation!);
                        }
                    }
                });
                _threads.Add(thread);
                thread.Start();
            }
        }

        public void Enqueue(IMyTask task)
        {
            lock (_lock)
            {
                if (_disposed != null)
                {
                    throw new ObjectDisposedException("This ThreadPool is already disposed.");
                }

                _tasks.Enqueue(task);
                Monitor.PulseAll(_lock);
            }
        }

        private void ReleaseUnmanagedResources()
        {
            lock (_lock)
            {
                _disposed = new CancellationToken();
                Monitor.PulseAll(_lock);
            }
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~MyThreadPool()
        {
            ReleaseUnmanagedResources();
        }
    }
}
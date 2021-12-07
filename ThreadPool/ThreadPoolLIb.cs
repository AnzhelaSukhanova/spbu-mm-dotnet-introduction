using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;


namespace ThreadPoolLib
{
    public static class ThreadPoolConstants
    {
        public const int ThreadsNum = 4;
        public const int MaxWaitTimeMs = (int)1e7;
    }
    public class ThreadPool : IDisposable
    {
        private readonly int _maxWaitTime;
        private readonly List<Thread> _freeThreads;
        private readonly List<Thread> _busyThreads;
        private readonly List<ITask>_taskList;
        private readonly List<int> _completedTasksId;

        public ThreadPool(int threadNum = ThreadPoolConstants.ThreadsNum,
            int maxWaitTime = ThreadPoolConstants.MaxWaitTimeMs)
        {
            Debug.Assert(threadNum > 0, 
                "The number of threads must be greater than 0");
            _maxWaitTime = maxWaitTime;

            _freeThreads = new List<Thread>();
            _busyThreads = new List<Thread>();
            _taskList = new List<ITask>();
            _completedTasksId = new List<int>();
            
            for (int i = 0; i < threadNum; i++)
            {
                _freeThreads.Add(new Thread(ThreadState.Free));
            }

            var taskExecutor = new BackgroundWorker();
            taskExecutor.WorkerSupportsCancellation = true;
            taskExecutor.DoWork += delegate
            {
                while (_taskList.Count == 0) { }
                Execute();
            };
            taskExecutor.RunWorkerAsync();
        }

        private void Execute()
        {
            var timer = new Stopwatch();
            while (true)
            {
                ITask task;
                lock (_taskList) 
                {
                    if (_taskList.Count == 0)
                        continue;
                    task = _taskList[0];
                    _taskList.Remove(task);
                }

                timer.Start();
                while (_freeThreads.Count == 0)
                {
                    if (TaskOptions.Token != null)
                        task.Cancel();
                    if (task.IsCancelled)
                        continue;
                    Debug.Assert(timer.ElapsedMilliseconds <= _maxWaitTime,
                        "Task execution time limit exceeded");
                }
                timer.Stop();
                Thread currentThread;
                lock (_busyThreads) lock(_freeThreads) lock(_taskList)
                {
                    currentThread = _freeThreads[0];
                    _freeThreads.RemoveAt(0);
                    _busyThreads.Add(currentThread);
                }
                BackgroundWorker thread = new BackgroundWorker();
                thread.DoWork += delegate
                {
                    currentThread.Execute(this, task);
                };
                thread.RunWorkerAsync();
            }
        }

        public void Enqueue<TResult>(IMyTask<TResult> task)
        {
            Debug.Assert(task != null, "The task is not initialized");
            if (task.IsCompleted)
                return;
            if (TaskOptions.Token != null)
                return;
            lock (_taskList)
            {
                _taskList.Add(task);
            }
        }

        public int CountThreads()
        {
            int number = 0;
            lock (_busyThreads)
            lock (_freeThreads)
            {
                number += _busyThreads.Count + _freeThreads.Count;
            }
            return number;
        }

        public void Dispose()
        {
            TaskOptions.Token = new CancellationToken();
            while(_busyThreads.Count != 0) {}
            for (int i = 0; i < _freeThreads.Count; i++)
                _freeThreads.RemoveAt(0);
            for (int i = 0; i < _taskList.Count; i++)
                _taskList.RemoveAt(0);
            TaskOptions.Token = null;
        }

        public void Shutdown()
        {
            Dispose();
        }

        private enum ThreadState
        {
            Busy,
            Free
        }

        private class Thread
        {
            ThreadState _state;

            public Thread(ThreadState state)
            {
                _state = state;
            }

            private void Take()
            {
                
                Debug.Assert(_state == ThreadState.Free, 
                    "The thread is already busy");
                _state = ThreadState.Busy;
            }
            
            private void Free()
            {
                Debug.Assert(_state == ThreadState.Busy,
                    "The thread is already free");
                _state = ThreadState.Free;
            }

            public void Execute(ThreadPool pool, ITask task)
            {
                Take();
                var prevTaskId = task.PrevTaskId;
                if (prevTaskId != -1)
                {
                    if (!pool._completedTasksId.Contains(prevTaskId))
                    {
                        Free();
                        lock (pool._busyThreads)
                        lock(pool._freeThreads) 
                        lock(pool._taskList)
                        {
                            pool._taskList.Add(task);
                            pool._busyThreads.RemoveAt(0);
                            pool._freeThreads.Add(this);
                        }
                        return;
                    }
                }

                if (!task.IsCancelled)
                {
                    task.Execute();
                    Free();
                    lock (pool._busyThreads)
                    lock(pool._freeThreads)
                    lock(pool._completedTasksId)
                    {
                        pool._busyThreads.RemoveAt(0);
                        pool._freeThreads.Add(this);
                        pool._completedTasksId.Add(task.Id);
                    }
                }
            }
        }
        
        public class CancellationToken {}
    }

    public interface ITask
    {
        int Id { get; }
        bool IsCancelled { get; }
        void Cancel();
        int PrevTaskId { get; }
        void Execute();
    }

    public interface IMyTask<out TResult> : ITask
    {
        bool IsCompleted { get; }
        TResult Result { get; }
        IMyTask<TNewResult> ContinueWith<TNewResult>
            (Func<TResult, TNewResult> function);
    }
    
    internal static class TaskOptions
    {
        internal static ThreadPool.CancellationToken Token;
        internal static int LastId = -1;
        internal static readonly ArrayList Results = new();
    }
    
    internal enum TaskState
    {
        Cancelled,
        JustCreated,
        InProgress,
        Completed,
    }

    public class MyTask<TResult> : IMyTask<TResult>
    {
        private readonly int _id;
        private readonly Func<TResult> _function;
        private TResult _result;
        private TaskState _state;
        private Exception _exception;
        
        private readonly int _prevTaskId = -1;
        private readonly Func<object, TResult> _lazyFunction;

        public MyTask(Func<TResult> function = null, 
            Func<object, TResult> lazyFunction = null, int id = -1)
        {
            TaskOptions.LastId++;
            _id = TaskOptions.LastId;
            TaskOptions.Results.Add(default(TResult));
            if (TaskOptions.Token == null)
            {
                _state = TaskState.JustCreated;
                if (function != null)
                    _function = function;
                if (lazyFunction != null & id != -1)
                {
                    _lazyFunction = lazyFunction;
                    _prevTaskId = id;
                }
            }
            else
                Cancel();
        }

        public int Id => _id;

        public bool IsCompleted
        {
            get
            {
                if (_state == TaskState.Completed)
                    return true;
                return false;
            }
        }

        public bool IsCancelled
        {
            get
            {
                if (_state == TaskState.Cancelled)
                    return true;
                return false;
            }
        }

        public int PrevTaskId => _prevTaskId;

        public void Execute()
        {
            try
            {
                _state = TaskState.InProgress;
                if (_prevTaskId == -1)
                    _result = _function();
                else
                {
                    var prevResult = TaskOptions.Results[_prevTaskId];
                    _result = _lazyFunction(prevResult);
                }
                
            }
            catch (Exception e)
            {
                _exception = e;
            }
            _state = TaskState.Completed;
            TaskOptions.Results[_id] = _result;
        }

        public void Cancel()
        {
            _state = TaskState.Cancelled;
        }

        public TResult Result
        {
            get
            {
                if (_exception != null)
                    throw new AggregateException(_exception);
                Debug.Assert(!IsCancelled, 
                    "Trying to get the result of a canceled task");
                while (!IsCompleted) {}
                return _result;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>
            (Func<TResult, TNewResult> function)
        {
            MyTask<TNewResult> newTask;
            if (IsCompleted)
            {
                TNewResult NewFunction() => function(Result);
                newTask = new MyTask<TNewResult>(NewFunction);
            }
            else
            {
                TNewResult LazyFunction(object o)
                {
                    var param = (TResult) o;
                    return function(param);
                }
                newTask = new MyTask<TNewResult>(lazyFunction: LazyFunction, id: _id);
            }
            return newTask;
        }
    }
}

using System;

namespace ThreadPool
{
    public interface IMyTask
    {
        bool IsCompleted { get; }
        bool IsFaulted { get; }
        public void Run();
        public bool HasContinuation { get; }
        public IMyTask? Continuation { get; }
        MyTaskStatus Status { get; }
    }

    public interface IMyTask<out TResult> : IMyTask
    {
        TResult Result { get; }
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
    }
}
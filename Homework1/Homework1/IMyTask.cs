using System;

namespace Homework1
{
    public enum MyTaskState
    {
        CREATED,
        RUNNING,
        COMPLETED,
        FAILED
    }

    public interface IMyTask
    {
        bool IsCompleted{ get; }

        bool HasContinuation { get; }

        IMyTask Continuation { get; }

        MyTaskState State { get; }

        void Execute();
    }

    public interface IMyTask<TResult> : IMyTask
    {
        TResult Result { get; }

        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
    }
}

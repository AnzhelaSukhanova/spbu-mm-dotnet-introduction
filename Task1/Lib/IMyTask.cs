using System;

namespace Lib
{
    public interface IMyTask<R>
    {
        bool IsCompleted { get; }
        R Result { get; }
        IMyTask<R, NR> ContinueWith<NR>(Func<R, NR> f);
    }

    public interface IMyTask<OR, R> : IMyTask<R>
    {
        IMyTask<OR> BasedOn { get; }
    }
}

using System;
using System.Threading;

namespace Lib
{
    public class SmpTask<R> : IMyTask<R>
    {
        private Func<R> F;
        private R RawResult;
        private Mutex ResultMutex;

        public bool IsCompleted { get; private set; }

        public SmpTask(Func<R> f)
        {
            F = f;
            IsCompleted = false;
            ResultMutex = new Mutex();
        }

        public R Result
        {
            get
            {
                if (!IsCompleted)
                {
                    ResultMutex.WaitOne();
                    if (IsCompleted)
                    {
                        return RawResult;
                    }
                    try
                    {
                        RawResult = F();
                    }
                    catch (Exception e)
                    {
                        throw new AggregateException(e);
                    }
                    IsCompleted = true;
                    ResultMutex.ReleaseMutex();
                }
                return RawResult;
            }
        }

        public IMyTask<R, NR> ContinueWith<NR>(Func<R, NR> f)
        {
            return new CntTask<R, NR>(f, this);
        }
    }

    public class CntTask<OR, R> : IMyTask<OR, R>
    {
        new private Func<OR, R> F;
        private R RawResult;
        private Mutex ResultMutex;

        public bool IsCompleted { get; private set; }

        public IMyTask<OR> BasedOn { get; private set; }

        public CntTask(Func<OR, R> f, IMyTask<OR> task)
        {
            F = f;
            IsCompleted = false;
            ResultMutex = new Mutex();
            BasedOn = task;
        }
        public R Result
        {
            get
            {
                if (!IsCompleted)
                {
                    ResultMutex.WaitOne();
                    if (IsCompleted)
                    {
                        return RawResult;
                    }
                    try
                    {
                        RawResult = F(BasedOn.Result);
                    }
                    catch (Exception e)
                    {
                        throw new AggregateException(e);
                    }
                    IsCompleted = true;
                    ResultMutex.ReleaseMutex();
                }
                return RawResult;
            }
        }

        public IMyTask<R, NR> ContinueWith<NR>(Func<R, NR> f)
        {
            return new CntTask<R, NR>(f, this);
        }
    }
}

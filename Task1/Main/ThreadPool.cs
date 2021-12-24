using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Main
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
                if (! IsCompleted)
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

    public class ThreadPool : IDisposable
    {
        private List<Thread> Threads;
        private Queue<IMyTask<object>> Tasks;
        private Mutex MainMutex;
        private SemaphoreSlim TasksIsEmptySemaphore;
        private CancellationToken Cancellation;
        private int ActiveThreadsCount;
        private Exception GeneratedException;
        private readonly bool IsVerbose;

        private class CancellationToken : Exception { }

        public int ThreadsCount { get { return Threads.Count; } }

        public ThreadPool(int threadsCount, bool isVerbose = true)
        {
            Tasks = new Queue<IMyTask<object>>();
            MainMutex = new Mutex();
            TasksIsEmptySemaphore = new SemaphoreSlim(0);
            ActiveThreadsCount = 0;
            IsVerbose = isVerbose;
            Threads = Enumerable.Range(0, threadsCount).Select((i) =>
                new Thread(() => {
                    Interlocked.Increment(ref ActiveThreadsCount);
                    IMyTask<object> task;
                    while (true)
                    {
                        MainMutex.WaitOne();
                        if (IsVerbose) { Console.WriteLine($"Thread #{i} >>>"); }
                        if (Tasks.Count == 0)
                        {
                            if (Cancellation != null)
                            {
                                if (IsVerbose) { Console.WriteLine($"Thread #{i} <<!"); }
                                MainMutex.ReleaseMutex();
                                break;
                            }
                            if (IsVerbose) { Console.WriteLine($"Thread #{i} <<w"); }
                            MainMutex.ReleaseMutex();
                            TasksIsEmptySemaphore.Wait();
                            continue;
                        }
                        task = Tasks.Dequeue();
                        if (IsVerbose) { Console.WriteLine($"Thread #{i} <<<"); }
                        MainMutex.ReleaseMutex();
                        try
                        {
                            var result = task.Result;
                            if (IsVerbose) { Console.WriteLine($"Thread #{i} has got the result {result}"); }
                        }
                        catch (Exception e)
                        {
                            if (IsVerbose) { Console.WriteLine($"Thread #{i} !!!"); }
                            MainMutex.WaitOne();
                            GeneratedException = e;
                            RawDispose();
                            MainMutex.ReleaseMutex();
                        }
                    }
                    Interlocked.Decrement(ref ActiveThreadsCount);
                })
            ).ToList();
            Threads.ForEach((thread) => thread.Start());
        }

        private void ThrowGeneratedException(bool ifReleaseMutex = true)
        {
            if (GeneratedException != null)
            {
                var e = GeneratedException;
                GeneratedException = null;
                if (ifReleaseMutex) { MainMutex.ReleaseMutex(); }
                throw e;
            }
        }

        public void Enqueue(IMyTask<object> task)
        {
            MainMutex.WaitOne();
            ThrowGeneratedException();
            if (IsVerbose) { Console.WriteLine("Enqueue >>>"); }
            if (Cancellation != null)
            {
                if (IsVerbose) { Console.WriteLine("Enqueue <<!"); }
                MainMutex.ReleaseMutex();
                return;
            }
            if (Tasks.Count == 0)
            {
                TasksIsEmptySemaphore.Release();
            }
            Tasks.Enqueue(task);
            if (IsVerbose) { Console.WriteLine("Enqueue <<<"); }
            MainMutex.ReleaseMutex();
        }

        private void RawDispose()
        {
            Cancellation = new CancellationToken();
            for (int i = 0; i < ThreadsCount; i++)
            {
                TasksIsEmptySemaphore.Release();
            }
        }

        public void Dispose()
        {
            MainMutex.WaitOne();
            ThrowGeneratedException();
            if (IsVerbose) { Console.WriteLine("Dispose >>>"); }
            RawDispose();
            if (IsVerbose) { Console.WriteLine("Dispose <<<"); }
            MainMutex.ReleaseMutex();
            GC.SuppressFinalize(this);
        }

        public bool IsStopped(int millisecondsTimeout)
        {
            var millisecondsStep = 200;
            while (ActiveThreadsCount != 0 && millisecondsTimeout > 0)
            {
                Thread.Sleep(millisecondsStep);
                millisecondsTimeout -= millisecondsStep;
            }
            ThrowGeneratedException(false);
            return ActiveThreadsCount == 0;
        }
    }

    public static class Program
    {
        public static void Main() { }
    }
}

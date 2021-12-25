using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Lib
{
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
}

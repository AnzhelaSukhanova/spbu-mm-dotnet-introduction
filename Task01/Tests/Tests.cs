using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ThreadPool;

namespace Tests
{
    public class Tests
    {
        private MyThreadPool _pool;

        [SetUp]
        public void Setup()
        {
            _pool = new MyThreadPool(4);
        }

        [Test]
        public void AddSingleTask()
        {
            var task1 = new MyTask<int>(() =>
            {
                Thread.Sleep(100);
                return 1 + 1;
            });
            _pool.Enqueue(task1);
            Assert.AreEqual(2, task1.Result);
        }

        [Test]
        public void AddManyTasks()
        {
            var tasks = new List<IMyTask<int>>();
            for (int i = 1; i <= 20; ++i)
            {
                var num = i;
                tasks.Add(new MyTask<int>(() =>
                {
                    Thread.Sleep(10 * num);
                    return num;
                }));
            }

            tasks.ForEach((task) => _pool.Enqueue(task));
            for (int i = 20; i >= 1; --i)
            {
                Assert.AreEqual(i, tasks[i - 1].Result);
            }
        }

        [Test]
        public void AddTaskAndContinuation()
        {
            var task1 = new MyTask<int>(() =>
            {
                Thread.Sleep(100);
                return 1;
            });
            var task2 = task1.ContinueWith((x) => x + 1);
            var task3 = task2.ContinueWith((x) => x + 1);
            var task4 = new MyTask<int>(() =>
            {
                Thread.Sleep(100);
                return 4;
            });
            _pool.Enqueue(task1);
            _pool.Enqueue(task4);
            Assert.AreEqual(3, task3.Result);
            Assert.AreEqual(4, task4.Result);
        }

        [Test]
        public void AddTaskThrowsException()
        {
            var task = new MyTask<int>(() =>
            {
                Thread.Sleep(100);
                throw new Exception();
            });
            _pool.Enqueue(task);
            Assert.Throws<AggregateException>(delegate
            {
                var _ = task.Result;
            });
        }

        [Test]
        public void AddTaskToDisposedPool()
        {
            var task = new MyTask<int>(() =>
            {
                Thread.Sleep(100);
                return 1;
            });
            _pool.Dispose();
            Assert.Throws<ObjectDisposedException>(delegate { _pool.Enqueue(task); });
        }

        [Test]
        public void CheckNumberOfActiveThreads()
        {
            var tasks = new List<IMyTask<int>>();
            for (int i = 1; i <= 2; ++i)
            {
                var num = i;
                tasks.Add(new MyTask<int>(() =>
                {
                    for (int i = 0; i < 2 * 1e9; ++i)
                    {
                        continue;
                    }

                    return num;
                }));
            }

            tasks.ForEach((task) => _pool.Enqueue(task));
            Assert.AreEqual(2, _pool.ActiveThreads);
            Assert.AreEqual(4, _pool.MaxThreadsNum);
        }

        [TearDown]
        public void TearDown()
        {
            _pool.Dispose();
        }
    }
}
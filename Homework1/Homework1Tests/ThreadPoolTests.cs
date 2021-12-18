using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Homework1
{
    public class Tests
    {
        private MyThreadPool _threadPool;
        private Func<int> _sleepOneSecond;
        private Func<int> _doCalculation;
        private Func<int, int> _doCalculationAfter;

        private const int MaxThreadsNum = 5;
        private const int NumOfIterations = 10000;

        [SetUp]
        public void Setup()
        {
            _threadPool = new MyThreadPool(MaxThreadsNum);

            _sleepOneSecond = () =>
            {
                Thread.Sleep(1000);
                return 1;
            };

            _doCalculation = () =>
            {
                int res = 0;

                for (int i = 0; i < NumOfIterations; i++)
                {
                    res++;
                }

                return res;
            };

            _doCalculationAfter = (initial) =>
            {
                int res = initial;

                for (int i = 0; i < NumOfIterations; i++)
                {
                    res++;
                }

                return res;
            };
        }

        [TearDown]
        public void TearDown()
        {
            _threadPool.Dispose();
        }

        [Test]
        public void ThreadsNum_ShouldReturnCorrectNumberOfThreads_WhenEnoughTasksAreSubmitted()
        {
            for (int i = 0; i < MaxThreadsNum + 1; i++)
            {
                _threadPool.Enqueue(new MyTask<int>(_sleepOneSecond));
            }

            int expected = MaxThreadsNum;

            Assert.AreEqual(_threadPool.ThreadsNum, expected);
        }

        [Test]
        public void AwaitResult_ShouldReturnCorrectValue_AfterOneTaskExecuted()
        {
            var task = new MyTask<int>(_doCalculation);
            int expected = NumOfIterations;

            _threadPool.Enqueue(task);

            Assert.AreEqual(task.Result, expected);
        }

        [Test]
        public void AwaitResult_ShouldReturnCorrectValue_AfterMultipleTaskExecuted()
        {
            var listOfTasks = new List<MyTask<int>>();

            for (int i = 0; i < 20; i++)
            {
                var task = new MyTask<int>(_doCalculation);
                listOfTasks.Add(task);
                _threadPool.Enqueue(task);
            }

            int expected = NumOfIterations;

            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(listOfTasks[i].Result, expected);
            }
        }

        [Test]
        public void AwaitResult_ShouldReturnCorrectValue_AfterContinueWithTasksExecuted()
        {
            var taskOne = new MyTask<int>(_doCalculation);
            var taskTwo = taskOne.ContinueWith<int>(_doCalculationAfter);
            var taskThree = taskTwo.ContinueWith<int>(_doCalculationAfter);

            _threadPool.Enqueue(taskOne);

            int expectedOne = NumOfIterations;

            Assert.AreEqual(taskOne.Result, expectedOne);

            int expectedTwo = NumOfIterations * 2;

            Assert.AreEqual(taskTwo.Result, expectedTwo);

            int expectedThree = NumOfIterations * 3;

            Assert.AreEqual(taskThree.Result, expectedThree);
        }

        [Test]
        public void CreateThreadPool_ShouldThrowException_WhenIncorrectNumOfThreadsSubmitted()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MyThreadPool(0));
        }

        [Test]
        public void Enqueue_ShouldThrowException_WhenNullTaskIsSubmitted()
        {
            Assert.Throws<ArgumentNullException>(() => _threadPool.Enqueue(null));
        }
    }
}

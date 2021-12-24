using NUnit.Framework;
using MyThreading;
using System;
using System.Collections.Generic;

namespace Tests {
    internal class MyTest<TResult> {
        internal TResult Expected => _expected;
        private TResult _expected;

        internal Func<TResult> Function => _func;
        private Func<TResult> _func;

        public MyTest(TResult expected, Func<TResult> func) {
            _expected = expected;
            _func = func;
        }
    }

    public class Tests {
        private ThreadPool _threadPool;
        private MyTest<int> _simpleMultiplicationTest;
        private MyTest<int> _simpleMultiplicationFailingTest;
        private List<MyTest<int>> _simpleMultiplicationTests;


        [SetUp]
        public void Setup() {
            _threadPool = new ThreadPool(4);

            _simpleMultiplicationTest = new MyTest<int>(4
                , () => {
                    System.Threading.Thread.Sleep(100);
                    return 2 * 2;
                });

            _simpleMultiplicationFailingTest = new MyTest<int>(-1
                , () => {
                    System.Threading.Thread.Sleep(100);

                    throw new Exception();
                });

            _simpleMultiplicationTests = new List<MyTest<int>>();
        }

        [TearDown]
        public void TearDown() {
            _threadPool.Dispose();
        }

        [Test]
        public void EnqueueOneTask() {
            var task = new MyTask<int>(_simpleMultiplicationTest.Function);

            _threadPool.Enqueue(task);

            Assert.AreEqual(_simpleMultiplicationTest.Expected, task.Result);
        }

        [Test]
        public void EnqueueManyTasks() {
            var tasks = new List<MyTask<int>>();
            for (int i = 0; i < 20; i++) {
                var currNum = i;
                _simpleMultiplicationTests.Add(new MyTest<int>(currNum * currNum
                    , () => {
                        System.Threading.Thread.Sleep(100);
                        return currNum * currNum;
                    })
                );

                var task = new MyTask<int>(_simpleMultiplicationTests[i].Function);
                
                tasks.Add(task);
                _threadPool.Enqueue(task);
            }

            for (int i = 0; i < 20; i++) {
                Assert.AreEqual(_simpleMultiplicationTests[i].Expected, tasks[i].Result);
            }
        }

        [Test]
        public void EnqueueOneTaskAndOneContinuation() {
            var task = new MyTask<int>(_simpleMultiplicationTest.Function);
            var contTask = task.ContinueWith(x => x + x);

            _threadPool.Enqueue(task);

            Assert.AreEqual(8, contTask.Result);
        }


        [Test]
        public void EnqueueOneTaskAndSomeContinuations() {
            var task = new MyTask<int>(_simpleMultiplicationTest.Function);
            var contTask1 = task.ContinueWith(x => x * 2);
            var contTask2 = contTask1.ContinueWith(x => x + 1);

            _threadPool.Enqueue(task);

            Assert.AreEqual(9, contTask2.Result);
        }

        [Test]
        public void EnqueueTaskWhenPoolDisposed() {
            var undisposedThreadPool = new ThreadPool(4);
            
            var task = new MyTask<int>(_simpleMultiplicationTest.Function);

            undisposedThreadPool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => undisposedThreadPool.Enqueue(task));
        }

        [Test]
        public void EnqueueTaskWhichThrowsException() {
            var task = new MyTask<int>(_simpleMultiplicationFailingTest.Function);

            Assert.Throws<AggregateException>(() => {
                _threadPool.Enqueue(task);
                var _ = task.Result;
            });
        }
        
        [Test]
        public void EnqueueNullTask() {
            Assert.Throws<ArgumentNullException>(() => _threadPool.Enqueue(null));
        }
    }
}
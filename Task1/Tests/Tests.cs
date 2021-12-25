using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using Lib;

namespace Tests
{
    public class Tests
    {
        [Test]
        public static void NoTask()
        {
            var tp = new Lib.ThreadPool(3);
            tp.Dispose();
            Assert.IsTrue(tp.IsStopped(10000));
        }

        [Test]
        public static void OneTask()
        {
            var tp = new Lib.ThreadPool(3);
            var t = new SmpTask<object>(() => {
                Thread.Sleep(3000);
                return null;
            });
            tp.Enqueue(t);
            tp.Dispose();
            Assert.IsTrue(tp.IsStopped(10000));
        }

        [Test]
        public static void TaskWithException()
        {
            var tp = new Lib.ThreadPool(3);
            var t = new SmpTask<object>(() => {
                Thread.Sleep(500);
                throw new System.Exception("Something went wrong...");
            });
            tp.Enqueue(t);
            tp.Dispose();
            Assert.Throws<System.AggregateException>(() => tp.IsStopped(10000));
        }

        [Test]
        public static void ManyTasks()
        {
            var tp = new Lib.ThreadPool(3);
            for (int i = 0; i < 42; i++)
            {
                tp.Enqueue(new SmpTask<object>(() => {
                    Thread.Sleep(420);
                    return i;
                }));
            }
            tp.Dispose();
            Assert.IsTrue(tp.IsStopped(10000));
        }

        [Test]
        public static void TasksWithContinuation1()
        {
            var tp = new Lib.ThreadPool(3);
            var ts1 = new List<IMyTask<object>> {
                new SmpTask<object>(() => 10),
                new SmpTask<object>(() => { Thread.Sleep(50); return 20; }),
                new SmpTask<object>(() => { Thread.Sleep(100); return 30; })};
            var ts2 = new List<IMyTask<object, object>>();
            ts1.ForEach((task) => {
                ts2.Add(task.ContinueWith<object>((x) => (int)x + 1));
                ts2.Add(task.ContinueWith<object>((x) => { Thread.Sleep(50); return (int)x + 2; }));
                ts2.Add(task.ContinueWith<object>((x) => { Thread.Sleep(100); return (int)x + 3; }));
            });
            ts1.ForEach((task) => tp.Enqueue(task));
            ts2.ForEach((task) => tp.Enqueue(task));
            tp.Dispose();
            Assert.IsTrue(tp.IsStopped(10000));
        }

        [Test]
        public static void TasksWithContinuation2()
        {
            var tp = new Lib.ThreadPool(3);
            IMyTask<object> ts = new SmpTask<object>(() => 1);
            tp.Enqueue(ts);
            ts = ts.ContinueWith<object>((x) => { Thread.Sleep(100); return 2; });
            tp.Enqueue(ts);
            ts = ts.ContinueWith<object>((x) => { Thread.Sleep(50); return (int)x * 7; });
            tp.Enqueue(ts);
            ts = ts.ContinueWith<object>((x) => "x = " + ((int)x).ToString());
            tp.Enqueue(ts);
            ts = ts.ContinueWith<object>((x) => { Thread.Sleep(100); return ((string)x).Length; });
            tp.Enqueue(ts);
            tp.Dispose();
            Assert.IsTrue(tp.IsStopped(10000));
        }
    }
}

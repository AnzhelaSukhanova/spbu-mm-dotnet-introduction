using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    public static class Program
    {
        public static void Main()
        {
            using var pool = new MyThreadPool(4);
            var task1 = new MyTask<int>(() =>
            {
                Thread.Sleep(5000);
                return 1;
            });
            var task2 = new MyTask<int>(() =>
            {
                Thread.Sleep(5000);
                return 2;
            });
            var task3 = new MyTask<int>(() =>
            {
                Thread.Sleep(5000);
                return 3;
            });
            // var task4 = task1.ContinueWith((x) =>
            // {
            //     Thread.Sleep(5000);
            //     return "haha";
            // });
            pool.Enqueue(task1);
            pool.Enqueue(task2);
            pool.Enqueue(task3);
            // pool.Enqueue(task4);
            Console.WriteLine(task1.Result);
            Console.WriteLine(task2.Result);
            Console.WriteLine(task3.Result);
            // Console.WriteLine(task4.Result);
        }
    }
}
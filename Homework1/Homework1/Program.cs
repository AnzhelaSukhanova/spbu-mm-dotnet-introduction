using System;
using System.Threading;

namespace Homework1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var pool = new MyThreadPool(2))
            {
                var task1 = new MyTask<int>(() => 5);
                var task2 = new MyTask<int>(() => 5 + 3 - 2);
                var task3 = task1.ContinueWith((res) =>
                {
                    Thread.Sleep(1000);
                    return res + 5;
                });

                pool.Enqueue(task1);
                pool.Enqueue(task2);

                Console.WriteLine(task1.Result);
                Console.WriteLine(task2.Result);
                Console.WriteLine(task3.Result);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ThreadPoolLib;


namespace Tests
{
    internal static class Functions
    {
        internal static int GetNumber()
        {
            Random rnd = new Random();
            int n = rnd.Next(5, 11);
            return n;
        }
        
        internal static int Factorial(int n)
        {
            if (n == 1) return 1;
            return n * Factorial(n - 1);
        }

        internal static double SomeCalculation()
        {
            double num = 0;
            for (int i = 0; i <= 1e4; i++)
                num += i;
            for (int i = 1; i <= 10; i++)
                num /= i;
            return num;
        }
    }

    public static class TestPrograms
    {
        public static void ManyTasksTest()
        {
            int threadsNum = 5;
            var pool = new ThreadPool(threadsNum);
            var tasks = new List<IMyTask<string>>();
            for (int i = 0; i <= 100; i++)
            {
                var j = i;
                var task = new MyTask<string>(() => j.ToString());
                tasks.Add(task);
                pool.Enqueue(task);
                Debug.Assert(pool.CountThreads() == threadsNum, 
                    "Incorrect number of threads");
            }

            int sum = 0;
            for (int i = 0; i <= 100; i++)
            {
                sum += Convert.ToInt32(tasks[i].Result);
                Debug.Assert(pool.CountThreads() == threadsNum, 
                    "Incorrect number of threads");
            }
            
            Debug.Assert(sum == 5050,
                "'ManyTasksTest' failed");
            Debug.Assert(pool.CountThreads() == threadsNum, 
                "Incorrect number of threads");
                
            pool.Shutdown();
        }
        public static void ContinueWithTest()
        {
            var pool = new ThreadPool();
            var task1 = new MyTask<int>(Functions.GetNumber);
            var task2 = task1.ContinueWith(Functions.Factorial);
            var task3 = task2.ContinueWith(x=> x / 5);
            var task4 = task3.ContinueWith(x => x - Math.PI);
            
            pool.Enqueue(task3);
            Debug.Assert(pool.CountThreads() == ThreadPoolConstants.ThreadsNum, 
                "Incorrect number of threads");
            pool.Enqueue(task2);
            Debug.Assert(pool.CountThreads() == ThreadPoolConstants.ThreadsNum, 
                "Incorrect number of threads");
            pool.Enqueue(task4);
            Debug.Assert(pool.CountThreads() == ThreadPoolConstants.ThreadsNum, 
                "Incorrect number of threads");
            pool.Enqueue(task1);
            Debug.Assert(pool.CountThreads() == ThreadPoolConstants.ThreadsNum, 
                "Incorrect number of threads");
            
            Debug.Assert(5 <= task1.Result & task1.Result <= 11 & 
                         task2.Result == Functions.Factorial(task1.Result) &
                         task3.Result == task2.Result / 5 &
                         Math.Abs(task4.Result - (task3.Result - Math.PI)) < 1e-6,
                "'ContinueWithTest' failed");
            Debug.Assert(pool.CountThreads() == ThreadPoolConstants.ThreadsNum, 
                "Incorrect number of threads");
            
            pool.Shutdown();
        }
        
        public static void SimpleTest()
        {
            int threadsNum = 10;
            var pool = new ThreadPool(threadsNum);
            var tasks = new List<IMyTask<int>>();
            var task = new MyTask<int>(Functions.GetNumber);
            tasks.Add(task);
            tasks.Add(tasks[^1].ContinueWith(Functions.Factorial));
            tasks.Add(tasks[^1].ContinueWith(x=> 3 * x));
            tasks.Add(tasks[^1].ContinueWith(x => x + 10));
            foreach (var t in tasks)
            {
                pool.Enqueue(t);
                Debug.Assert(pool.CountThreads() == threadsNum, 
                    "Incorrect number of threads");
            }
            
            Debug.Assert(5 <= tasks[0].Result & tasks[0].Result <= 11 & 
                         tasks[1].Result == Functions.Factorial(tasks[0].Result) &
                         tasks[2].Result == tasks[1].Result * 3 &
                         tasks[3].Result == tasks[2].Result + 10,
                "'SimpleTest' failed");
            
            Debug.Assert(pool.CountThreads() == threadsNum, 
                "Incorrect number of threads");
            
            pool.Shutdown();
        }
        
        public static void OneTaskTest()
        {
            var pool = new ThreadPool();
            var task = new MyTask<double>(Functions.SomeCalculation);
            pool.Enqueue(task);
            Debug.Assert(pool.CountThreads() == ThreadPoolConstants.ThreadsNum, 
                "Incorrect number of threads");
            
            Debug.Assert(Math.Round(task.Result, 2) == 13.78,
                "'OneTaskTest' failed");
            pool.Shutdown();
        }
    }
}
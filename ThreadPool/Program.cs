using System;
using Tests;

namespace Application
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            TestPrograms.SimpleTest();
            Console.WriteLine("'SimpleTest': done");
            TestPrograms.ContinueWithTest();
            Console.WriteLine("'ContinueWithTest': done");
            TestPrograms.OneTaskTest();
            Console.WriteLine("'OneTaskTest': done");
            TestPrograms.ManyTasksTest();
            Console.WriteLine("'ManyTasksTest': done");
        }
    }
}

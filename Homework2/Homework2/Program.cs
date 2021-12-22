using System;
using System.Collections.ObjectModel;

namespace Homework2
{
    class Program
    {
        static void Main(string[] args)
        {
            var coll = new Collection<Collection<int>> {
                new Collection<int> { 1, 2 }, null, null
            };
            var ind = new int[] { 0, 0 };
          
            // equal to coll?.[0]?.[0]
            Console.WriteLine(NullConditionalOperator.AccessElement<Collection<int>, int>(coll, ind));
        }
    }
}

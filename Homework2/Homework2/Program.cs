using System;
using System.Collections.ObjectModel;

namespace Homework2
{
    class Program
    {
        static void Main(string[] args)
        {
            var l = new Collection<Collection<string>> { new Collection<string> {"abc"}, null, null };
            //l = null;
            var ind = new int[] { 0, 0 };

            Console.WriteLine(NullConditionalOperator.AccessElement<Collection<string>, string>(l, ind));
        }
    }
}

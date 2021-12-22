using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SecondTask
{
    public static class SafeGetter
    {
        private static Array GetArray(object array, List<int> path)
        {
            List<object> values = new List<object>() {array};
            for (int i = 0; i < path.Count; i++)
            {
                int index = path[i];
                Debug.Assert(values[^1].GetType().IsArray);
                var newArray = (object[]) values[^1];
                if (index > newArray.Length - 1 || newArray[index] == null)
                    return null;
                values.Add(newArray[index]);
            }
            return (Array) values[^1];
        }
        
        internal static Func<int, object> FromArray<T>(T array, List<int> path)
        {
            var newArray = GetArray(array, path);

            ParameterExpression param = Expression.Parameter(typeof(int));
            Expression<Func<int, object>> result = GetNullExpr(param);
            if (newArray != null)
            {
                ConstantExpression arrayExpr = Expression.Constant(newArray);
                Expression arrayAccess = Expression.ArrayAccess(arrayExpr, param);
                Expression convertExpr = Expression.Convert(arrayAccess, typeof(object));
                result = Expression.Lambda<Func<int, object>>
                (
                    convertExpr,
                    param
                );
            }
            return result.Compile();
        }

        private static Expression<Func<int, object>> GetNullExpr(ParameterExpression param)
        {
            Expression<Func<int, object>> nullExpr = Expression.Lambda<Func<int, object>>
            (
                Expression.Constant(null),
                param
            );
            return nullExpr;
        }
    }

    static class Program
    {
        static void Main()
        {
            Tests.PathOfOne();
            Console.WriteLine("'PathOfOne' test: done");
            Tests.PathOfTwo();
            Console.WriteLine("'PathOfTwo' test: done");
            Tests.PathOfThree();
            Console.WriteLine("'PathOfThree' test: done");
        }
    }

    static class Tests
    {
        internal static void PathOfOne()
        {
            int[][] array = new int[2][];
            array[0] = new int[3];
            array[0][0] = 42;
            array[0][1] = 404;

            List<int> path1 = new List<int>() {0};
            List<int> path2 = new List<int>() {1};
            List<int> path3 = new List<int>() {2};

            Debug.Assert((int)SafeGetter.FromArray(array, path1)(0) == 42);
            Debug.Assert((int)SafeGetter.FromArray(array, path1)(1) == 404);
            Debug.Assert((int)SafeGetter.FromArray(array, path1)(2) == 0);
            Debug.Assert(SafeGetter.FromArray(array, path2)(0) == null);
            Debug.Assert(SafeGetter.FromArray(array, path3)(0) == null);
        }
        
        internal static void PathOfTwo()
        {
            string[][][] array = new string[1][][];
            array[0] = new string[1][];
            array[0][0] = new string[2]
            {
                "Mordor, Gandalf, is it left or right?",
                "— You have my sword. — And my bow. — And my axe."
            };
            List<int> path1 = new List<int>() {0, 0};
            List<int> path2 = new List<int>() {1};
            List<int> path3 = new List<int>() {0, 1};
            
            Debug.Assert((string)SafeGetter.FromArray(array, path1)(0) == 
                         "Mordor, Gandalf, is it left or right?");
            Debug.Assert((string)SafeGetter.FromArray(array, path1)(1) == 
                         "— You have my sword. — And my bow. — And my axe.");
            Debug.Assert(SafeGetter.FromArray(array, path2)(0) == null);
            Debug.Assert(SafeGetter.FromArray(array, path3)(0) == null);
        }
        
        internal static void PathOfThree()
        {
            bool[][][][] array = new bool[1][][][];
            array[0] = new bool[2][][];
            array[0][1] = new bool[3][];
            array[0][1][2] = new bool[1] {true};

            List<int> path1 = new List<int>() {0, 1, 2};
            List<int> path2 = new List<int>() {0, 0, 0};
            List<int> path3 = new List<int>() {0, 1, 0};
            
            Debug.Assert((bool)SafeGetter.FromArray(array, path1)(0));
            Debug.Assert(SafeGetter.FromArray(array, path2)(0) == null);
            Debug.Assert(SafeGetter.FromArray(array, path3)(0) == null);
        }
    }
}

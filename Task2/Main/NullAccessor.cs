using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Main
{
    public interface IAccessField { }

    public class Index : IAccessField
    {
        public int Value;

        public Index(int index) { Value = index; }
    }

    public class Field : IAccessField
    {
        public string Name;

        public Field(string name) { Name = name; }
    }

    public static class NullAccessor
    {
        private static Expression GetField(IAccessField field, ParameterExpression resultVar, ref Type resultType, BlockExpression returnNull)
        {
            Expression getter;
            if (field is Index)
            {
                try
                {
                    getter = Expression.Property(
                        Expression.Convert(resultVar, resultType),
                        "Item",
                        Expression.Constant(((Index)field).Value));
                }
                catch
                {
                    getter = Expression.Constant(null);
                }
            }
            else if (field is Field)
            {
                try
                {
                    getter = Expression.Field(
                        Expression.Convert(resultVar, resultType),
                        ((Field)field).Name);
                }
                catch
                {
                    getter = Expression.Constant(null);
                }
            }
            else
            {
                return returnNull;
            }
            resultType = getter.Type;
            var tryPart = Expression.Block(
                typeof(void),
                Expression.Assign(
                    resultVar,
                    Expression.Convert(getter, typeof(object))));
            var catchPart = Expression.Catch(
                typeof(Exception),
                returnNull);
            return Expression.TryCatch(
                tryPart,
                catchPart);
        }

        public static Func<T, R> Get<T, R>(params IAccessField[] fields)
        {
            var param = Expression.Parameter(typeof(T));
            var resultVar = Expression.Variable(typeof(object));
            var resultType = typeof(T);
            var returnLabel = Expression.Label();
            var returnNull = Expression.Block(
                Expression.Assign(resultVar, Expression.Constant(null)),
                Expression.Goto(returnLabel));
            var body = new List<Expression> { Expression.Assign(resultVar, param) };
            foreach (var field in fields)
            {
                body.Add(GetField(field, resultVar, ref resultType, returnNull));
            }
            body.Add(Expression.Label(returnLabel));
            body.Add(Expression.Convert(resultVar, typeof(R)));
            var func = Expression.Lambda<Func<T, R>>(
                Expression.Block(
                    new List<ParameterExpression> { resultVar },
                    body),
                param);
            return func.Compile();
        }
    }

    public static class Program
    {
        public static void Main() { }
    }
}

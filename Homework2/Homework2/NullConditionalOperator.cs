using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Homework2
{
    public static class NullConditionalOperator
    {
        public static R? AccessElement<T, R>(Collection<T> collection, int[] indices)
        {
            if (indices.Length == 0)
            {
                throw new ArgumentException("Indices length must be greater than zero");
            }

            LabelTarget returnTarget = Expression.Label(typeof(R), "returnTarget");
            var parameter = Expression.Parameter(typeof(Collection<T>));
            var current = Expression.Variable(typeof(object));
            var elemType = parameter.Type;

            var exprList = new List<Expression> {
                Expression.IfThenElse(
                    Expression.Equal(parameter, Expression.Constant(null, elemType)),
                    Expression.Return(returnTarget, Expression.Constant(null, typeof(R))),
                    Expression.Assign(current, parameter)
                )
            };

            foreach (int index in indices)
            {
                Type elemGenericType = elemType.GetGenericTypeDefinition();

                if (!elemGenericType.Equals(typeof(Collection<>)) &&
                    !elemGenericType.IsSubclassOf(typeof(Collection<>)))
                {
                    throw new ArgumentException("All items in path should be a collection");
                }

                var item = Expression.Property(Expression.Convert(current, elemType),
                     "Item", Expression.Constant(index));

                elemType = item.Type;
                exprList.Add(
                    Expression.IfThenElse(
                        Expression.Equal(item, Expression.Constant(null, elemType)),
                        Expression.Return(returnTarget, Expression.Constant(null, typeof(R))),
                        Expression.Assign(current, item)
                    )
                );
            }

            exprList.Add(Expression.Label(returnTarget, Expression.Convert(current, elemType)));

            var body = Expression.Block(new[] { current }, exprList);
            Expression<Func<Collection<T>, R>> lambda =
                Expression.Lambda<Func<Collection<T>, R>>(body, parameter);

            return lambda.Compile().Invoke(collection);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Nullability
{
    public static class NullabilityUtils
    {
        public static TResult? NullableGet<TClass, TResult>(TClass instance, string fieldPath)
        {
            string[] fieldNames = fieldPath.Split('.');
            if (fieldNames.Length == 0 || fieldNames.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("Invalid sequence of fields.");
            }

            if (instance == null)
            {
                return default;
            }

            var type = instance.GetType();
            foreach (var fieldName in fieldNames)
            {
                var field = type.GetField(fieldName);
                if (field == null)
                {
                    throw new ArgumentException(
                        $"Field {fieldName} was not found in {type.Name}, or can not be accessed.");
                }

                type = field.FieldType;
            }

            ParameterExpression parameter = Expression.Parameter(typeof(TClass));
            var current = Expression.Variable(typeof(object));
            var expressions = new List<Expression> {Expression.Assign(current, parameter)};

            Type fieldType = typeof(TClass);
            foreach (var fieldName in fieldNames)
            {
                var field = Expression.Field(Expression.Convert(current, fieldType), fieldName);
                expressions.Add(
                    Expression.IfThenElse(
                        Expression.Equal(current, Expression.Constant(null)),
                        Expression.Assign(current, Expression.Constant(null)),
                        Expression.Assign(current, field)
                    )
                );
                fieldType = field.Type;
            }

            expressions.Add(Expression.Convert(current, fieldType));
            var body = Expression.Block(new[] {current}, expressions);

            Expression<Func<TClass, TResult>> lambda = Expression.Lambda<Func<TClass, TResult>>(body, parameter);
            return lambda.Compile().Invoke(instance);
        }

        public static void Main()
        {
        }
    }
}
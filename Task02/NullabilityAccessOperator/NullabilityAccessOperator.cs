using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NullabilityAccessOperator {
    public static class NullabilityAccess {
        public static object GetFromArray<T>(T array, IList<int> indices) {
            if (indices == null || indices.Count == 0) {
                throw new ArgumentException("Indices must be not null and have at list one index");
            }

            var lastIndex = indices[^1];

            ParameterExpression param = Expression.Parameter(typeof(int));

            Expression<Func<int, object>> lambda = Expression.Lambda<Func<int, object>>(Expression.Constant(null), param);

            if (array == null) {
                return lambda.Compile().Invoke(lastIndex);
            }

            var currArray = array as object[];
            int currArrayIndex = indices[0];
            object finalArray = null;

            for (int i=0; i < indices.Count - 1; i++) {
                currArrayIndex = indices[i];

                if (currArrayIndex > currArray.Length - 1
                    || currArray[currArrayIndex] == null
                    || !currArray[currArrayIndex].GetType().IsArray) {
                    return lambda.Compile().Invoke(lastIndex);
                }
                
                if (i >= indices.Count - 2) {
                    finalArray = currArray[currArrayIndex];
                }
                else {
                    currArray = (object[])currArray[currArrayIndex];
                }

            }

            Expression arrExpr = Expression.Constant(finalArray);
            Expression arrayAccessExpr = Expression.ArrayAccess(arrExpr, param);
            Expression convertedArrayExpr = Expression.Convert(arrayAccessExpr, typeof(object));
            lambda = Expression.Lambda<Func<int, object>>(convertedArrayExpr, param);

            return lambda.Compile().Invoke(lastIndex);
        }
    }
}

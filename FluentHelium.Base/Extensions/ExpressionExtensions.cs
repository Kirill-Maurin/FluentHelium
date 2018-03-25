using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentHelium.Base
{
    public static class ExpressionExtensions
    {
        public static Expression Convert(this Expression expression, Type type) => Expression.Convert(expression, type);

        public static Expression Convert<T>(this Expression expression) => Expression.Convert(expression, typeof(T));

        public static Expression Get(this Expression expression, PropertyInfo property) => Expression.Property(expression, property);
    }
}

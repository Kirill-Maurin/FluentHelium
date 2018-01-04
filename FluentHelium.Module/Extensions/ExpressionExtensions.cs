using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentHelium.Module
{
    public static class ExpressionExtensions
    {
        public static Expression Convert(this Expression expression, Type type)
        {
            return Expression.Convert(expression, type);
        }

        public static Expression Convert<T>(this Expression expression)
        {
            return Expression.Convert(expression, typeof(T));
        }

        public static Expression Get(this Expression expression, PropertyInfo property)
        {
            return Expression.Property(expression, property);
        }
    }
}

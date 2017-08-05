using System.Collections.Generic;

namespace FluentHelium.Module
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Enumerable<T>(params T[] items) => items;
    }
}

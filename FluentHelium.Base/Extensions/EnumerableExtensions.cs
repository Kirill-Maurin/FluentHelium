using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentHelium.Base
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> CreateEnumerable<T>(params T[] items) => items;

        /// <summary>
        /// Generic topology sort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodes"></param>
        /// <param name="getLinks"></param>
        /// <param name="order">Order if success, cycle if fail</param>
        /// <returns>True - success, False - fail (graph contains at least one cycle)</returns>
        public static bool TryTopologySort<T>(
            this IEnumerable<T> nodes,
            Func<T, IEnumerable<T>> getLinks,
            out IEnumerable<T> order) where T : class
        {
            var nodeList = nodes.ToImmutableList();
            if (nodeList.Count == 0)
            {
                order = Enumerable.Empty<T>();
                return true;
            }
            var path = new Stack<IEnumerator<T>>();
            var result = new List<T>();
            var colors = Enumerable.Range(0, nodeList.Count).ToDictionary(i => nodeList[i], i => Color.White);
            var next = nodeList.Where(m => colors[m] == Color.White).GetEnumerator();
            T current = null;
            for (; ; )
            {
                if (!next.MoveNext())
                {
                    if (path.Count == 0)
                        break;
                    colors[current] = Color.Black;
                    result.Add(current);
                    next = path.Pop();
                    current = path.Count > 0 ? path.Peek().Current : null;
                    continue;
                }
                current = next.Current;
                if (colors[current] == Color.Gray)
                {
                    order = path.Select(e => e.Current).TakeWhile(c => !c.Equals(current)).Concat(new[] { current });
                    return false;
                }
                path.Push(next);
                next = getLinks(current).Where(l => colors[l] != Color.Black).GetEnumerator();
                colors[current] = Color.Gray;
            }
            order = result;
            return true;
        }

        enum Color { White, Gray, Black }
    }
}

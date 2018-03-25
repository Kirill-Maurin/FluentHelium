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
            var moduleList = nodes.ToImmutableList();
            var path = new Stack<T>();
            var result = new List<T>();
            var colors = Enumerable.Range(0, moduleList.Count).ToDictionary(i => moduleList[i], i => Color.White);

            foreach (var m in moduleList.Where(m => colors[m] == Color.White))
            {
                var current = m;
                colors[current] = Color.Gray;
                for (; ; )
                {
                    var next = getLinks(current).FirstOrDefault(l => colors[l] != Color.Black);
                    if (next == null)
                    {
                        colors[current] = Color.Black;
                        result.Add(current);
                        if (path.Count == 0)
                            break;
                        current = path.Pop();
                        continue;
                    }
                    path.Push(current);
                    if (colors[next] == Color.Gray)
                    {
                        order = path.TakeWhile(c => !ReferenceEquals(c, next)).Concat(new[] { next });
                        return false;
                    }
                    current = next;
                    colors[current] = Color.Gray;
                }
            }
            order = result;
            return true;
        }

        private enum Color { White, Gray, Black }
    }
}

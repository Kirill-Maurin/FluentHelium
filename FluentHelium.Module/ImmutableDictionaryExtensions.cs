using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FluentHelium.Module
{
    public static class ImmutableDictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }

        public static IEnumerable<TValue> GetValue<TKey, TValue>(
            this IImmutableDictionary<TKey, IEnumerable<TValue>> dictionary, TKey key)
        {
            IEnumerable<TValue> value;
            return dictionary.TryGetValue(key, out value) ? value : Enumerable.Empty<TValue>();
        }

        public static IEnumerable<IGrouping<TGroup, TValue>> GetValue<TKey, TGroup, TValue>(
            this IImmutableDictionary<TKey, ILookup<TGroup, TValue>> dictionary, TKey key)
        {
            ILookup<TGroup, TValue> value;
            return dictionary.TryGetValue(key, out value) ? value : Enumerable.Empty<IGrouping<TGroup, TValue>>();
        }
    }
}

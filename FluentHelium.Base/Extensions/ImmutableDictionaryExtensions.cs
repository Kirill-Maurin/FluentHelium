using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentHelium.Base
{
    public static class ImmutableDictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key) 
	        => dictionary.TryGetValue(key, out var value) ? value : default;

        public static IEnumerable<TValue> GetValue<TKey, TValue>(
            this IImmutableDictionary<TKey, IEnumerable<TValue>> dictionary, TKey key) 
	        => dictionary.TryGetValue(key, out var value) ? value : Enumerable.Empty<TValue>();

        public static IEnumerable<IGrouping<TGroup, TValue>> GetValue<TKey, TGroup, TValue>(
            this IImmutableDictionary<TKey, ILookup<TGroup, TValue>> dictionary, TKey key) 
	        => dictionary.TryGetValue(key, out var value) ? value : Enumerable.Empty<IGrouping<TGroup, TValue>>();
    }
}

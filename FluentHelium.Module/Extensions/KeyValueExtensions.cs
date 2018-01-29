using System.Collections.Generic;
using NullGuard;

namespace FluentHelium.Module
{
    public static class KeyValueExtensions
    {
        public static KeyValuePair<TKey, TValue> LinkValue<TKey, TValue>(this TKey key, [AllowNull]TValue value) 
            => new KeyValuePair<TKey,TValue>(key, value);
        public static KeyValuePair<TKey, TValue> LinkKey<TKey, TValue>(this TValue value, TKey key) 
            => new KeyValuePair<TKey, TValue>(key, value);
    }
}
using System;

namespace FluentHelium.Module
{
    public static class OptionExtensions
    {
        public static Option<T> ToOption<T>(this T source) => source != null ? new Option<T>(source) : Option<T>.Nothing;
        public static Option<T> ToOption<T>(this T? source) where T : struct => source.HasValue ? new Option<T>(source.Value) : Option<T>.Nothing;
        public static T? ToNullable<T>(this T source) where T : struct => source;

        public static Option<TOutput> Select<T, TOutput>(this Option<T> option, Func<T, TOutput> selector)
            => option.HasValue ? selector(option.Value).ToOption() : Option<TOutput>.Nothing;
        public static Option<TOutput> Bind<T, TOutput>(this Option<T> option, Func<T, Option<TOutput>> selector)
            => option.HasValue ? selector(option.Value) : Option<TOutput>.Nothing;
    }
}
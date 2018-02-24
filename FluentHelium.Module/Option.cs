using System;
using NullGuard;

namespace FluentHelium.Module
{
    public static class Option
    {
        public static Option<T, RefOption<T>> From<T>([AllowNull]T value) where T : class => RefOption.From(value);
        public static Option<T, ValOption<T>> From<T>([AllowNull]T? value) where T : struct => ValOption.From(value);
        public static Option<T, RefOption<T>> ToOption<T>([AllowNull]this T source) where T : class => From(source);
        public static Option<T, ValOption<T>> ToOption<T>([AllowNull]this T? source) where T : struct => From(source);
        public static T? AsNullable<T>(this T source) where T : struct => source;

        public static T GetValue<T, TO>(this Option<T, TO> option, T fallback) 
            where TO : struct, IOption<T, TO> 
            => option.TryGet(out var value) ? value : fallback;
        public static T GetValue<T>(this ValOption<T> option, T fallback) 
            where T : struct 
            => option.TryGet(out var value) ? value : fallback;
        public static T GetValue<T>(this RefOption<T> option, T fallback)
            where T : class
            => option.TryGet(out var value) ? value : fallback;

        public static Option<TOutput, ValOption<TOutput>> Select<T, TO, TOutput>(this Option<T, TO> option, Func<T, TOutput?> selector) 
            where TOutput : struct 
            where TO : struct, IOption<T, TO> 
            => option.TryGet(out var value) ? selector(value).ToOption() : default;

        public static Option<TOutput, RefOption<TOutput>> Select<T, TO, TOutput>(this Option<T, TO> option, Func<T, TOutput> selector)
            where TOutput : class
            where TO : struct, IOption<T, TO>
            => option.TryGet(out var value) ? selector(value).ToOption() : default;

        public static Option<TOutput, ValOption<TOutput>> SelectValue<T, TO, TOutput>(this Option<T, TO> option, Func<T, TOutput> selector)
            where TOutput : struct
            where TO : struct, IOption<T, TO>
            => option.TryGet(out var value) ? selector(value).ToValSome() : default;

        public static Option<TOutput, TOutputO> SelectMany<T, TOutput, TO, TOutputO>(this Option<T, TO> option, Func<T, Option<TOutput, TOutputO>> selector) 
            where TO : struct, IOption<T, TO>
            where TOutputO : struct, IOption<TOutput, TOutputO>
            => option.TryGet(out var value) ? selector(value) : default;
    }

    public struct Option<T, TO> : IOption<T, TO> where TO : struct, IOption<T, TO>
    {
        public Option(TO value) => Inner = value;

        public bool TryGet([AllowNull]out T value) => Inner.TryGet(out value);

        public TO Some(T value) => default(TO).Some(value);

        public TO Inner { get; }

        public static implicit operator TO(Option<T, TO> option) => option.Inner;

        public static implicit operator Option<T, TO>(TO option) => new Option<T, TO>(option);
    }
}
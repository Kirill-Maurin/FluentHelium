using System;
using NullGuard;

namespace FluentHelium.Module
{
    public static class Option
    {
        public static Option<T> ToJust<T>( this T value ) => new Option<T>( value );
        public static RefOption<T> ToRefJust<T>( this T value ) where T : class => new RefOption<T>( value );
        public static RefOption<T> ToRefOption<T>([AllowNull]this T source) where T : class => source?.ToRefJust() ?? RefOption<T>.Nothing;
        public static Option<T> ToOption<T>([AllowNull]this T source) where T : class => source?.ToJust() ?? Option<T>.Nothing;
        public static Option<T> ToOption<T>([AllowNull]this T? source) where T : struct => source?.ToJust() ?? Option<T>.Nothing;
        public static T? ToNullable<T>(this T source) where T : struct => source;

        public static Option<TOutput> Select<T, TOutput>(this Option<T> option, Func<T, TOutput> selector)
            => option.GetValue(Unit.Value, (s, v) => selector(v).ToJust(), Option<TOutput>.Nothing);
        public static Option<TOutput> SelectMany<T, TOutput>(this Option<T> option, Func<T, Option<TOutput>> selector)
            => option.GetValue(Unit.Value, (s, v) => selector(v), Option<TOutput>.Nothing );
    }

    public interface IOption<T>
    {
        bool HasValue { get; }
        TOutput GetValue<TState, TOutput>(TState state, Func<TState, T, TOutput> selector, Func<TState, TOutput> fallback);
        TOutput GetValue<TState, TOutput>(TState state, Func<TState, T, TOutput> selector, TOutput fallback);
        T GetValue(T fallback);
    }

    /// <summary>
    /// Safe universal nullable (by design)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Option<T> : IOption<T>
    {
        internal Option(T value) => _value = new Value<T>(value);

        private readonly Value<T>? _value;

        public static Option<T> Nothing { get; } = new Option<T>();

        public bool HasValue => _value.HasValue;

        public TOutput GetValue<TState, TOutput>(TState state, Func<TState, T, TOutput> selector, Func<TState, TOutput> fallback) 
            => _value.HasValue ? selector(state, _value.Value.Unwrap) : fallback(state);

        public TOutput GetValue<TState, TOutput>(TState state, Func<TState, T, TOutput> selector, TOutput fallback)
            => _value.HasValue ? selector(state, _value.Value.Unwrap) : fallback;

        public T GetValue(T fallback) => _value.HasValue ? _value.Value.Unwrap : fallback;

        public override string ToString() => GetValue(Unit.Value, (s, v) => $"Option{{{v}}}", s => $"Nothing<{nameof(T)}>");
    }

    public struct RefOption<T> where T: class
    {
        internal RefOption(T value) => _value = value;

        private readonly T _value;

        public static RefOption<T> Nothing { get; } = new RefOption<T>();

        public bool HasValue => _value != null;
        public T GetValue( T fallback ) => _value ?? fallback;

        public override string ToString() => _value != null ? $"Option{{{_value}}}" : $"Nothing<{nameof( T )}>";
    }

    public readonly struct Value<T>
    {
        internal Value( T value ) => Unwrap = value;

        public T Unwrap { get; }
    }
}
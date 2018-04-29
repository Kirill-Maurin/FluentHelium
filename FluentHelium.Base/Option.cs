using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NullGuard;

namespace FluentHelium.Base
{
    public static class Option
    {
        public static Option<T> From<T>(T value) where T: struct => new Option<T>(Some(value));
        public static Option<T, RefOption<T>> ToOption<T>([AllowNull]this T source) where T : class => RefOption.From(source);
        public static Option<T, Option<T>> ToOption<T>([AllowNull]this T? source) where T : struct => source?.AsSome() ?? default;
        public static T? AsNullable<T>(this T source) where T : struct => source;
        public static Some<T> Some<T>(T value) => new Some<T>(value);
        public static Option<T, Some<T>> ToSome<T>(this T value) => Some(value);
        public static Option<T, Option<T>> AsSome<T>(this T value) where T: struct => From(value);

        public static T GetValue<T, TO>(this Option<T, TO> option, T fallback) 
            where TO : struct, IOption<T, TO> 
            => option.TryGet(out var value) ? value : fallback;

        public static T GetValueWithLazyFallback<T, TO>(this Option<T, TO> option, Func<T> fallback)
            where TO : struct, IOption<T, TO>
            => option.TryGet(out var value) ? value : fallback();

        public static Option<TOutput, Option<TOutput>> Select<T, TO, TOutput>(this Option<T, TO> option, Func<T, TOutput?> selector) 
            where TOutput : struct 
            where TO : struct, IOption<T, TO> 
            => option.TryGet(out var value) ? selector(value).ToOption() : default;

        public static Option<TOutput, RefOption<TOutput>> Select<T, TO, TOutput>(this Option<T, TO> option, Func<T, TOutput> selector)
            where TOutput : class
            where TO : struct, IOption<T, TO>
            => option.TryGet(out var value) ? selector(value).ToOption() : default;

        public static Option<TOutput, Option<TOutput>> SelectValue<T, TO, TOutput>(this Option<T, TO> option, Func<T, TOutput> selector)
            where TOutput : struct
            where TO : struct, IOption<T, TO>
            => option.TryGet(out var value) ? selector(value).AsSome() : default;

        public static Option<TOutput, TOutputO> SelectMany<T, TOutput, TO, TOutputO>(this Option<T, TO> option, Func<T, Option<TOutput, TOutputO>> selector) 
            where TO : struct, IOption<T, TO>
            where TOutputO : struct, IOption<TOutput, TOutputO>
            => option.TryGet(out var value) ? selector(value) : default;

        public static T Unwrap<T, TO>(this Option<T, TO> option) 
            where TO : struct, IOption<T, TO> 
            => option.Unwrap(() => new InvalidOperationException($@"Attempt to unwrap null option <{typeof(T).Name}>"));

        public static T Unwrap<T, TO>(this Option<T, TO> option, Func<Exception> create)
            where TO : struct, IOption<T, TO>
        {
            if (option.TryGet(out var value))
                return value;
            throw create();
        }


        public static RefOptionTaskAwaitable<T> CanceledToNone<T>(this Task<T> task) where T : class => new RefOptionTaskAwaitable<T>(task);

        public struct RefOptionTaskAwaitable<T> : INotifyCompletion where T: class
        {
            public RefOptionTaskAwaitable(Task<T> task) => _task = task;

            public RefOptionTaskAwaitable<T> GetAwaiter()
            {
                _awaiter = _task.ConfigureAwait(false).GetAwaiter();
                return this;
            }

            public RefOption<T> GetResult() => _task.IsCanceled ? default : _awaiter.GetResult().ToRefSome();

            public bool IsCompleted => _awaiter.IsCompleted;

            public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

            private ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter _awaiter;
            private readonly Task<T> _task;
        }
    }

    public readonly struct Option<T, TO> : IOption<T, TO> where TO : struct, IOption<T, TO>
    {
        private Option(TO value) => Inner = value;

        public bool TryGet([AllowNull]out T value) => Inner.TryGet(out value);

        public TO Some(T value) => default(TO).Some(value);

        public TO Inner { get; }

        public static implicit operator TO(Option<T, TO> option) => option.Inner;

        public static implicit operator Option<T, TO>(TO option) => new Option<T, TO>(option);
    }

    /// <summary>
    /// Safe universal nullable (by design)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Option<T> : IOption<T, Option<T>>
    {
        internal Option(Some<T> value) => _value = value;

        private readonly Some<T>? _value;

        public bool TryGet([AllowNull]out T value)
        {
            var result = _value.HasValue;
            value = result ? _value.Value.Unwrap : default;
            return result;
        }

        public Option<T> Some(T value) => new Option<T>(Option.Some(value));

        public static explicit operator Option<T>(Option<T, Option<T>> option) => option.Inner;

        public Option<T, Option<T>> AsGeneric => this;

        public static explicit operator Option<T, Option<T>>(Option<T> option) => option;

        public override string ToString() => TryGet(out var v) ? $"Some{{{v}}}" : $"None<{nameof(T)}>";
    }

}
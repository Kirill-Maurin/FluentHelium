using System;
using NullGuard;

namespace FluentHelium.Module
{
    public static class Result
    {
        public static T Unwrap<T>(this Result<T> result)
        {
            if (result.IsFail)
                throw new InvalidOperationException("Attempt to unwrap result with error", result.InternalError);
            return result.InternalValue;
        }

        public static T Unwrap<T>(this Result<T> result, Func<Exception, Exception> create)
        {
            if (result.IsFail)
                throw create(result.InternalError);
            return result.InternalValue;
        }

        public static Exception UnwrapFail<T>(this Result<T> result, Func<Exception> create)
        {
            if (result.IsSuccess)
                throw create();
            return result.InternalError;
        }

        public static T GetValue<T>(this Result<T> result, T fallback) => result.IsFail 
            ? fallback 
            : result.InternalValue;

        public static T GetValue<T>(this Result<T> result, Func<Exception, T> fallback) => result.IsFail
            ? fallback(result.InternalError)
            : result.InternalValue;

        public static Result<T> ToSuccess<T>(this T value) => new Result<T>(value);
        public static Result<T> ToFail<T>(this Exception error) => new Result<T>(error);

        public static Result<TOutput> Select<T, TOutput>(this Result<T> result, Func<T, TOutput> selector) =>
            result.IsFail
                ? result.InternalError.ToFail<TOutput>()
                : selector(result.InternalValue).ToSuccess();

        public static Result<TOutput> AndTry<T, TOutput>(this Result<T> result, Func<T, TOutput> selector) =>
            result.IsFail
                ? result.InternalError.ToFail<TOutput>()
                : Try(() => selector(result.InternalValue));

        public static Result<TOutput> Try<T, TOutput>([AllowNull]this T result, Func<T, TOutput> selector) =>
            Try(() => selector(result));

        public static Result<TOutput> SelectMany<T, TOutput>(this Result<T> result, Func<T, Result<TOutput>> selector) =>
            result.IsFail
                ? result.InternalError.ToFail<TOutput>()
                : selector(result.InternalValue);

        public static Result<T> SelectError<T>(this Result<T> result, Func<Exception, Exception> selector) =>
            result.IsFail
                ? selector(result.InternalError).ToFail<T>()
                : result;

        public static Result<T> Do<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess)
                action(result.InternalValue);
            return result;
        }

        public static Result<T> Try<T>(Func<T> func)
        {
            try
            {
                return func().ToSuccess();
            }
            catch (Exception e)
            {
                return e.ToFail<T>();
            }
        }

        public static Result<T> AndTry<T>(this Result<T> result, Action<T> action)
        {
            try
            {
                return result.Do(action);
            }
            catch (Exception e)
            {
                return e.ToFail<T>();
            }
        }
    }

    public readonly struct Result<T>
    {
        internal Result(T value)
        {
            InternalValue = value;
            InternalError = null;
        }

        internal Result(Exception error)
        {
            InternalError = error;
            InternalValue = default;
        }

        internal T InternalValue { get; }
        [AllowNull]
        internal Exception InternalError { get; }

        public bool IsFail => InternalError != null;
        public bool IsSuccess => InternalError == null;

        public override string ToString() => IsFail ? $"Error{{{InternalError}}}": $"Success{{{InternalValue}}}";
    }
}
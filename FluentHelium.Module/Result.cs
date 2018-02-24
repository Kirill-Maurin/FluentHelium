using System;
using NullGuard;

namespace FluentHelium.Module
{
    public static class Result
    {
        public static T Unwrap<T>(this Result<T> result) 
        {
            if (result.TryGet(out var value, out var error))
                return value;
            throw new InvalidOperationException("Attempt to unwrap result with error", error);
        }

        public static T Unwrap<T, TResult, TError>(this TResult result, Func<TError, Exception> create) where TResult : struct, IResult<T, TError, TResult>
        {
            if (result.TryGet(out var value, out var error))
                return value;
            throw create(error);
        }
        
        public static T Unwrap<T, TResult>(this TResult result, Func<TResult, Exception, Exception> create) where TResult : struct, IResult<T, Exception, TResult>
        {
            if (result.TryGet(out var value, out var error))
                return value;
            throw create(result, error);
        }

        public static T Unwrap<T>(this Result<T> result, Func<Exception, Exception> create)
        {
            if (result.TryGet(out var value, out var error))
                return value;
            throw create(error);
        }

        public static Exception UnwrapFail<TResult>(this TResult result, Func<Exception> create) where TResult : struct, IFailure<Exception>
        {
            if (result.TryGetError(out var error))
                return error;
            throw create();
        }

        public static Result<T> ToSuccess<T>(this T value) => new Result<T>(value);
        public static Result<T> ToFail<T>(this Exception error) => new Result<T>(error);
        
        public static TOutputResult Select<T, TOutput, TResult, TOutputResult>(this TResult result, Func<T, TOutput> selector) 
            where TResult: struct, IResult<T>
            where TOutputResult: struct, IResult<TOutput, Exception, TOutputResult>
            => result.TryGet(out var value, out var error) ? default(TOutputResult).Some(selector(value)) : default(TOutputResult).Failure(error);
            

        public static Result<TOutput> AndTry<T, TOutput>(this Result<T> result, Func<T, TOutput> selector) =>
            result.IsFail
                ? result.InternalError.ToFail<TOutput>()
                : Try(() => selector(result.InternalValue));

        public static Result<TOutput> Try<T, TOutput>([AllowNull]this T result, Func<T, TOutput> selector) =>
            Try(() => selector(result));

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

    public interface IFailure<T>
    {
        bool TryGetError(out T failure);
    }

    public interface IFailure: IFailure<Exception> {}

    public interface IResult<T, TFailure> : IOption<T>, IFailure<TFailure>
    {
        bool TryGet(out T success, out TFailure failure);
    }

    public interface IFailure<T, out TResult> : IFailure<T> where TResult : struct, IFailure<T>
    {
        TResult Failure(T error);
    }

    public interface IResult<T, TFailure, out TResult> : IResult<T, TFailure>, IOption<T, TResult>, IFailure<TFailure, TResult> 
        where TResult : struct, IResult<T, TFailure>, IOption<T, TResult> {}

    public interface IResult<T> : IResult<T, Exception>, IFailure {}

    public readonly struct Result<T> : IResult<T>, IResult<T, Exception, Result<T>>
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

        public bool TryGet([AllowNull] out T success, [AllowNull] out Exception failure)
        {
            var result = IsSuccess;
            if (result)
            {
                success = InternalValue;
                failure = default;
            }
            else
            {
                failure = InternalError;
                success = default;
            }

            return result;
        }

        public bool TryGet([AllowNull]out T success)
        {
            var result = IsSuccess;
            success = result ? InternalValue : default;
            return result;
        }

        public bool TryGetError([AllowNull]out Exception failure)
        {
            var result = IsFail;
            failure = result ? InternalError : default;
            return result;
        }

        public override string ToString() => IsFail ? $"Error{{{InternalError}}}": $"Success{{{InternalValue}}}";
        public Result<T> Some(T value) => new Result<T>(value);
        public Result<T> Failure(Exception error) => new Result<T>(error);
    }
}
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NullGuard;

namespace FluentHelium.Base
{
    public static class Result
    {
        /// <summary>
        /// Try extract value from result; throw exception when failure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static T Unwrap<T>(this Result<T> result) 
            => result.Unwrap(error => new InvalidOperationException("Attempt to unwrap result with error", error));

        /// <summary>
        /// Try extract value from result; throw exception when failure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static T Unwrap<T>(this Result<T> result, Func<Exception, Exception> create)
        {
            if (result.TryGet(out var value, out var error))
                return value;
            throw create(error);
        }

        /// <summary>
        /// Try extract exception from result; throw exception when success
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static Exception UnwrapFail<TResult>(this TResult result, Func<Exception> create) where TResult : struct, IFailure<Exception>
        {
            if (result.TryGetError(out var error))
                return error;
            throw create();
        }

        /// <summary>
        /// Convert value to success
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Result<T> ToSuccess<T>(this T value) => new Result<T>(value);
        public static Result<T, TE> ToSuccess<T, TE>(this T value) where TE : class => new Result<T, TE>(value);

        /// <summary>
        /// Convert exception to failure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Result<T> ToFail<T>(this Exception error) => new Result<T>(error);
        public static Result<T, TE> ToFail<T, TE>(this T value) where TE : class => new Result<T, TE>(value);

        /// <summary>
        /// Apply function if success
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="result"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static Result<TOutput> Select<T, TOutput>(this Result<T> result, Func<T, TOutput> selector) 
            => result.TryGet(out var value, out var error) ? selector(value).ToSuccess() : error.ToFail<TOutput>();
        
        /// <summary>
        /// Try apply function if success; catch exception and convert to failure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="result"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static Result<TOutput> Try<T, TOutput>(this Result<T> result, Func<T, TOutput> selector) =>
            result.TryGet(out var value, out var error)
                ? error.ToFail<TOutput>()
                : Try(() => selector(value));

        public static Result<TOutput> Try<T, TOutput>([AllowNull]this T result, Func<T, TOutput> selector) =>
            Try(() => selector(result));

        public static Result<T> SelectError<T>(this Result<T> result, Func<Exception, Exception> selector) =>
            result.TryGetError(out var error)
                ? selector(error).ToFail<T>()
                : result;

        public static Result<T, TE, TResult> Do<T, TE, TResult>(this Result<T, TE, TResult> result, Action<T> action) where TResult : struct, IResult<T, TE, TResult>
        {
            if (result.TryGet(out var value))
                action(value);
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

        public static Result<T> Try<T>(this Result<T> result, Action<T> action)
        {
            try
            {
                return result.Generic.Do(action);
            }
            catch (Exception e)
            {
                return e.ToFail<T>();
            }
        }
        
        public static readonly Result<Unit> Unit = Base.Unit.Value.ToSuccess();
        public static Awaitable<T, ValueAwaiter<T>> ToResult<T>(this ValueTask<T> task) => new Awaitable<T, ValueAwaiter<T>>(new ValueAwaiter<T>(task));
        public static Awaitable<T, TaskAwaiter<T>> ToResult<T>(this Task<T> task) => new Awaitable<T, TaskAwaiter<T>>(new TaskAwaiter<T>(task));
        public static Awaitable<Unit, Awaiter> ToResult(this Task task) => new Awaitable<Unit, Awaiter>(new Awaiter(task));
        public static Awaitable<T, Awaiter<T>> ToResult<T>(this Task<Result<T>> task) => new Awaitable<T, Awaiter<T>>(new Awaiter<T>(task));

        public static Task<T> Unwrap<T>(this Task<Result<T>> resultTask)
        {
            if (resultTask.IsCanceled)
                return TaskExtensions.Canceled<T>();
            if (resultTask.TryGetException(out var e))
                return e.ToTask<T>();
            if (resultTask.IsCompleted)
                return resultTask.Result.TryGet(out var success, out var failure) ? Task.FromResult(success)
                    : failure is TaskCanceledException ? TaskExtensions.Canceled<T>() 
                    : failure.ToTask<T>();

            var result = new TaskCompletionSource<T>();
            resultTask.ContinueWith(
                (t, o) =>
                {
                    var r = (TaskCompletionSource<T>)o;
                    if (t.IsCanceled)
                        r.TrySetCanceled();
                    else if (resultTask.Result.TryGet(out var success, out var failure))
                        r.TrySetResult(success);
                    else if (failure is TaskCanceledException)
                        r.TrySetCanceled();
                    else
                        r.TrySetException(failure);
                },
                result);
            return result.Task;
        }

        public static async Task<Result<T>> ToResulTask<T, TAwaiter>(this Awaitable<T, TAwaiter> awaitable) 
            where TAwaiter : IResultAwaiter<T>
            => await awaitable;

        public static Task<T> ToTask<T, TAwaiter>(this Awaitable<T, TAwaiter> awaitable) 
            where TAwaiter : IResultAwaiter<T> 
            => awaitable.ToResulTask().Unwrap();

        public static Awaitable<TResult, Awaiter<TResult>> Select<T, TAwaiter, TResult>(
            this Awaitable<T, TAwaiter> awaitable,
            Func<T, Task<TResult>> selector)
            where TAwaiter : IResultAwaiter<T>
            => awaitable.Select(v => selector(v).ToResult());

        public static Awaitable<TResult, Awaiter<TResult>> Select<T, TAwaiter, TResult, TResultAwaiter>(
            this Awaitable<T, TAwaiter> awaitable,
            Func<T, Awaitable<TResult, TResultAwaiter>> selector)
            where TAwaiter : IResultAwaiter<T>
            where TResultAwaiter : IResultAwaiter<TResult>
        {
            return Async().ToResult();
            async Task<Result<TResult>> Async() => (await awaitable).TryGet(out var success, out var failure) 
                ? await selector(success) 
                : failure.ToFail<TResult>();
        }

        public static Awaitable<TResult, Awaiter<TResult>> Using<T, TAwaiter, TResult, TResultAwaiter>(
            this Awaitable<T, TAwaiter> awaitable,
            Func<T, Awaitable<TResult, TResultAwaiter>> selector)
            where TAwaiter : IResultAwaiter<T>
            where TResultAwaiter : IResultAwaiter<TResult>
            where T: IDisposable
            => awaitable.Select(v =>
            {
                using (v)
                    return selector(v);
            });

        public static Awaitable<TResult, Awaiter<TResult>> Using<T, TAwaiter, TResult, TResultAwaiter>(
            this Awaitable<Usable<T>, TAwaiter> awaitable,
            Func<T, Awaitable<TResult, TResultAwaiter>> selector)
            where TAwaiter : IResultAwaiter<Usable<T>>
            where TResultAwaiter : IResultAwaiter<TResult>
            => awaitable.Using(v => v.Unwrap(t => selector(t)));

        public static Awaitable<TResult, Awaiter<TResult>> Using<T, TAwaiter, TResult>(
            this Awaitable<Usable<T>, TAwaiter> awaitable,
            Func<T, Task<TResult>> selector)
            where TAwaiter : IResultAwaiter<Usable<T>>
            => awaitable.Using(v => selector(v).ToResult());

        public static Awaitable<TResult, Awaiter<TResult>> Using<T, TAwaiter, TResult>(
            this Awaitable<T, TAwaiter> awaitable,
            Func<T, Task<TResult>> selector)
            where TAwaiter : IResultAwaiter<T>
            where T : IDisposable
            => awaitable.Using(v => selector(v).ToResult());

        public readonly struct Awaitable<T, TAwaiter> where TAwaiter : IResultAwaiter<T>
        {
            public Awaitable(TAwaiter awaiter) => _awaiter = awaiter;

            public TAwaiter GetAwaiter() => _awaiter;

            private readonly TAwaiter _awaiter;
        }

        public readonly struct TaskAwaiter<T> : IResultAwaiter<T>
        {
            public TaskAwaiter(Task<T> task) => _task = task;

            public void OnCompleted(Action continuation) => Inner.OnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation) => Inner.UnsafeOnCompleted(continuation);

            public bool IsCompleted => _task.IsCompleted;

            public Result<T> GetResult() 
                => _task.TryGetException(out var e) ? e.ToFail<T>() 
                : _task.IsCanceled ? new TaskCanceledException().ToFail<T>()
                : _task.Result.ToSuccess();

            private ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter Inner => _task.ConfigureAwait(false).GetAwaiter();

            private readonly Task<T> _task;
        }

        public readonly struct ValueAwaiter<T> : IResultAwaiter<T>
        {
            public ValueAwaiter(ValueTask<T> task) => _task = task;

            public void OnCompleted(Action continuation) => Inner.OnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation) => Inner.UnsafeOnCompleted(continuation);

            public bool IsCompleted => _task.IsCompleted;

            public Result<T> GetResult()
                => _task.TryGetException(out var e) ? e.ToFail<T>()
                : _task.IsCanceled ? new TaskCanceledException().ToFail<T>()
                : _task.Result.ToSuccess();

            private ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter Inner => _task.ConfigureAwait(false).GetAwaiter();

            private readonly ValueTask<T> _task;
        }

        public readonly struct Awaiter<T> : IResultAwaiter<T>
        {
            public Awaiter(Task<Result<T>> task) => _task = task;

            public void OnCompleted(Action continuation) => Inner.OnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation) => Inner.UnsafeOnCompleted(continuation);

            public bool IsCompleted => _task.IsCompleted;

            public Result<T> GetResult()
                => _task.TryGetException(out var e) ? e.ToFail<T>()
                : _task.IsCanceled ? new TaskCanceledException().ToFail<T>()
                : _task.Result;

            private ConfiguredTaskAwaitable<Result<T>>.ConfiguredTaskAwaiter Inner => _task.ConfigureAwait(false).GetAwaiter();

            private readonly Task<Result<T>> _task;
        }

        public readonly struct Awaiter : IResultAwaiter<Unit>
        {
            public Awaiter(Task task) => _task = task;

            public void OnCompleted(Action continuation) => Inner.OnCompleted(continuation);

            public void UnsafeOnCompleted(Action continuation) => Inner.UnsafeOnCompleted(continuation);

            public bool IsCompleted => _task.IsCompleted;

            public Result<Unit> GetResult()
                => _task.TryGetException(out var e) ? e.ToFail<Unit>()
                : _task.IsCanceled ? new TaskCanceledException().ToFail<Unit>()
                : Unit;

            private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter Inner => _task.ConfigureAwait(false).GetAwaiter();

            private readonly Task _task;
        }
    }

    public readonly struct Result<T, TE> : IResult<T, TE, Result<T, TE>> where TE : class
    {
        public Result(T value) => (Value, Error) = (value, default);
        public Result(TE error) => (Value, Error) = (default, error.ToRefSome());

        private T Value { get; }
        private RefOption<TE> Error { get; }

        public bool TryGet(out T value)
        {
            var result = !Error.TryGet(out var _);
            value = result ? Value : default;
            return result;
        }

        public bool TryGetError(out TE failure) => Error.TryGet(out failure);

        public bool TryGet(out T success, out TE failure)
        {
            var result = !Error.TryGet(out failure);
            success = result ? Value : default;
            return result;
        }

        public Result<T, TE> Some(T value) => new Result<T, TE>(value);

        public Result<T, TE> Failure(TE error) => new Result<T, TE>(error);

        public override string ToString() => TryGet(out var success, out var failure) ? $"Success{{{success}}}" : $"Error{{{failure}}}";
    }

    public readonly struct Result<T, TE, TR> : IResult<T, TE, TR>
        where TR : struct, IResult<T, TE, TR>
    {
        public Result(TR inner) => Inner = inner;

        public TR Inner { get; }

        public TR Failure(TE error) => Inner.Failure(error);

        public TR Some(T value) => Inner.Some(value);

        public bool TryGet(out T success, out TE failure) => Inner.TryGet(out success, out failure);

        public bool TryGet(out T value) => Inner.TryGet(out value);

        public bool TryGetError(out TE failure) => Inner.TryGetError(out failure);

        public static implicit operator TR(Result<T, TE, TR> result) => result.Inner;

        public static implicit operator Result<T, TE, TR>(TR result) => new Result<T, TE, TR>(result);
    }

    /// <summary>
    /// Result: success with value or failure with exception
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        private T InternalValue { get; }
        [AllowNull]
        private Exception InternalError { get; }

        public bool IsFail => InternalError != null;
        public bool IsSuccess => InternalError == null;
        public Result<T, Exception, Result<T>> Generic => this;

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

        public Result<T> Some(T value) => new Result<T>(value);
        public Result<T> Failure(Exception error) => new Result<T>(error);
    }
}
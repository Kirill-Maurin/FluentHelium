using System;
using FluentHelium.Module;
using NullGuard;

namespace FluentHelium.Bdd
{
    public static class GivenWhenThenExtensions
    {
        public static GivenResult<T, T> Given<T>([AllowNull]T result) => new GivenResult<T, T>(result, result);

        public static GivenResult<T, T> Given<T>(Func<T> result) => Given(result());

        public static GivenResult<TResult, TMock> And<T, TMock, TResult>(this GivenResult<T, TMock> givenResult, Func<T, TMock, TResult> and) => 
            new GivenResult<TResult, TMock>(and(givenResult.Result, givenResult.Mock), givenResult.Mock);

        public static WhenResult<TResult, TMock> When<T, TMock, TResult>(this GivenResult<T, TMock> givenResult, Func<T, TResult> when) =>
            givenResult.When((r, m) => when(r));

        public static WhenResult<TResult, TMock> When<T, TMock, TResult>(this GivenResult<T, TMock> givenResult, Func<T, TMock, TResult> when) =>
            new WhenResult<TResult, TMock>(givenResult.Result.Try(r => when(r, givenResult.Mock)), givenResult.Mock);

        public static WhenResult<TResult, TMock> And<T, TMock, TResult>(this WhenResult<T, TMock> whenResult, Func<T, TMock, TResult> and) =>
            new WhenResult<TResult, TMock>(whenResult.Result.AndTry(r => and(r, whenResult.Mock)), whenResult.Mock);

        public static WhenResult<T, TMock> When<T, TMock>(this GivenResult<T, TMock> givenResult, Action<T> when) 
            => givenResult.When(o => 
            {
                when(o);
                return o;

            });

        public static WhenResult<T, TMock> When<T, TMock>(this GivenResult<T, TMock> givenResult, Action<T, TMock> when) 
            => givenResult.When(o =>
            {
                when(o, givenResult.Mock);
                return o;
            });

        public static WhenResult<T, TMock> And<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T, TMock> and) 
            => new WhenResult<T, TMock>(whenResult.Result.AndTry(r => and(r, whenResult.Mock)), whenResult.Mock);

        public static GivenResult<T, TMock> And<T, TMock>(this GivenResult<T, TMock> givenResult, Action<T> and)
        {
            and(givenResult.Result);
            return givenResult;
        }

        public static GivenResult<T, TMock> And<T, TMock>(this GivenResult<TMock, object> givenResult, Func<TMock, T> and) => 
            new GivenResult<T, TMock>(and(givenResult.Result), givenResult.Result);

        public static GivenResult<TResult, TMock> And<T, TMock, TResult>(this GivenResult<T, TMock> givenResult, Func<T, TResult> and) => 
            givenResult.And((o, m) => and(o));

        public static WhenResult<TResult, TMock> And<T, TMock, TResult>(this WhenResult<T, TMock> whenResult, Func<T, TResult> and) => 
            whenResult.And((o, m) => and(o));

        public static WhenResult<T, TMock> And<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T> and) =>
            whenResult.And((o, m) => and(o));
        

        public static ThenResult<T, TMock> Then<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T> then) =>
            whenResult.Then((r, m) => then(r));

        private static Exception CreateThenException(Exception e) => 
            new InvalidOperationException($"Test has thrown an exception {e.GetType().Name}:{e.Message}", e);

        private static InvalidOperationException CreateThenSuccessException() => 
            new InvalidOperationException("Test has thrown no exception");

        public static ThenResult<T, TMock> ThenCatch<T, TMock>(this WhenResult<T, TMock> whenResult, Action<Exception> then)
        {
            then(whenResult.Result.UnwrapFail(CreateThenSuccessException));
            return new ThenResult<T, TMock>(whenResult.Result, whenResult.Mock);
        }

        public static ThenResult<T, TMock> Then<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T, TMock> then)
        {
            then(whenResult.Result.Unwrap(CreateThenException), whenResult.Mock);
            return new ThenResult<T, TMock>(whenResult.Result, whenResult.Mock);
        }

        public static ThenResult<T, TMock> ThenMock<T, TMock>(this WhenResult<T, TMock> whenResult, Action<TMock> then) 
            => whenResult.Then((r, m) => then(m));

        public static ThenResult<T, TMock> And<T, TMock>(this ThenResult<T, TMock> thenResult, Action<T> and) =>
            thenResult.And((r, m) => and(r));

        public static ThenResult<T, TMock> And<T, TMock>(this ThenResult<T, TMock> thenResult, Action<T, TMock> and)
        {
            and(thenResult.Result.Unwrap(CreateThenException), thenResult.Mock);
            return thenResult;
        }

        public static ThenResult<T, TMock> AndMock<T, TMock>(this ThenResult<T, TMock> thenResult, Action<TMock> and) =>
            thenResult.And((r, m) => and(m));
    }
}
using System;
using FluentAssertions;

namespace FluentHelium.Module.Tests.BDD
{
    public static class GivenWhenThenExtensions
    {
        public static GivenResult<T, object> Given<T>(T result) => new GivenResult<T, object>(result, null);
        public static GivenResult<T, object> Given<T>(Func<T> result) => new GivenResult<T, object>(result(), null);

        public static GivenResult<TResult, TMock> And<T, TMock, TResult>(this GivenResult<T, TMock> givenResult, Func<T, TMock, TResult> and) => 
            new GivenResult<TResult, TMock>(and(givenResult.Result, givenResult.Mock), givenResult.Mock);

        public static WhenResult<TResult, TMock> When<T, TMock, TResult>(this GivenResult<T, TMock> givenResult, Func<T, TResult> when)
        {
            try
            {
                return new WhenResult<TResult, TMock>(when(givenResult.Result), givenResult.Mock);
            }
            catch (Exception e)
            {
                return new WhenResult<TResult, TMock>(default(TResult), givenResult.Mock, e);
            }
        }

        public static WhenResult<TResult, TMock> When<T, TMock, TResult>(this GivenResult<T, TMock> givenResult, Func<T, TMock, TResult> when)
        {
            try
            {
                return new WhenResult<TResult, TMock>(when(givenResult.Result, givenResult.Mock), givenResult.Mock);
            }
            catch (Exception e)
            {
                return new WhenResult<TResult, TMock>(default(TResult), givenResult.Mock, e);
            }
        }

        public static WhenResult<TResult, TMock> And<T, TMock, TResult>(this WhenResult<T, TMock> whenResult, Func<T, TMock, TResult> and)
        {
            if (whenResult.Exception != null)
                return new WhenResult<TResult, TMock>(default(TResult), whenResult.Mock, whenResult.Exception);
            try
            {
                return new WhenResult<TResult, TMock>(and(whenResult.Result, whenResult.Mock), whenResult.Mock);
            }
            catch (Exception e)
            {
                return new WhenResult<TResult, TMock>(default(TResult), whenResult.Mock, e);
            }
        }

        public static WhenResult<T, TMock> When<T, TMock>(this GivenResult<T, TMock> givenResult, Action<T> when)
        {
            return givenResult.When(o =>
            {
                when(o);
                return o;
            });
        }

        public static WhenResult<T, TMock> When<T, TMock>(this GivenResult<T, TMock> givenResult, Action<T, TMock> when)
        {
            return givenResult.When(o =>
            {
                when(o, givenResult.Mock);
                return o;
            });
        }

        public static WhenResult<T, TMock> And<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T, TMock> and)
        {
            return whenResult.And((o, m) =>
            {
                and(o, m);
                return o;
            });
        }

        public static ThenResult<T, TMock> Then<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T, TMock, Exception> then)
        {
            then(whenResult.Result, whenResult.Mock, whenResult.Exception);
            return new ThenResult<T, TMock>(whenResult.Result, whenResult.Mock, whenResult.Exception);
        }

        public static ThenResult<T, TMock> And<T, TMock>(this ThenResult<T, TMock> thenResult, Action<T, TMock, Exception> and)
        {
            and(thenResult.Result, thenResult.Mock, thenResult.Exception);
            return thenResult;
        }

        public static GivenResult<T, TMock> And<T, TMock>(this GivenResult<T, TMock> givenResult, Action<T> and)
        {
            return givenResult.And((o, m) =>
            {
                and(o);
                return o;
            });
        }

        public static GivenResult<T, TMock> And<T, TMock>(this GivenResult<TMock, object> givenResult, Func<TMock, T> and) => 
            new GivenResult<T, TMock>(and(givenResult.Result), givenResult.Result);

        public static GivenResult<TResult, TMock> And<T, TMock, TResult>(this GivenResult<T, TMock> givenResult, Func<T, TResult> and)
        {
            return givenResult.And((o, m) => and(o));
        }


        public static WhenResult<TResult, TMock> And<T, TMock, TResult>(this WhenResult<T, TMock> whenResult, Func<T, TResult> and)
        {
            return whenResult.And((o, m) => and(o));
        }

        public static WhenResult<T, TMock> And<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T> and)
        {
            return whenResult.And((o, m) =>
            {
                and(o);
                return o;
            });
        }

        public static ThenResult<T, TMock> Then<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T> then)
        {
            return whenResult.Then((r, m, e) =>
            {
                e.Should().Be(null);
                then(r);
            });
        }

        public static ThenResult<T, TMock> Then<T, TMock>(this WhenResult<T, TMock> whenResult, Func<T, bool> then)
        {
            return whenResult.Then((r, m, e) => e == null && then(r));
        }

        public static ThenResult<T, TMock> Then<T, TMock>(this WhenResult<T, TMock> whenResult, Func<T, TMock, Exception, bool> then)
        {
            return whenResult.Then((r, m, e) => { then(r, m, e).Should().Be(true); });
        }

        public static ThenResult<T, TMock> ThenCatch<T, TMock>(this WhenResult<T, TMock> whenResult, Action<Exception> then)
        {
            return whenResult.Then((r, m, e) =>
            {
                e.Should().NotBe(null);
                then(e);
            });
        }

        public static ThenResult<T, TMock> ThenCatch<T, TMock>(this WhenResult<T, TMock> whenResult, Func<Exception, bool> then)
        {
            return whenResult.Then((r, m, e) => e != null && then(e));
        }

        public static ThenResult<T, TMock> Then<T, TMock>(this WhenResult<T, TMock> whenResult, Action<T, TMock> then)
        {
            return whenResult.Then((r, m, e) =>
            {
                e.Should().Be(null);
                then(r, m);
            });
        }

        public static ThenResult<T, TMock> ThenMock<T, TMock>(this WhenResult<T, TMock> whenResult, Action<TMock> then)
        {
            return whenResult.Then((r, m, e) =>
            {
                e.Should().Be(null);
                then(m);
            });
        }

        public static ThenResult<T, TMock> ThenMock<T, TMock>(this WhenResult<T, TMock> whenResult, Func<TMock, bool> then)
        {
            return whenResult.Then((r, m, e) => e == null && then(m));
        }


        public static ThenResult<T, TMock> And<T, TMock>(this ThenResult<T, TMock> thenResult, Action<T> and)
        {
            return thenResult.And((o, m, e) => and(o));
        }

        public static ThenResult<T, TMock> And<T, TMock>(this ThenResult<T, TMock> thenResult, Action<T, TMock> and)
        {
            return thenResult.And((o, m, e) => and(o, m));
        }

        public static ThenResult<T, TMock> And<T, TMock>(this ThenResult<T, TMock> thenResult, Func<T, TMock, bool> and)
        {
            return thenResult.And((o, m, e) => and(o, m).Should().Be(true));
        }

        public static ThenResult<T, TMock> And<T, TMock>(this ThenResult<T, TMock> thenResult, Func<T, bool> and)
        {
            return thenResult.And((o, m) => and(o));
        }

        public static ThenResult<T, TMock> AndMock<T, TMock>(this ThenResult<T, TMock> thenResult, Action<TMock> and)
        {
            return thenResult.And((o, m, e) => and(m));
        }
    }
}
using System;
using FluentAssertions;
using NSubstitute;
using Xunit;
using static FluentHelium.Bdd.GivenWhenThenExtensions;
using static NSubstitute.Substitute;

namespace FluentHelium.Module.Tests
{
    public class UsableTests
    {
        [Fact]
        public void GivenDisposable_WhenDisposeThenusage_TimeShouldBeDisposed()
        {
            Given(For<IDisposable>()).
                And(usageTime => For<IDisposable>().ToUsable(usageTime)).
            When(_ => _.Dispose()).
            ThenMock(usageTime => usageTime.Received(1).Dispose());
        }

        [Fact]
        public void GivenActionDisposable_WhenDispose_ThenActionShouldBeCalledOnce()
        {
            Given(For<IDisposable>()).
                And(_ => _.ToUsable(v => v.Dispose())).
            When(_ => _.Dispose()).
            ThenMock(_ => _.Received(1).Dispose());
        }

        [Fact]
        public void GivenNeutralUsable_WhenDispose_ThenValueShouldBeNotDisposed()
        {
            Given(For<IDisposable>()).
                And(_ => _.ToUsable()).
            When(_ => _.Dispose()).
            ThenMock(_ => _.DidNotReceive().Dispose());
        }

        [Fact]
        public void GivenObject_WhenToUsable_ThenValueShouldBeSameAsObject()
        {
            Given(For<object>()).
                And(_ => _.ToUsable(For<IDisposable>())).
            When(_ => _).
            Then((_, @object) => _.Do(v => v.Should().Be(@object)));
        }

        [Fact]
        public void GivenSelfUsable_WhenDispose_ThenValueShouldBeDisposed()
        {
            Given(For<IDisposable>()).
                And(_ => _.ToSelfUsable()).
            When(_ => _.Dispose()).
            ThenMock(_ => _.Received(1).Dispose());
        }

        [Fact]
        public void GivenDisposable_WhenDisposeTwice_ThenShouldBeDisposedTwice()
        {
            Given(For<IDisposable>()).
                And(mock => For<object>().ToUsable(mock)).
            When(_ => _.Dispose()).
                And(_ => _.Dispose()).
            ThenCatch(e => e.Should().BeOfType<ObjectDisposedException>());
        }
    }
}

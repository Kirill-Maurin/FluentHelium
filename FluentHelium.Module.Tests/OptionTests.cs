using FluentAssertions;
using Xunit;
using static FluentHelium.Bdd.GivenWhenThenExtensions;

namespace FluentHelium.Module.Tests
{
    public sealed class OptionTests
    {
        [Fact]
        public void GivenReferenceNull_WhenToOption_ThenHasNoValue() 
            => Given((object)null).When(_ => _.ToOption()).Then(_ => _.TryGet(out var _).Should().BeFalse());

        [Fact]
        public void GivenReferenceNotNull_WhenToOption_ThenHasValue() 
            => Given(new object()).When(_ => _.ToOption().Generic.SelectMany(o => o.ToString().ToRefJust())).Then(_ => _.TryGet(out var _).Should().BeTrue());

        [Fact]
        public void GivenValueNull_WhenToOption_ThenHasNoValue()
            => Given((int?)null).When(_ => _.ToOption()).Then(_ => _.TryGet(out var _).Should().BeFalse());

        [Fact]
        public void GivenValueNotNull_WhenToOption_ThenHasValue() 
            => Given((int?)0).When(_ => _.ToOption()).Then(_ => _.TryGet(out var _).Should().BeTrue());
    }
}

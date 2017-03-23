using FluentAssertions;
using Xunit;
using static FluentHelium.Bdd.GivenWhenThenExtensions;

namespace FluentHelium.Module.Tests
{
    public class OptionTests
    {
        [Fact]
        public void GivenReferenceNullWhenToOptionThenHasNoValue()
        {
            Given((object)null).When(_ => _.ToOption()).Then(_ => _.HasValue.Should().BeFalse());
        }

        [Fact]
        public void GivenValueNullWhenToOptionThenHasNoValue()
        {
            Given((int?)null).When(_ => _.ToOption()).Then(_ => _.HasValue.Should().BeFalse());
        }
    }
}

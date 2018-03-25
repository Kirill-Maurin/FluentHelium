using FluentAssertions;
using FluentHelium.Base;
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
            => Given(new object()).When(_ => _.ToOption().SelectMany(o => o.ToString().ToRefSome())).Then(_ => _.TryGet(out var _).Should().BeTrue());

        [Fact]
        public void GivenValueNull_WhenToOption_ThenHasNoValue()
            => Given((int?)null).When(_ => _.ToOption()).Then(_ => _.TryGet(out var _).Should().BeFalse());

        [Fact]
        public void GivenValueNotNull_WhenToOption_ThenHasValue() 
            => Given((int?)0).When(_ => _.ToOption()).Then(_ => _.TryGet(out var _).Should().BeTrue());

        [Fact]
        public void GivenCanceledTask_WhenCanceledToNone_ThenNotCanceled()
            => Given(TaskExtensions.Canceled<object>())
                .When(async _ => await _.CanceledToNone())
                .Then(_ => _.IsCompleted.Should().BeTrue())
                    .And(_ => _.IsCanceled.Should().BeFalse())
                    .And(_ => _.Result.TryGet(out var _).Should().BeFalse());
    }
}

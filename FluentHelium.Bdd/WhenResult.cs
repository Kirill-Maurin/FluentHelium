using FluentHelium.Module;

namespace FluentHelium.Bdd
{
    public sealed class WhenResult<T, TMock> : GivenWhenThenBase<Result<T>, TMock>
    {
        internal WhenResult(Result<T> result, TMock mock) : base(result, mock) {}
    }
}
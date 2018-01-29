using FluentHelium.Module;

namespace FluentHelium.Bdd
{
    public sealed class ThenResult<T, TMock> : GivenWhenThenBase<Result<T>, TMock>
    {
        internal ThenResult(Result<T> result, TMock mock) : base(result, mock) { }
    }
}
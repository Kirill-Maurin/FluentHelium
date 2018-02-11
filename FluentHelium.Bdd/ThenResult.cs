namespace FluentHelium.Bdd
{
    public sealed class ThenResult<T, TMock> : GivenWhenThenBase<T, TMock>
    {
        internal ThenResult(T result, TMock mock) : base(result, mock) { }
    }
}
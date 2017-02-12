namespace FluentHelium.Bdd
{
    public sealed class GivenResult<T, TMock> : GivenWhenThenBase<T, TMock>
    {
        internal GivenResult(T result, TMock mock) : base(result, mock)
        {
        }
    }
}
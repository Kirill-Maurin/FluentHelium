using System;

namespace FluentHelium.Bdd
{
    public sealed class ThenResult<T, TMock> : GivenWhenThenBase<T, TMock>
    {
        internal ThenResult(T result, TMock mock, Exception e = null) : base(result, mock)
        {
            Exception = e;
        }

        internal Exception Exception { get; set; }
    }
}
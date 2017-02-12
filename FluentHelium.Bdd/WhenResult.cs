using System;

namespace FluentHelium.Bdd
{
    public sealed class WhenResult<T, TMock> : GivenWhenThenBase<T, TMock>
    {
        internal WhenResult(T result, TMock mock, Exception e = null) : base(result, mock)
        {
            Exception = e;
        }

        internal Exception Exception { get; set; }
    }
}
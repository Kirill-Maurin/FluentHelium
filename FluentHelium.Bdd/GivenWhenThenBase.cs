using NullGuard;

namespace FluentHelium.Bdd
{
    public abstract class GivenWhenThenBase<T, TMock>
    {
        internal GivenWhenThenBase([AllowNull]T result, [AllowNull]TMock mock)
        {
            Result = result;
            Mock = mock;
        }

        [AllowNull]
        internal T Result { get; }
        [AllowNull]
        internal TMock Mock { get; }
    }
}
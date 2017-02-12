namespace FluentHelium.Bdd
{
    public abstract class GivenWhenThenBase<T, TMock>
    {
        internal GivenWhenThenBase(T result, TMock mock)
        {
            Result = result;
            Mock = mock;
        }

        internal T Result { get; set; }
        internal TMock Mock { get; }
    }
}
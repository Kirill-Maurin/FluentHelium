namespace FluentHelium.Base
{
    public interface IOption<T, out TOption> : IOption<T> where TOption: struct, IOption<T, TOption>
    {
        TOption Some(T value);
    }

    public interface IOption<T>
    {
        bool TryGet(out T value);
    }
}
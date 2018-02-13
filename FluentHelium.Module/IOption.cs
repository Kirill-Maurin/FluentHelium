namespace FluentHelium.Module
{
    public interface IOption<T, out TOption> : IOption<T> where TOption: struct, IOption<T, TOption>
    {
        TOption Just(T value);
    }

    public interface IOption<T>
    {
        bool TryGet(out T value);
    }
}
using NullGuard;

namespace FluentHelium.Base
{
    public struct Some<T> : IOption<T, Some<T>>
    {
        internal Some(T value) => Unwrap = value;

        [AllowNull]
        public T Unwrap { get; }

        public bool TryGet(out T value)
        {
            value = Unwrap;
            return true;
        }

        Some<T> IOption<T, Some<T>>.Some(T value) => new Some<T>(value);
    }
}
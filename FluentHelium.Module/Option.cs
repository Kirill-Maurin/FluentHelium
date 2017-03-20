namespace FluentHelium.Module
{
    /// <summary>
    /// Nullable reference (by design)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Option<T> where T: class
    {
        internal Option(T value)
        {
            _value = value;
        }

        public static Option<T> NoValue { get; } = new Option<T>(null);

        public T GetValueOrDefault() => _value;

        private readonly T _value;
    }
}
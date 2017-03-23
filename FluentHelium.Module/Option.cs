namespace FluentHelium.Module
{
    /// <summary>
    /// Safe nullable (by design)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Option<T> 
    {
        internal Option(T value)
        {
            Value = value;
            HasValue = true;
        }

        public static Option<T> Nothing { get; } = new Option<T>();

        public T GetValue(T fallback) => HasValue ? Value : fallback;

        public bool HasValue { get; }

        internal T Value { get; }
    }
}
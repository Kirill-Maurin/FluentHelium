using NullGuard;

namespace FluentHelium.Module
{
    public static class ValOption
    {
        public static ValOption<T> From<T>(T? value) where T : struct => new ValOption<T>(value);
        public static ValOption<T> Just<T>(T value) where T : struct => new ValOption<T>(value);
        public static Option<T, ValOption<T>> ToValJust<T>(this T value) where T : struct => Just(value);
    }

    /// <summary>
    /// Safe universal nullable (by design)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ValOption<T> : IOption<T, ValOption<T>> where T: struct
    {
        internal ValOption(T value) => _value = value;
        internal ValOption(T? value) => _value = value;

        private readonly T? _value;

        public bool TryGet([AllowNull]out T value)
        {
            var result = _value.HasValue;
            value = result ? _value.Value : default;
            return result;
        }

        public ValOption<T> Just(T value) => new ValOption<T>(value);

        public static explicit operator ValOption<T>(Option<T, ValOption<T>> option) => option.Inner;

        public static explicit operator Option<T, ValOption<T>>(ValOption<T> option) => new Option<T, ValOption<T>>(option);

        public override string ToString() => TryGet(out var v) ? $"Just{{{v}}}" : $"Nothing<{nameof(T)}>";
    }
}
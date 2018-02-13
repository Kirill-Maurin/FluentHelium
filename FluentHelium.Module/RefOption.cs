using NullGuard;

namespace FluentHelium.Module {

    public static class RefOption
    {
        public static Option<T, RefOption<T>> ToRefJust<T>(this T value) where T : class => Just(value);
        public static RefOption<T> From<T>([AllowNull]T value) where T: class => new RefOption<T>(value);
        public static RefOption<T> Just<T>(T value) where T : class => From(value);
    }

    public struct RefOption<T> : IOption<T, RefOption<T>> where T: class
    {
        internal RefOption(T value) => _value = value;

        private readonly T _value;

        public T GetValue( T fallback ) => _value ?? fallback;

        public bool TryGet([AllowNull]out T value)
        {
            value = _value;
            return _value != null;
        }

        public RefOption<T> Just(T value) => new RefOption<T>(value);
        public Option<T, RefOption<T>> Generic => new Option<T, RefOption<T>>(this);

        public static explicit operator RefOption<T>(Option<T, RefOption<T>> option) => option.Inner;

        public static explicit operator Option<T, RefOption<T>>(RefOption<T> option) => new Option<T, RefOption<T>>(option);

        public override string ToString() => _value != null ? $"Just{{{_value}}}" : $"Nothing<{nameof( T )}>";
    }
}
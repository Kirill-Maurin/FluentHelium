using NullGuard;

namespace FluentHelium.Base
{

    public static class RefOption
    {
        public static RefOption<T> ToRefOption<T>([AllowNull]this T value) where T : class => From(value);
        public static Option<T, RefOption<T>> ToRefSome<T>(this T value) where T : class => Some(value);
        public static RefOption<T> Some<T>(T value) where T : class => From(value);
        public static RefOption<T> From<T>([AllowNull]T value) where T : class => RefOption<T>.From(value);
    }

    public struct RefOption<T> : IOption<T, RefOption<T>> where T: class
    {
        public static RefOption<T> From([AllowNull]T value) => new RefOption<T>(value);

        RefOption(T value) => _value = value;

        readonly T _value;

        public T GetValue( T fallback ) => _value ?? fallback;

        public bool TryGet([AllowNull]out T value)
        {
            value = _value;
            return _value != null;
        }

        public RefOption<T> Some(T value) => new RefOption<T>(value);
        public Option<T, RefOption<T>> AsGeneric => this;

        public static explicit operator RefOption<T>(Option<T, RefOption<T>> option) => option.Inner;

        public static explicit operator Option<T, RefOption<T>>(RefOption<T> option) => option;

        public override string ToString() => _value != null ? $"Some{{{_value}}}" : $"None<{nameof( T )}>";
    }
}
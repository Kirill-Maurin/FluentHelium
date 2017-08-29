namespace FluentHelium.Module
{
    public static class Property
    {
        public static IMutableProperty<T> ToProperty<T>(this T @default) => new PropertyImpl<T>(@default);
        public static IProperty<T> Create<T>() => default(T).ToProperty();
    }
}

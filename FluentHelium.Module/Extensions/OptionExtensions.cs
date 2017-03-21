namespace FluentHelium.Module
{
    public static class OptionExtensions
    {
        public static Option<T> ToOption<T>(this T source) where T: class => source != null ? new Option<T>(source) : Option<T>.NoValue;
        public static T? ToNullable<T>(this T source) where T : struct => source;
        public static bool HasValue<T>(this Option<T> option) where T : class => option.GetValueOrDefault() != null;

        public static T GetValueOrDefault<T>(this Option<T> option, T @default) where T : class => 
            option.GetValueOrDefault() ?? @default;
    }
}
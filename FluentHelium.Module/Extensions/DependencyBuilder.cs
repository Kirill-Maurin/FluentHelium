using System;
using System.Linq;

namespace FluentHelium.Module
{
    public static class DependencyBuilder
    {
        public static IModuleDependencyBuilder ToBuilder(
            this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> linkFallback, IModuleDependencyBuilder fallback = null) =>
            linkFallback.Else(fallback ?? Fail);

        public static IModuleDependencyBuilder External { get; } = new ExternalDependencyBuilder();

        public static IModuleDependencyBuilder Fail { get; } = new FailDependencyBuilder();

        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Simple { get; } = b => new SimpleDependencyBuilder(b);
        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Direct<T>(this IModuleDescriptor implementation, IModuleDescriptor client = null) =>
            b => new DirectDependencyBuilder(client, typeof(T), implementation, b);
        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Direct(this IModuleDescriptor implementation, IModuleDescriptor client = null) =>
            b => new DirectDependencyBuilder(client, null, implementation, b);
        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Optional { get; } = b => new OptionalDependencyBuilder(b);
        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Multiple { get; } = b => new MultipleDependencyBuilder(b);

        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> ForType<T>(
            this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> buildFunc) =>
            b => new FilteredDependencyBuilder(buildFunc(b), b, (c, t, i) => t == typeof(T), null);

        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> WhereClient(
            this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> buildFunc,
            Predicate<IModuleDescriptor> condition) =>
            b => new FilteredDependencyBuilder(buildFunc(b), b, (c, t, i) => condition(c), null);

        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> WhereImplementation(
            this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> buildFunc,
            Predicate<IModuleDescriptor> condition) =>
            b => new FilteredDependencyBuilder(
                buildFunc(b),
                b,
                null,
                l => l.SelectMany(p => p.Where(i => condition(i)).Select(c => c.LinkKey(p.Key))).
                    ToLookup(i => i.Key, i => i.Value));
    }
}

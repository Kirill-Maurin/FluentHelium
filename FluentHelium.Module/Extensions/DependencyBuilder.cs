using System;
using System.Linq;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    public static class DependencyBuilder
    {
        public static IModuleDependencyBuilder Or(this IModuleDependencyBuilder builder, params IModuleDependencyBuilder[] fallback) 
            => new FallbackDependencyBuilder(builder, fallback);

        public static IModuleDependencyBuilder External { get; } = new ExternalDependencyBuilder();

        public static IModuleDependencyBuilder Fail { get; } = new FailDependencyBuilder();

        public static IModuleDependencyBuilder Simple { get; } = new SimpleDependencyBuilder();
        public static IModuleDependencyBuilder Direct<T>(this IModuleDescriptor implementation, IModuleDescriptor client = null) 
            => new DirectDependencyBuilder(client, typeof(T), implementation);
        public static IModuleDependencyBuilder Direct(this IModuleDescriptor implementation, IModuleDescriptor client = null) =>
            new DirectDependencyBuilder(client, null, implementation);
        public static IModuleDependencyBuilder Optional { get; } = new OptionalDependencyBuilder();
        public static IModuleDependencyBuilder Multiple { get; } = new MultipleDependencyBuilder();

        public static IModuleDependencyBuilder ForType<T>(this IModuleDependencyBuilder buildFunc) 
            => new FilteredDependencyBuilder(buildFunc, (c, t, i) => t == typeof(T), null);

        public static IModuleDependencyBuilder WhereClient(
            this IModuleDependencyBuilder buildFunc,
            Predicate<IModuleDescriptor> condition) 
            => new FilteredDependencyBuilder(buildFunc, (c, t, i) => condition(c), null);

        public static IModuleDependencyBuilder WhereImplementation(
            this IModuleDependencyBuilder buildFunc,
            Predicate<IModuleDescriptor> condition) 
            => new FilteredDependencyBuilder(
                buildFunc,
                null,
                l => l.SelectMany(p => p.Where(i => condition(i)).Select(c => c.LinkKey(p.Key))).
                    ToLookup(i => i.Key, i => i.Value));
    }
}

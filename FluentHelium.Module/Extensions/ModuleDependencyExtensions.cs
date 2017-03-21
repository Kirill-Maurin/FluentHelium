using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHelium.Module
{
    public static class ModuleDependencyExtensions
    {
        public static readonly Guid External = new Guid("{6671930C-FC8F-4148-A596-097D94285279}");
        public static bool IsExternal(this IModuleDescriptor descriptor) => descriptor.Id == External;
        public static IModuleDescriptor ExternalModule { get; } = new ModuleDescriptor("External", External, null, null);
        public static IModuleDependencyBuilder ExternalDependencyBuilder { get; } = new ExternalDependencyBuilder();

        public static IModuleDependencyBuilder ElseExternal(
            this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> linkFallback) =>
            linkFallback(ExternalDependencyBuilder);

        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> DependencyBuilder() => b => b;
        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Simple(this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> linkFallback) =>
            b => linkFallback(new SimpleDependencyBuilder(b));
        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Optional(
            this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> linkFallback) =>
            b => linkFallback(new OptionalDependencyBuilder(b));
        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Multiple(this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> linkFallback) =>
            b => new MultipleDependencyBuilder(b);

        public static ModuleOutputDependency ToModuleOutputDependency(
            this Type @interface, IModuleDescriptor descriptor) =>
            new ModuleOutputDependency(descriptor, @interface);

        public static IModuleInputDependency ToModuleInputDependency(
            this IEnumerable<ModuleOutputDependency> dependencies,
            IModuleDescriptor client,
            Type @interface,
            Func<Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, dependencies, resolver, client);

        public static IModuleInputDependency ToModuleInputDependency(
            this Type @interface,
            IModuleDescriptor client,
            Func<Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, Enumerable.Empty<ModuleOutputDependency>(), provider => resolver(), client);

        public static IModuleInputDependency ToModuleInputDependency(
            this ModuleOutputDependency source,
            IModuleDescriptor client,
            Type @interface,
            Func<Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, new[] { source }, resolver, client);

        public static IModuleInputDependency ToModuleInputDependency(
            this IModuleDescriptor source,
            IModuleDescriptor client,
            Type @interface,
            Func<Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, new[] { @interface.ToModuleOutputDependency(source) }, resolver, client);

        public static IModuleInputDependency ToModuleInputDependency(
            this IModuleDescriptor source,
            IModuleDescriptor client,
            Type @interface,
            Func<IModuleDescriptor, Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, new[] { @interface.ToModuleOutputDependency(source) }, r => resolver(source, r), client);
    }
}

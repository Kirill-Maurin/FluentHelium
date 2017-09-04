using System;
using System.Linq;

namespace FluentHelium.Module {
    internal sealed class FilteredDependencyBuilder : IModuleDependencyBuilder
    {
        internal FilteredDependencyBuilder(
            IModuleDependencyBuilder main,
            IModuleDependencyBuilder fallback,
            Func<IModuleDescriptor, Type, ILookup<Type, IModuleDescriptor>, bool> filter,
            Func<ILookup<Type, IModuleDescriptor>, ILookup<Type, IModuleDescriptor>> implementationFilter)
        {
            _main = main;
            _fallback = fallback;
            _filter = filter;
            _implementationFilter = implementationFilter;
        }

        public IModuleInputDependency Build(IModuleDescriptor client, Type @interface, ILookup<Type, IModuleDescriptor> implementations)
        {
            var filteredImplementations = _implementationFilter != null ? _implementationFilter(implementations) : implementations;
            bool IsFiltered() => _filter == null || _filter(client, @interface, filteredImplementations);

            return filteredImplementations != null && IsFiltered()
                ? _main.Build(client, @interface, filteredImplementations) 
                : _fallback.Build(client, @interface, implementations);
        }

        private readonly IModuleDependencyBuilder _main;
        private readonly IModuleDependencyBuilder _fallback;
        private readonly Func<IModuleDescriptor, Type, ILookup<Type, IModuleDescriptor>, bool> _filter;
        private readonly Func<ILookup<Type, IModuleDescriptor>, ILookup<Type, IModuleDescriptor>> _implementationFilter;
    }
}
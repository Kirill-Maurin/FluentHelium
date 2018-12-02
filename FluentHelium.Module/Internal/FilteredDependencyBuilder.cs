using System;
using System.Linq;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    sealed class FilteredDependencyBuilder : IModuleDependencyBuilder
    {
        internal FilteredDependencyBuilder(
            IModuleDependencyBuilder main,
            Func<IModuleDescriptor, Type, ILookup<Type, IModuleDescriptor>, bool> filter,
            Func<ILookup<Type, IModuleDescriptor>, ILookup<Type, IModuleDescriptor>> implementationFilter)
            => (_main, _filter, _implementationFilter) = (main, filter, implementationFilter);

        public RefOption<IModuleInputDependency> Build(IModuleDescriptor client, Type @interface, ILookup<Type, IModuleDescriptor> implementations)
        {
            var filteredImplementations = _implementationFilter != null ? _implementationFilter(implementations) : implementations;
            bool IsFiltered() => _filter == null || _filter(client, @interface, filteredImplementations);

            return filteredImplementations != null && IsFiltered()
                ? _main.Build(client, @interface, filteredImplementations) 
                : default;
        }

        readonly IModuleDependencyBuilder _main;
        readonly Func<IModuleDescriptor, Type, ILookup<Type, IModuleDescriptor>, bool> _filter;
        readonly Func<ILookup<Type, IModuleDescriptor>, ILookup<Type, IModuleDescriptor>> _implementationFilter;
    }
}
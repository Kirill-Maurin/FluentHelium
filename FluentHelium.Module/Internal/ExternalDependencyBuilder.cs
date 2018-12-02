using FluentHelium.Base;
using System;
using System.Linq;
using static FluentHelium.Module.ModuleDependencyExtensions;

namespace FluentHelium.Module
{
    /// <inheritdoc />
    /// <summary>
    /// Trivial builder: use external implementation for any service
    /// </summary>
    sealed class ExternalDependencyBuilder : IModuleDependencyBuilder
    {
        public RefOption<IModuleInputDependency> Build(IModuleDescriptor client, Type @interface, ILookup<Type, IModuleDescriptor> implementations) =>
            ExternalModule.ToModuleInputDependency(client, @interface, provider => provider(ExternalModule).Resolve(@interface)).ToRefSome();
    }
}
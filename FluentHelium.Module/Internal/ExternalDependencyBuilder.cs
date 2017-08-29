using System;
using System.Linq;
using static FluentHelium.Module.ModuleDependencyExtensions;

namespace FluentHelium.Module
{
    /// <summary>
    /// Trivial builder: use external implementation for any service
    /// </summary>
    internal sealed class ExternalDependencyBuilder : IModuleDependencyBuilder
    {
        public IModuleInputDependency Build(IModuleDescriptor client, Type @interface, ILookup<Type, IModuleDescriptor> implementations) =>
            ExternalModule.ToModuleInputDependency(client, @interface, provider => provider(ExternalModule).Resolve(@interface));
    }
}
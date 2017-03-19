using System;
using System.Linq;

namespace FluentHelium.Module
{
    /// <summary>
    /// Trivial builder: use external implementation for any service
    /// </summary>
    public sealed class ExternalDependencyBuilder : IModuleDependencyBuilder
    {
        internal ExternalDependencyBuilder()
        { }

        public IModuleInputDependency Build(IModuleDescriptor client, Type @interface, ILookup<Type, IModuleDescriptor> implementations) =>
            ModuleExtensions.ExternalModule.ToModuleInputDependency(client, @interface, provider => provider(ModuleExtensions.ExternalModule).Resolve(@interface));
    }
}
using System;
using System.Linq;

namespace FluentHelium.Module
{
    internal sealed class FailDependencyBuilder : IModuleDependencyBuilder
    {
        public IModuleInputDependency Build(IModuleDescriptor client, Type @interface, ILookup<Type, IModuleDescriptor> implementations) =>
            throw new ArgumentException($"Cannot resovle dependency {@interface.Name} for module {client.Name}({client.Id})");
    }
}
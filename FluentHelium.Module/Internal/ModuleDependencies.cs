using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentHelium.Module
{
    internal sealed class ModuleDependencies : IModuleDependencies
    {
        internal ModuleDependencies(IImmutableDictionary<Type, IModuleInputDependency> dependencies, IModuleDescriptor client)
        {
            _dependencies = dependencies;
            Client = client;
            Links = _dependencies.Values.
                SelectMany(d => d.Output.Select(o => new ModuleLink(d.Input, o.Output, d.Client, o.Implementation))).
                ToLookup(l => l.Implementation);
        }

        public IModuleDescriptor Client { get; }
        public int Count => _dependencies.Count;
        public IModuleInputDependency this[Type @interface] => _dependencies[@interface];
        public ILookup<IModuleDescriptor, ModuleLink> Links { get; }

        public IEnumerator<IModuleInputDependency> GetEnumerator() => _dependencies.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        private readonly IImmutableDictionary<Type, IModuleInputDependency> _dependencies;
    }
}
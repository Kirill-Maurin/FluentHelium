using System;
using System.Collections.Generic;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    public sealed class ModuleInputDependency : IModuleInputDependency
    {
        internal ModuleInputDependency(Type input, IEnumerable<ModuleOutputDependency> output,
            Func<Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver, IModuleDescriptor client)
        {
            _resolver = resolver;
            Client = client;
            Input = input;
            Output = output;
        }
        public Type Input { get; }
        public IEnumerable<ModuleOutputDependency> Output { get; }

        public Usable<object> Resolve(Func<IModuleDescriptor, IDependencyProvider> provider) =>
            _resolver(provider);

        private readonly Func<Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> _resolver;
        public IModuleDescriptor Client { get; }
    }
}
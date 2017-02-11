using System;

namespace FluentHelium.Module
{
    internal sealed class Module : IModule
    {
        public Module(IModuleDescriptor descriptor, Func<IDependencyProvider, Usable<IDependencyProvider>> activator)
        {
            Descriptor = descriptor;
            _activator = activator;
        }

        public IModuleDescriptor Descriptor { get; }
        public Usable<IDependencyProvider> Activate(IDependencyProvider dependencies) => _activator(dependencies);

        private readonly Func<IDependencyProvider, Usable<IDependencyProvider>> _activator;
    }
}

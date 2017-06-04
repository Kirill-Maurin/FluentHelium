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

        public Usable<IDependencyProvider> Activate(IDependencyProvider dependencies)
        {
            if (Active)
                throw new InvalidOperationException($"Module {Descriptor.Name}({Descriptor.Id}) already activated");
            try
            {
                Active = true;
                return _activator(dependencies).ToUsable(() => Active = false);
            }
            catch
            {
                Active = false;
                throw;
            }
        }

        public bool Active { get; private set; }

        public override string ToString() => ModuleExtensions.ToString(this);

        private readonly Func<IDependencyProvider, Usable<IDependencyProvider>> _activator;
    }
}

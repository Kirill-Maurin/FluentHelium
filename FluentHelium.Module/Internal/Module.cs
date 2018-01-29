using System;

namespace FluentHelium.Module
{
    internal sealed class Module : IModule
    {
        public Module(IModuleDescriptor descriptor, Func<IDependencyProvider, Usable<IDependencyProvider>> activator)
        {
            Descriptor = descriptor;
            _activator = activator;
            _active = false.ToProperty();
        }

        public IModuleDescriptor Descriptor { get; }

        public Usable<IDependencyProvider> Activate(IDependencyProvider dependencies)
        {
            if (Active.Value)
                throw new InvalidOperationException($"Module {Descriptor.Name}({Descriptor.Id}) already activated");
            try
            {
                _active.OnNext(true);
                return _activator(dependencies).Wrap(() => _active.OnNext(false));
            }
            catch
            {
                _active.OnNext(false);
                throw;
            }
        }

        public IProperty<bool> Active => _active;

        public override string ToString() => ModuleExtensions.ToString(this);

        private readonly Func<IDependencyProvider, Usable<IDependencyProvider>> _activator;
        private readonly IMutableProperty<bool> _active;
    }
}

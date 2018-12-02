using Autofac;
using FluentHelium.Base;
using FluentHelium.Module;
using System;

namespace FluentHelium.Autofac
{
    sealed class AutofacModule : IModule
    {
        public AutofacModule(IModuleDescriptor descriptor, Action<ContainerBuilder> registrator)
        {
            _registrator = registrator;
            Descriptor = descriptor;
            _active = false.ToProperty();
        }

        public IModuleDescriptor Descriptor { get; }
        public Usable<IDependencyProvider> Activate(IDependencyProvider dependencies)
        {
            if (Active.Value)
                throw new InvalidOperationException($"Module {Descriptor.Name}({Descriptor.Id}) already activated");
            var builder = new ContainerBuilder();
            foreach (var dependency in dependencies.Dependencies)
                builder.RegisterDependency(dependency, c => dependencies.Resolve(dependency));
            _registrator(builder);
            try
            {
                return builder.
                    Build().
                    ToSelfUsable().
                    Wrap(c => _active.OnNext(true), c => _active.OnNext(false)).
                    Select(c => Descriptor.Output.ToDependencyProvider(t => c.Resolve(t).ToUsable()));
            }
            catch
            {
                _active.OnNext(false);
                throw;
            }
        }

        public override string ToString() => $"Autofac {base.ToString()}";

        public IProperty<bool> Active => _active;

        readonly Action<ContainerBuilder> _registrator;
        readonly IMutableProperty<bool> _active;
    }
}

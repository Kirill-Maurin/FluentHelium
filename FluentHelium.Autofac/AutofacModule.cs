using System;
using Autofac;
using FluentHelium.Module;
using IModule = FluentHelium.Module.IModule;

namespace FluentHelium.Autofac
{
    internal sealed class AutofacModule : IModule
    {
        private readonly Action<ContainerBuilder> _registrator;

        public AutofacModule(IModuleDescriptor descriptor, Action<ContainerBuilder> registrator)
        {
            _registrator = registrator;
            Descriptor = descriptor;
        }

        public IModuleDescriptor Descriptor { get; }
        public Usable<IDependencyProvider> Activate(IDependencyProvider dependencies)
        {
            var builder = new ContainerBuilder();
            foreach (var dependency in dependencies.Dependencies)
            {
                builder.RegisterDependency(dependency, c => dependencies.Resolve(dependency));
            }
            _registrator(builder);
            return builder.
                Build().
                ToSelfUsable().
                Select(c => Descriptor.Output.ToDependencyProvider(t => c.Resolve(t).ToUsable()));
        }
    }
}

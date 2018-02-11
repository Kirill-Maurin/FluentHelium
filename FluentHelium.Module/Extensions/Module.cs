using System;
using System.Linq;
using static FluentHelium.Module.ModuleDescriptor;
using static FluentHelium.Module.EnumerableExtensions;

namespace FluentHelium.Module
{
    public static class Module
    { 
        public static IModule CreateSimpleModule<TInput, TOutput>(string name, Func<TInput, TOutput> activate) => 
            CreateSimpleDescriptor<TInput, TOutput>(name).ToModule(p => p.Resolve<TInput>().Select(i => activate(i).ToDependencyProvider()));

        public static IModule CreateSimpleModule<T>(string name, Func<T> activate) => 
            CreateSimpleDescriptor<T>(name).ToModule(p => activate().ToDependencyProvider().ToUsable());

        public static IModule CreateSimpleModule<T>(string name, Action<T> activate) =>
            typeof(T).ToConsumerModuleDescriptor(name).ToModule(p => p.Resolve<T>().SelectMany(i => {
                activate(i);
                return DependencyProvider.Empty.ToUsable();
            }));

        public static IModule ToModule(
            this IModuleDescriptor descriptor,
            Func<IDependencyProvider, Usable<IDependencyProvider>> activator) =>
            new Implementation(descriptor, activator);

        public static string ToString(this IModule module) =>
            $"{(module.Active.Value ? "Active" : "Inactive")} module {module.Descriptor}";

        public static string ToPlantUml(this IModule module) =>
            string.Join("\n",
                $"note left of [{module.Descriptor.Name}] : {(module.Active.Value ? "Active" : "Inactive")}",
                module.Descriptor.ToPlantUml());

        public static string ToPlantUml(this IModuleDescriptor descriptor) =>
            string.Join("\n",
                CreateEnumerable($"note right of [{descriptor.Name}] : {descriptor.Id}").
                Concat(descriptor.Input.Select(t => $"[{descriptor.Name}] .d.> {t.Name}")).
                Concat(descriptor.Output.Select(t => $"[{descriptor.Name}] -u-> {t.Name}")));

        private sealed class Implementation : IModule
        {
            public Implementation(IModuleDescriptor descriptor, Func<IDependencyProvider, Usable<IDependencyProvider>> activator)
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

            public override string ToString() => Module.ToString(this);

            private readonly Func<IDependencyProvider, Usable<IDependencyProvider>> _activator;
            private readonly IMutableProperty<bool> _active;
        }
    }
}
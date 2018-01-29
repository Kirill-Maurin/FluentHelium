using System;
using System.Linq;
using static FluentHelium.Module.ModuleDescriptorExtensions;
using static FluentHelium.Module.EnumerableExtensions;

namespace FluentHelium.Module
{
    public static class ModuleExtensions
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
            new Module(descriptor, activator);

        public static string ToString(this IModule module) =>
            $"{(module.Active.Value ? "Active" : "Inactive")} module {module.Descriptor}";

        public static string ToPlantUml(this IModule module) =>
            string.Join("\n",
                $"note left of [{module.Descriptor.Name}] : {(module.Active.Value ? "Active" : "Inactive")}",
                module.Descriptor.ToPlantUml());

        public static string ToPlantUml(this IModuleDescriptor descriptor) =>
            string.Join("\n",
                Enumerable($"note right of [{descriptor.Name}] : {descriptor.Id}").
                Concat(descriptor.Input.Select(t => $"[{descriptor.Name}] .d.> {t.Name}")).
                Concat(descriptor.Output.Select(t => $"[{descriptor.Name}] -u-> {t.Name}")));
    }
}
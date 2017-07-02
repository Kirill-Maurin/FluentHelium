using System;
using static FluentHelium.Module.ModuleDescriptorExtensions;

namespace FluentHelium.Module
{
    public static class ModuleExtensions
    { 
        public static IModule CreateSimpleModule<TInput, TOutput>(string name, Func<TInput, TOutput> activate) => 
            CreateSimpleDescriptor<TInput, TOutput>(name).ToModule(p => p.Resolve<TInput>().Select(i => activate(i).ToDependencyProvider()));

        public static IModule CreateSimpleModule<T>(string name, Func<T> activate) => 
            CreateSimpleDescriptor<T>(name).ToModule(p => activate().ToDependencyProvider().ToUsable());

        public static IModule ToModule(
            this IModuleDescriptor descriptor,
            Func<IDependencyProvider, Usable<IDependencyProvider>> activator) =>
            new Module(descriptor, activator);

        public static string ToString(this IModule module) =>
            $"{(module.Active.Value ? "Active" : "Inactive")} module {module.Descriptor}";
    }
}
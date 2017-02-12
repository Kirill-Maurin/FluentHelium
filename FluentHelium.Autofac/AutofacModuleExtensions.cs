using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using FluentHelium.Module;
using System.Linq;

namespace FluentHelium.Autofac
{
    public static class AutofacModuleExtensions
    {
        public static void RegisterFluentHeliumModule<T>(this ContainerBuilder builder, IModule module) =>
            builder.RegisterFluentHeliumModule(module, new [] { typeof(T) });

        public static void RegisterFluentHeliumModule(this ContainerBuilder builder, IModule module) =>
            builder.RegisterFluentHeliumModule(module, module.Descriptor.Output);

        public static void RegisterFluentHeliumModule(this ContainerBuilder builder, IModule module, IEnumerable<Type> types)
        {
            var dependencies = types.ToArray();
            if (!module.Descriptor.Output.IsSupersetOf(dependencies))
                throw new ArgumentException($"Module {module.Descriptor.Name} doesn't implement at least one of target types", nameof(types));

            builder.Register(c => 
                module.Activate(module.Descriptor.Input.ToDependencyProvider(t => c.Resolve(t).ToUsable()))).
                SingleInstance().
                Keyed<Usable<IDependencyProvider>>(module.Descriptor.Id);
            builder.Register(c => c.ResolveKeyed<Usable<IDependencyProvider>>(module.Descriptor.Id).Unwrap(r => r)).
                Keyed<IDependencyProvider>(module.Descriptor.Id);
            foreach (var dependency in dependencies)
            {
                builder.RegisterDependency(dependency, c => c.ResolveKeyed<IDependencyProvider>(module.Descriptor.Id).Resolve(dependency));
            }
        }

        public static void RegisterDependency(this ContainerBuilder builder, Type dependency, Func<IComponentContext, Usable<object>> action) =>
            typeof (AutofacModuleExtensions).
                GetTypeInfo().
                GetDeclaredMethods(nameof(RegisterDependency)).
                First(m => m.IsGenericMethod).
                MakeGenericMethod(dependency).
                Invoke(null, new object[] {builder, action});


        public static IModule ToAutofacModule(this IModuleDescriptor descriptor, Action<ContainerBuilder> registrator) =>
            new AutofacModule(descriptor, registrator);

        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterDependency<T>(
            this ContainerBuilder container, Func<IComponentContext, Usable<object>> provider) where T: class
        {
            container.Register(c => provider(c).Select(p => (T)p)).As<Usable<T>>().SingleInstance();
            return container.Register(c => c.Resolve<Usable<T>>().Unwrap(r => r));
        }
    }
}
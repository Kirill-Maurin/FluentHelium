using System;
using System.Linq;
using System.Reflection;

namespace FluentHelium.Module
{
    internal sealed class OptionalDependencyBuilder : IModuleDependencyBuilder
    {
        internal OptionalDependencyBuilder(IModuleDependencyBuilder fallback)
        {
            _fallback = fallback;
        }

        public IModuleInputDependency Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations)
        {
            if (!@interface.IsConstructedGenericType)
                return _fallback.Build(client, @interface, implementations);
            var t = @interface.GetGenericTypeDefinition();
            if (t != typeof(Option<>) && t != typeof(Nullable<>))
                return _fallback.Build(client, @interface, implementations);
            var parameter = @interface.GenericTypeArguments[0];
            var implementation = implementations[parameter].FirstOrDefault();
            if (implementation == null)
                return _fallback.Build(client, @interface, implementations);
            return parameter.
                ToModuleOutputDependency(implementation).
                ToModuleInputDependency(
                    client,
                    @interface,
                    provider => provider(implementation).Resolve(parameter).Select(
                        o => CreateOption(o, parameter.IsByRef ? nameof(OptionExtensions.ToOption) : nameof(OptionExtensions.ToNullable))));
        }

        private object CreateOption(object o, string methodName) =>
            typeof(OptionExtensions).
                GetTypeInfo().
                GetDeclaredMethods(methodName).
                First(m => m.IsGenericMethod).
                MakeGenericMethod(o.GetType()).
                Invoke(null, new[] {o});

        private readonly IModuleDependencyBuilder _fallback;
    }
}
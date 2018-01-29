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
            if (t != typeof(Option<>))
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
                        o => CreateOption(o, parameter)));
        }

        private object CreateOption(object o, Type t) =>
            typeof(Option).
                GetTypeInfo().
                GetDeclaredMethods(nameof(Option.ToOption)).
                Where(m => m.IsGenericMethod).
                First(m =>
                {
                    var generics = m.GetGenericArguments();
                    var isReference = (generics.First().GetTypeInfo().GenericParameterAttributes &
                        GenericParameterAttributes.ReferenceTypeConstraint) != 0;
                    return !isReference ^ t.GetTypeInfo().IsClass;
                }).
                MakeGenericMethod(t).
                Invoke(null, new[] {o});

        private readonly IModuleDependencyBuilder _fallback;
    }
}
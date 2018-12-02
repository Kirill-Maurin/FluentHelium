using FluentHelium.Base;
using System;
using System.Linq;
using System.Reflection;

namespace FluentHelium.Module
{
    sealed class OptionalDependencyBuilder : IModuleDependencyBuilder
    {
        public RefOption<IModuleInputDependency> Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations)
        {
            if (!@interface.IsConstructedGenericType)
                return default;
            var t = @interface.GetGenericTypeDefinition();
            if (t != typeof(Option<>) && t != typeof(RefOption<>) && t != typeof(Option<,>))
                return default;
            var parameter = @interface.GenericTypeArguments[0];
            var implementation = implementations[parameter].FirstOrDefault();
            if (implementation == null)
                return default;
            return parameter.
                ToModuleOutputDependency(implementation).
                ToModuleInputDependency(
                    client,
                    @interface,
                    provider => provider(implementation).Resolve(parameter).Select(
                        o => CreateOption(t, o, parameter))).
                ToRefSome();
        }

        Type GetOptionType(Type option)
        {
            if (option == typeof(Option<>))
                return typeof(Option);
            return option == typeof(RefOption<>) ? typeof(RefOption) : typeof(Option<,>);
        }

        object CreateOption(Type option, object o, Type t) =>
            GetOptionType(option).
                GetTypeInfo().
                GetDeclaredMethods(nameof(Option.From)).
                Where(m => m.IsGenericMethod).
                First(m =>
                {
                    var generics = m.GetGenericArguments();
                    var isReference = (generics.First().GetTypeInfo().GenericParameterAttributes &
                        GenericParameterAttributes.ReferenceTypeConstraint) != 0;
                    return !isReference ^ t.GetTypeInfo().IsClass;
                }).
                MakeGenericMethod(t).
                Invoke(null, new[] { o });
    }
}
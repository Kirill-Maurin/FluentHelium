using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    internal sealed class MultipleDependencyBuilder : IModuleDependencyBuilder
    {
        public RefOption<IModuleInputDependency> Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations)
        {
            if (!@interface.IsConstructedGenericType)
                return default;
            var t = @interface.GetGenericTypeDefinition();
            if (t != typeof(IEnumerable<>))
                return default;
            var parameter = @interface.GenericTypeArguments[0];
            var enumerables = implementations[@interface].Select(@interface.ToModuleOutputDependency);
            return implementations[parameter].
                Select(parameter.ToModuleOutputDependency).Concat(enumerables).ToModuleInputDependency(
                    client,
                    @interface,
                    p =>
                    {
                        var singlets =
                            implementations[parameter].Select(d => p(d).Resolve(parameter)).
                                ToAggregatedUsable().
                                Select(e => (IEnumerable)e);
                        var multiplets =
                            implementations[@interface].Select(d => p(d).Resolve(@interface).Select(o => (IEnumerable)o));
                        return new [] { singlets }.
                            Concat(multiplets).
                            ToAggregatedUsable().
                            Select(e => Cast(SelectMany(e), parameter));
                    }).ToRefSome();
        }

        private IEnumerable SelectMany(IEnumerable<IEnumerable> source)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var e in source)
            foreach (var o in e)
                yield return o;
        }

        private object Cast(IEnumerable e, Type type) =>
            typeof(Enumerable).
                GetTypeInfo().
                GetDeclaredMethods(nameof(Enumerable.Cast)).
                First(m => m.IsGenericMethod).
                MakeGenericMethod(type).
                Invoke(null, new[] { (object)e });
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentHelium.Module
{
    internal sealed class MultipleDependencyBuilder : IModuleDependencyBuilder
    {
        internal MultipleDependencyBuilder(IModuleDependencyBuilder fallback) => _fallback = fallback;

        public IModuleInputDependency Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations)
        {
            if (!@interface.IsConstructedGenericType)
                return _fallback.Build(client, @interface, implementations);
            var t = @interface.GetGenericTypeDefinition();
            if (t != typeof(IEnumerable<>))
                return _fallback.Build(client, @interface, implementations);
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
                    });
        }

        private IEnumerable SelectMany(IEnumerable<IEnumerable> source)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var e in source)
            {
                foreach (var o in e)
                {
                    yield return o;
                }
            }
        }

        private object Cast(IEnumerable e, Type type) =>
            typeof(Enumerable).
                GetTypeInfo().
                GetDeclaredMethods(nameof(Enumerable.Cast)).
                First(m => m.IsGenericMethod).
                MakeGenericMethod(type).
                Invoke(null, new[] { (object)e });

        private readonly IModuleDependencyBuilder _fallback;
    }
}
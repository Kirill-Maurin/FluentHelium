using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHelium.Module
{
    internal sealed class MultipleDependencyBuilder : IModuleDependencyBuilder
    {
        internal MultipleDependencyBuilder(IModuleDependencyBuilder fallback)
        {
            _fallback = fallback;
        }

        public IModuleInputDependency Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations)
        {
            if (@interface.IsConstructedGenericType)
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
                                Select(e => (object) e);
                        var multiplets =
                            implementations[@interface].Select(d => p(d).Resolve(@interface));
                        return new[] {singlets}.
                            Concat(multiplets).
                            ToAggregatedUsable().
                            Select(e => (object)e);
                    });
        }


        private readonly IModuleDependencyBuilder _fallback;
    }
}
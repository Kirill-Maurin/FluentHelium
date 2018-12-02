using System;
using System.Linq;
using FluentHelium.Base;

namespace FluentHelium.Module {
    sealed class FallbackDependencyBuilder : IModuleDependencyBuilder
    {
        internal FallbackDependencyBuilder(
            IModuleDependencyBuilder builder,
            params IModuleDependencyBuilder[] fallback) 
            => (_builder, _fallback) = (builder, fallback);

        public RefOption<IModuleInputDependency> Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations)
        {
            var result = _builder.Build(client, @interface, implementations);
            foreach (var fallback in _fallback)
            {
                if (result.TryGet(out _))
                    return result;
                result = fallback.Build(client, @interface, implementations);
            }

            return result;
        }

        readonly IModuleDependencyBuilder _builder;
        readonly IModuleDependencyBuilder[] _fallback;
    }
}
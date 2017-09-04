using System;
using System.Linq;

namespace FluentHelium.Module
{
    /// <inheritdoc />
    /// <summary>
    /// Simple builder: use the single implementation from available else use fallback
    /// </summary>
    internal sealed class SimpleDependencyBuilder : IModuleDependencyBuilder
    {
        internal SimpleDependencyBuilder(IModuleDependencyBuilder fallback)
        {
            _fallback = fallback;
        }

        public IModuleInputDependency Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations) =>
            implementations[@interface].SingleOrDefault()
                ?.ToModuleInputDependency(client, @interface, (s, provider) => provider(s).Resolve(@interface)) 
                ?? _fallback.Build(client, @interface, implementations);

        private readonly IModuleDependencyBuilder _fallback;
    }
}
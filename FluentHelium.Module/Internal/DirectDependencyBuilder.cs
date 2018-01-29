using System;
using System.Linq;

namespace FluentHelium.Module
{
    /// <inheritdoc />
    /// <summary>
    /// Hard builder: use concrete implementation for (client, service) pair
    /// </summary>
    internal sealed class DirectDependencyBuilder : IModuleDependencyBuilder
    {
        internal DirectDependencyBuilder(
            IModuleDescriptor client,
            Type @interface,
            IModuleDescriptor implementation,
            IModuleDependencyBuilder fallback)
        {
            _client = client;
            _interface = @interface;
            _implementation = implementation;
            _fallback = fallback;
        }

        public IModuleInputDependency Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations) =>
            client == (_client ?? client) && @interface == (_interface ?? @interface)
                ? _implementation.ToModuleInputDependency(
                    client,
                    @interface,
                    provider => provider(_implementation).Resolve(@interface))
                : _fallback.Build(client, @interface, implementations);

        private readonly IModuleDescriptor _client;
        private readonly Type _interface;
        private readonly IModuleDescriptor _implementation;
        private readonly IModuleDependencyBuilder _fallback;
    }
}
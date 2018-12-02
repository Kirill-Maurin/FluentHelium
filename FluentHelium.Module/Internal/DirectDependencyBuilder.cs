using System;
using System.Linq;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    /// <inheritdoc />
    /// <summary>
    /// Hard builder: use concrete implementation for (client, service) pair
    /// </summary>
    sealed class DirectDependencyBuilder : IModuleDependencyBuilder
    {
        internal DirectDependencyBuilder(
            IModuleDescriptor client,
            Type @interface,
            IModuleDescriptor implementation)
        {
            _client = client;
            _interface = @interface;
            _implementation = implementation;
        }

        public RefOption<IModuleInputDependency> Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations) =>
            client == (_client ?? client) && @interface == (_interface ?? @interface)
                ? _implementation.ToModuleInputDependency(
                    client,
                    @interface,
                    provider => provider(_implementation).Resolve(@interface)).ToRefSome()
                : default;

        readonly IModuleDescriptor _client;
        readonly Type _interface;
        readonly IModuleDescriptor _implementation;
    }
}
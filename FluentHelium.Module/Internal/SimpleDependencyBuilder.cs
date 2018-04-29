using System;
using System.Linq;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    /// <inheritdoc />
    /// <summary>
    /// Simple builder: use the single implementation from available else use fallback
    /// </summary>
    internal sealed class SimpleDependencyBuilder : IModuleDependencyBuilder
    {
        public RefOption<IModuleInputDependency> Build(
            IModuleDescriptor client,
            Type @interface,
            ILookup<Type, IModuleDescriptor> implementations) =>
            (implementations[@interface].SingleOrDefault()
                ?.ToModuleInputDependency(client, @interface, (s, provider) => provider(s).Resolve(@interface))).ToRefOption();
    }
}
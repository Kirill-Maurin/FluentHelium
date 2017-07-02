using System;

namespace FluentHelium.Module
{
    /// <summary>
    /// Signle output dependency from module
    /// </summary>
    public sealed class ModuleOutputDependency
    {
        internal ModuleOutputDependency(IModuleDescriptor implementation, Type output)
        {
            Implementation = implementation;
            Output = output;
        }
        public IModuleDescriptor Implementation { get; }
        public Type Output { get; }
    }
}
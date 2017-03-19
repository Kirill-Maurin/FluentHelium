using System;

namespace FluentHelium.Module
{
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
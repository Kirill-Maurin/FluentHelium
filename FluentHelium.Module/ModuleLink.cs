using System;

namespace FluentHelium.Module
{
    /// <summary>
    /// Single dependency link between two modules
    /// </summary>
    public sealed class ModuleLink
    {
        internal ModuleLink(Type input, Type output, IModuleDescriptor client, IModuleDescriptor implementation)
        {
            Input = input;
            Output = output;
            Client = client;
            Implementation = implementation;
        }
        /// <summary>
        /// Input dependency
        /// </summary>
        public Type Input { get; }

        /// <summary>
        /// Output dependency: can be different from input in general case
        /// </summary>
        public Type Output { get; }

        public IModuleDescriptor Client { get; }
        public IModuleDescriptor Implementation { get; }
    }
}
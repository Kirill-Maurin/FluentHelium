using System;

namespace FluentHelium.Module
{
    public sealed class ModuleLink
    {
        internal ModuleLink(Type input, Type output, IModuleDescriptor client, IModuleDescriptor implementation)
        {
            Input = input;
            Output = output;
            Client = client;
            Implementation = implementation;
        }
        public Type Input { get; }
        public Type Output { get; }
        public IModuleDescriptor Client { get; }
        public IModuleDescriptor Implementation { get; }
    }
}
using System;
using System.Collections.Immutable;

namespace FluentHelium.Module
{
    internal sealed class ModuleDescriptor : IModuleDescriptor
    {
        public ModuleDescriptor(string name, Guid id, IImmutableSet<Type> input, IImmutableSet<Type> output)
        {
            Name = name;
            Id = id;
            Input = input;
            Output = output;
        }

        public string Name { get; }
        public Guid Id { get; }
        public IImmutableSet<Type> Input { get; }
        public IImmutableSet<Type> Output { get; }
    }
}
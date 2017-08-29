using System;
using System.Collections.Immutable;
using System.Linq;

namespace FluentHelium.Module
{
    internal sealed class ModuleGraph : IModuleGraph {
        public ModuleGraph(
            IImmutableDictionary<IModuleDescriptor, IModuleDependencies> dependencies, 
            ILookup<Type, IModuleDescriptor> input, 
            ILookup<Type, IModuleDescriptor> output, 
            IImmutableList<IModuleDescriptor> order, 
            IImmutableList<IModuleDescriptor> cycle)
        {
            Dependencies = dependencies;
            Input = input;
            Output = output;
            Order = order;
            Cycle = cycle;
        }

        public IImmutableDictionary<IModuleDescriptor, IModuleDependencies> Dependencies { get; }
        public ILookup<Type, IModuleDescriptor> Input { get; }
        public ILookup<Type, IModuleDescriptor> Output { get; }
        public IImmutableList<IModuleDescriptor> Order { get; }
        public IImmutableList<IModuleDescriptor> Cycle { get; }

        public override string ToString() =>
            (Order?.Count ?? 0) > 0 
                ? $"ModuleGraph Order{{{string.Join("; ", Order.Select(d => d.Name))}}}"
                : $"ModuleGraph Cycle{{{string.Join("=>", Cycle.Select(d => d.Name))}}}";
    }
}
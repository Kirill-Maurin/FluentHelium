using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentHelium.Module
{
    internal sealed class ModuleGraph : IModuleGraph {
        public ModuleGraph(
            IImmutableDictionary<IModuleDescriptor, ILookup<IModuleDescriptor, Type>> innerDependencies, 
            ILookup<Type, IModuleDescriptor> input, 
            ILookup<Type, IModuleDescriptor> output, 
            IImmutableList<IModuleDescriptor> order, 
            ILookup<KeyValuePair<IModuleDescriptor, Type>, IModuleDescriptor> excessive, 
            IImmutableList<KeyValuePair<IModuleDescriptor, IEnumerable<Type>>> cycle)
        {
            InnerDependencies = innerDependencies;
            Input = input;
            Output = output;
            Order = order;
            Excessive = excessive;
            Cycle = cycle;
        }

        public IImmutableDictionary<IModuleDescriptor, ILookup<IModuleDescriptor, Type>> InnerDependencies { get; }
        public ILookup<Type, IModuleDescriptor> Input { get; }
        public ILookup<Type, IModuleDescriptor> Output { get; }
        public IImmutableList<IModuleDescriptor> Order { get; }
        public ILookup<KeyValuePair<IModuleDescriptor, Type>, IModuleDescriptor> Excessive { get; }
        public IImmutableList<KeyValuePair<IModuleDescriptor, IEnumerable<Type>>> Cycle { get; }
    }
}
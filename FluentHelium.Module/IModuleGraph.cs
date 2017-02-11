using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentHelium.Module
{
    /// <summary>
    /// Module graph 
    /// </summary>
    public interface IModuleGraph
    {
        /// <summary>
        /// Dependencies inside module graph client -> (implementation, types)
        /// </summary>
        IImmutableDictionary<IModuleDescriptor, ILookup<IModuleDescriptor, Type>> InnerDependencies { get; }
        /// <summary>
        /// Input dependencies (type, clients) - they needed be provided by kernel for successful intialization
        /// </summary>
        ILookup<Type, IModuleDescriptor> Input { get; }
        /// <summary>
        /// Output dependencies (type, implementations) - they can be used by kernel after successful initialization of all modules
        /// </summary>
        ILookup<Type, IModuleDescriptor> Output { get; }
        /// <summary>
        /// Initialization order - result of topology sorting of module graph
        /// </summary>
        IImmutableList<IModuleDescriptor> Order { get; } 
        /// <summary>
        /// Excessive links (client, type) -> implementations - failed to resolve input dependencies by modules on single way
        /// </summary>
        ILookup<KeyValuePair<IModuleDescriptor, Type>, IModuleDescriptor> Excessive { get; }
        /// <summary>
        /// Dependency cycle - if present initialisation order cannot be found
        /// </summary>
        IImmutableList<KeyValuePair<IModuleDescriptor, IEnumerable<Type>>> Cycle { get; } 
    }
}
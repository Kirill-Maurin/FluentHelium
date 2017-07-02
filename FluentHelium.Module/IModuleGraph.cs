using System;
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
        /// Dependencies: client -> (type, dependency)
        /// </summary>
        IImmutableDictionary<IModuleDescriptor, IModuleDependencies> Dependencies { get; }
        /// <summary>
        /// Input dependencies (type, clients) - they needed be provided by kernel for successful intialization
        /// </summary>
        ILookup<Type, IModuleDescriptor> Input { get; }
        /// <summary>
        /// Output dependencies (type, implementations) - they can be used by kernel after successful initialization of all modules
        /// </summary>
        ILookup<Type, IModuleDescriptor> Output { get; }
        /// <summary>
        /// Initialization order - result of topology sorting
        /// </summary>
        IImmutableList<IModuleDescriptor> Order { get; }
        /// <summary>
        /// Dependency cycle - if present initialization order cannot be found
        /// </summary>
        IImmutableList<IModuleDescriptor> Cycle { get; } 
    }
}
using System;
using System.Collections.Immutable;

namespace FluentHelium.Module
{
    /// <summary>
    /// "Static" information about module: it is defined during module creation and not changed after
    /// </summary>
    public interface IModuleDescriptor
    {
        /// <summary>
        /// Human-readable module name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Machine-readable module id (must be unique in module graph)
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// Types, needed for successful module initializaion
        /// </summary>
        IImmutableSet<Type> Input { get; }
        /// <summary>
        /// Types, implemented by module 
        /// </summary>
        IImmutableSet<Type> Output { get; }
    }
}
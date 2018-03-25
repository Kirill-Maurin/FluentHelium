using System;
using System.Collections.Immutable;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    /// <summary>
    /// Access to implemented dependencies
    /// </summary>
    public interface IDependencyProvider
    {
        /// <summary>
        /// Resolve dependency - fail if Dependencies don't contain it
        /// </summary>
        /// <param name="type">Interface type (can be delegate, must contained in Dependencies)</param>
        /// <returns>Implementation (shared between consumers). Use Dispose if implementation is no longer needed</returns>
        Usable<object> Resolve(Type type);
        /// <summary>
        /// Implemented dependencies
        /// </summary>
        IImmutableSet<Type> Dependencies { get; } 
    }
}
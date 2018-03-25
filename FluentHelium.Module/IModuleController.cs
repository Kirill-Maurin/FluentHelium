using System;
using System.Collections.Generic;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    /// <summary>
    /// Lifetime control for modules in case "providing resources on demand"
    /// based on module dependencies
    /// </summary>
    public interface IModuleController
    {
        /// <summary>
        /// List of controlled modules (only descriptors)
        /// </summary>
        IEnumerable<IModuleDescriptor> Modules { get; }
        /// <summary>
        /// Get resource provider for module
        /// with lazy activation of module and its dependencies
        /// The module will be deactivated after its reference reaches zero
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        Usable<IDependencyProvider> GetProvider(IModuleDescriptor descriptor);
        /// <summary>
        /// Status of modules
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        bool IsActive(IModuleDescriptor descriptor);
        /// <summary>
        /// Fired after module status changed
        /// </summary>
        IObservable<KeyValuePair<IModuleDescriptor, bool>> ActiveChanged { get; }
    }
}
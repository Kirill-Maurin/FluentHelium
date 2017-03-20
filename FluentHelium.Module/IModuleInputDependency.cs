using System;
using System.Collections.Generic;

namespace FluentHelium.Module
{
    /// <summary>
    /// One input dependency of module
    /// </summary>
    public interface IModuleInputDependency
    {
        /// <summary>
        /// Depended module
        /// </summary>
        IModuleDescriptor Client { get; }
        /// <summary>
        /// Dependency type 
        /// </summary>
        Type Input { get; }
        /// <summary>
        /// Implementations (pairs module-type)
        /// </summary>
        IEnumerable<ModuleOutputDependency> Output { get; }
        /// <summary>
        /// Dependency resolver
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        Usable<object> Resolve(Func<IModuleDescriptor, IDependencyProvider> provider);
    }
}
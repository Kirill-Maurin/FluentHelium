using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHelium.Module
{
    /// <summary>
    /// All unit dependencies for one module
    /// </summary>
    public interface IModuleDependencies : IEnumerable<IModuleInputDependency>
    {
        /// <summary>
        /// Depended module
        /// </summary>
        IModuleDescriptor Client { get; }
        /// <summary>
        /// Dependencies by type
        /// </summary>
        /// <param name="interface"></param>
        /// <returns></returns>
        IModuleInputDependency this[Type @interface] { get; }
        /// <summary>
        /// Count of dependencies: same as Client.Type
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Dependencies by module
        /// </summary>
        ILookup<IModuleDescriptor, ModuleLink> Links { get; }
    }
}
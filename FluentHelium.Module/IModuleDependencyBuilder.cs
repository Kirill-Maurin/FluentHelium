using System;
using System.Linq;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    /// <summary>
    /// Builder of module2module dependencies
    /// </summary>
    public interface IModuleDependencyBuilder
    {
        /// <summary>
        /// Build input module dependency from implementations lookup
        /// </summary>
        /// <param name="client">Depended module</param>
        /// <param name="interface">Input interface for resolve</param>
        /// <param name="implementations">Implementations by type</param>
        /// <returns></returns>
        RefOption<IModuleInputDependency> Build(IModuleDescriptor client, Type @interface, ILookup<Type, IModuleDescriptor> implementations);
    }
}
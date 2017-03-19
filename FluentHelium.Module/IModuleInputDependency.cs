using System;
using System.Collections.Generic;

namespace FluentHelium.Module
{
    public interface IModuleInputDependency
    {
        IModuleDescriptor Client { get; }
        Type Input { get; }
        IEnumerable<ModuleOutputDependency> Output { get; }
        Usable<object> Resolve(Func<IModuleDescriptor, IDependencyProvider> provider);
    }
}
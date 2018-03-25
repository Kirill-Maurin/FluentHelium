using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FluentHelium.Base;
using NSubstitute;
using static NSubstitute.Substitute;

namespace FluentHelium.Module.Tests
{
    public static class ModuleTestExtensions
    {
        public static IModule ToFakeModule(this IModuleDescriptor descriptor) => descriptor.ToModule(d =>
            descriptor.Output.ToFakeProvider().ToUsable());

        public static IDependencyProvider ToFakeProvider(this IImmutableSet<Type> types)
        {
            var provider = For<IDependencyProvider>();
            provider.Dependencies.Returns(types);
            return provider;
        }

        public static IDependencyProvider ToFakeProvider(this IEnumerable<Type> types) => 
            types.ToImmutableHashSet().ToFakeProvider();
    }
}
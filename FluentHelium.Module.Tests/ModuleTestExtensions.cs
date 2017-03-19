using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NSubstitute;
using static NSubstitute.Substitute;

namespace FluentHelium.Module.Tests
{
    public static class ModuleTestExtensions
    {
        public static IModuleDescriptor ToFakeModuleDescriptor(
            this IEnumerable<Type> input,
            string name,
            params Type[] output)
        {
            var result = For<IModuleDescriptor>();
            result.Input.Returns(input.ToImmutableHashSet());
            result.Output.Returns(output.ToImmutableHashSet());
            result.Name.Returns(name);
            return result;
        }

        public static IModuleDescriptor ToFakeModuleDescriptor(this Type input, string name, params Type[] output) =>
            new[] {input}.ToFakeModuleDescriptor(name, output);

        public static IModuleDescriptor ToFakeProducerModuleDescriptor(this IEnumerable<Type> output, string name) =>
            Enumerable.Empty<Type>().ToFakeModuleDescriptor(name, output.ToArray());

        public static IModuleDescriptor ToFakeProducerModuleDescriptor(this Type output, string name) =>
            new[] {output}.ToFakeProducerModuleDescriptor(name);

        public static IModule ToFakeModule(this IModuleDescriptor descriptor) => descriptor.ToModule(d =>
            descriptor.Output.ToFakeProvider().ToUsable());

        public static IDependencyProvider ToFakeProvider(this IImmutableSet<Type> types)
        {
            var provider = For<IDependencyProvider>();
            provider.Dependencies.Returns(types);
            return provider;
        }

        public static IDependencyProvider ToFakeProvider(this IEnumerable<Type> types)
            => types.ToImmutableHashSet().ToFakeProvider();

    }
}
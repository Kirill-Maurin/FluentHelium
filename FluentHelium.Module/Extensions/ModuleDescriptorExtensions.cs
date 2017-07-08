using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentHelium.Module
{
    public static class ModuleDescriptorExtensions
    {
        public static IModuleDescriptor CreateSimpleDescriptor<TInput, TOutput>(string name, Guid id) =>
            typeof(TInput).ToModuleDescriptor(name, id, typeof(TOutput));

        public static IModuleDescriptor CreateSimpleDescriptor<TInput, TOutput>(string name) => CreateSimpleDescriptor<TInput, TOutput>(name, Guid.NewGuid());

        public static IModuleDescriptor CreateSimpleDescriptor<T>(string name, Guid id) =>
            typeof(T).ToProducerModuleDescriptor(name, id);

        public static IModuleDescriptor CreateSimpleDescriptor<T>(string name) => CreateSimpleDescriptor<T>(name, Guid.NewGuid());

        public static IModuleDescriptor ToModuleDescriptor(
                    this IEnumerable<Type> input, string name, Guid id, params Type[] output) =>
                        new ModuleDescriptor(name, id, input.ToImmutableHashSet(), output.ToImmutableHashSet());

        public static IModuleDescriptor ToModuleDescriptor(
            this Type input, string name, Guid id, params Type[] output) =>
                new ModuleDescriptor(name, id, new[] { input }.ToImmutableHashSet(), output.ToImmutableHashSet());

        public static IModuleDescriptor ToProducerModuleDescriptor(
            this Type input, string name) =>
            Enumerable.Empty<Type>().ToModuleDescriptor(name, Guid.NewGuid(), new Type[0].ToImmutableHashSet());
        
        public static IModuleDescriptor ToConsumerModuleDescriptor(
            this Type output, string name) =>
                new [] {output}.ToModuleDescriptor(name, Guid.NewGuid(), ImmutableHashSet<Type>.Empty);

        public static IModuleDescriptor ToProducerModuleDescriptor(
            this Type output, string name, Guid id) =>
                Enumerable.Empty<Type>().ToModuleDescriptor(name, id, new[] { output }.ToImmutableHashSet());

        public static IModuleDescriptor ToModuleDescriptor(
            this Type input, string name, params Type[] output) =>
                new ModuleDescriptor(name, Guid.NewGuid(), new[] { input }.ToImmutableHashSet(), output.ToImmutableHashSet());

        public static IModuleDescriptor ToModuleDescriptor(
            this IEnumerable<Type> input, string name, Guid id, IEnumerable<Type> output) =>
                new ModuleDescriptor(name, id, input.ToImmutableHashSet(), output.ToImmutableHashSet());

        public static string ToString(this IModuleDescriptor descriptor) =>
            $"{descriptor.Name}({descriptor.Id}) {descriptor.Input.Count}/{descriptor.Output.Count}" +
            (descriptor.Input.Count == 0 ? string.Empty : $"\n Input {{{string.Join("; ", descriptor.Input.Select(t => t.Name))}}}") +
            (descriptor.Output.Count == 0? String.Empty : $"\n Output {{{string.Join("; ", descriptor.Output.Select(t => t.Name))}}}");
    }
}

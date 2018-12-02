using FluentHelium.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using static FluentHelium.Module.DependencyBuilder;

namespace FluentHelium.Module
{
    public static class ModuleGraph
    {
        public static void WritePlantUml(
            this IImmutableDictionary<IModuleDescriptor, IModuleDependencies> dependencies,
            TextWriter writer,
            Func<IEnumerable<Type>, IModuleDescriptor, IModuleDescriptor, IEnumerable<Type>> filter)
        {
            foreach (var line in dependencies.SelectMany(
                p => p.Value.SelectMany(i => i.Output.
                    Select(o => new { i.Client, i.Input, o.Output, o.Implementation }).
                    GroupBy(l => l.Implementation).
                    SelectMany(g => filter(g.Select(d => d.Output), p.Key, g.Key).
                    Select(t => ToPlantUml(p.Key, g.Key, t))))))
                writer.WriteLine(line);
        }

        static string ToPlantUml(IModuleDescriptor client, IModuleDescriptor implementation, Type type)
            => $"[{client.Name}] ..> [{implementation.Name}] : {type.Name}";

        public static string ToPlantUml(this IModuleGraph graph)
        {
            using (var writer = new StringWriter())
            {
                graph.Dependencies.WritePlantUml(writer, (t, l, r) => t.Take(1));
                return writer.ToString();
            }
        }

        public static IModule ToSimpleSuperModule(this IModuleGraph graph, string name, params IModule[] modules)
            => graph.ToSuperModule(
                (t, mds) => mds.SingleOrDefault(),
                name,
                Guid.NewGuid(),
                modules);

        public static IModule ToSimpleLazyModule(this IModuleGraph graph, string name, params IModule[] modules)
           => graph.ToLazyModule(
                (t, mds) => mds.SingleOrDefault(),
                name,
                Guid.NewGuid(),
                modules);

        public static IModule ToSuperModule(this IModuleGraph graph, string name, params IModule[] modules)
            => graph.ToSuperModule(
                (t, mds) => mds.SingleOrDefault(),
                name,
                Guid.NewGuid(),
                modules);

        public static IModule ToLazyModule(this IModuleGraph graph, string name, params IModule[] modules)
            => graph.ToLazyModule(
                (t, mds) => mds.SingleOrDefault(),
                name,
                Guid.NewGuid(),
                modules);

        /// <summary>
        /// Create super module - activate all modules from graph during super module activation
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="tryChoiceOutput"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="modules"></param>
        /// <returns></returns>
        public static IModule ToSuperModule(
            this IModuleGraph graph,
            Func<Type, IEnumerable<IModuleDescriptor>, IModuleDescriptor> tryChoiceOutput,
            string name,
            Guid id,
            params IModule[] modules) =>
            CreateModule(graph, tryChoiceOutput, name, id, CreateSuperModule, modules);

        /// <summary>
        /// Create "lazy" super module - activate modules from graph on demand and deactivate if unused
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="tryChoiceOutput"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="modules"></param>
        /// <returns></returns>
        public static IModule ToLazyModule(
            this IModuleGraph graph,
            Func<Type, IEnumerable<IModuleDescriptor>, IModuleDescriptor> tryChoiceOutput,
            string name,
            Guid id,
            params IModule[] modules) =>
            CreateModule(graph, tryChoiceOutput, name, id, CreateLazyModule, modules);

        static IModule CreateModule(
            IModuleGraph graph,
            Func<Type, IEnumerable<IModuleDescriptor>, IModuleDescriptor> tryChoiceOutput,
            string name,
            Guid id,
            Func<IModuleGraph, IReadOnlyDictionary<IModuleDescriptor, IModule>, IModuleDescriptor, IEnumerable<KeyValuePair<Type, IModuleDescriptor>>, IModule> factory,
            params IModule[] modules)
        {
            if (graph.Cycle.Count > 0)
                throw new ArgumentException("Cannot create module from dependency graph with cycle", nameof(graph));
            var descriptor2Module = modules.ToImmutableDictionary(m => m.Descriptor);
            if (graph.Order.Any(md => !descriptor2Module.ContainsKey(md)))
                throw new ArgumentException("Need module for each descriptor from graph", nameof(modules));
            var output = graph.SelectOutput(tryChoiceOutput).ToImmutableList();
            var descriptor = graph.Input.Select(g => g.Key).ToModuleDescriptor(name, id, output.Select(p => p.Key));
            return factory(graph, descriptor2Module, descriptor, output);
        }

        static IModule CreateSuperModule(
            IModuleGraph graph,
            IReadOnlyDictionary<IModuleDescriptor, IModule> descriptor2Module, IModuleDescriptor descriptor, IEnumerable<KeyValuePair<Type, IModuleDescriptor>> output)
        {
            var module2Types = output.ToLookup(g => g.Value, g => g.Key);
            return descriptor.ToModule(dependencies => graph.Activate(dependencies, descriptor2Module).Select(p => p.ToDependencyProvider(module2Types)));
        }

        static IModule CreateLazyModule(
            IModuleGraph graph,
            IReadOnlyDictionary<IModuleDescriptor, IModule> descriptor2Module, IModuleDescriptor descriptor, IEnumerable<KeyValuePair<Type, IModuleDescriptor>> output)
        {
            var type2Module = output.ToImmutableDictionary(o => o.Key, o => o.Value);

            return descriptor.ToModule(
                dependencies =>
                {
                    var controller = graph.ToModuleController(descriptor2Module, dependencies);
                    return type2Module.Keys.
                        ToDependencyProvider(t => controller.GetProvider(type2Module[t]).SelectMany(p => p.Resolve(t))).
                        ToUsable();
                });
        }

        /// <summary>
        /// Select output types and implementations from module graph
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="tryChoiceOutput">Can return module or null for output type</param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<Type, IModuleDescriptor>> SelectOutput(
            this IModuleGraph graph, Func<Type, IEnumerable<IModuleDescriptor>, IModuleDescriptor> tryChoiceOutput) =>
            graph.Output.
                Select(g => g.Key.LinkValue(tryChoiceOutput(g.Key, g))).
                Where(p => p.Value != null);

        public static IDependencyProvider ToDependencyProvider(
            this IReadOnlyDictionary<IModuleDescriptor, IDependencyProvider> providers,
            ILookup<IModuleDescriptor, Type> selector) =>
                providers.Select(p => p.Value.Restrict(selector[p.Key])).Aggregate((l, r) => l.Union(r));

        public static Usable<IReadOnlyDictionary<IModuleDescriptor, IDependencyProvider>> Activate(
            this IModuleGraph graph, IDependencyProvider kernel, IReadOnlyDictionary<IModuleDescriptor, IModule> modules)
        {
            var missed = graph.Input.Select(g => g.Key).ToImmutableHashSet().Except(kernel.Dependencies);
            if (!missed.IsEmpty)
                throw new ArgumentException(
                    $"Kernel does not support types: {string.Join(";", missed.Select(t => t.Name))}");
            var activated = new Stack<IDisposable>();
            var providers = new Dictionary<IModuleDescriptor, IDependencyProvider>();
            foreach (var module in graph.Order)
            {
                var inputProvider = module.Input.ToDependencyProvider(t => graph.Dependencies[module][t].Resolve(
                    d => d.IsExternal() ? kernel : providers[d]));
                var result = modules[module].Activate(inputProvider);
                activated.Push(result);
                result.Do(p => providers[module] = p);
            }

            return ((IReadOnlyDictionary<IModuleDescriptor, IDependencyProvider>)providers).ToUsable(() =>
            {
                while (activated.Count > 0)
                    activated.Pop().Dispose();
            });
        }

        public static IModuleController ToModuleController(
            this IModuleGraph graph, IReadOnlyDictionary<IModuleDescriptor, IModule> modules, IDependencyProvider input) =>
            new ModuleController(graph, input, modules);

        public static IModuleGraph ToSimpleModuleGraph(this IEnumerable<IModuleDescriptor> modules) =>
            modules.ToModuleGraph(Simple.Or(External));

        public static IModuleGraph ToModuleGraph(
            this IEnumerable<IModuleDescriptor> modules,
            IModuleDependencyBuilder builder)
        {
            var moduleList = modules.ToImmutableList();
            var outputs = moduleList.
                SelectMany(m => m.Output.Select(m.LinkKey)).
                ToLookup(p => p.Key, p => p.Value);
            var inner = moduleList.ToImmutableDictionary(
                m => m,
                m => (IModuleDependencies)new ModuleDependencies(m.Input.ToImmutableDictionary(t => t, t => builder.Build(m, t, outputs).AsGeneric.Unwrap()), m));
            var inputs = inner.
                SelectMany(p => p.Value.
                    SelectMany(d => d.Output.
                        Where(o => o.Implementation.IsExternal()).
                        Select(o => o.Output)).
                    Distinct().
                    Select(t => t.LinkValue(p.Key))).
                ToLookup(p => p.Key, p => p.Value);
            return moduleList.TryTopologySort(
                m => ImmutableDictionaryExtensions.GetValueOrDefault(inner, m)?.Links.Select(l => l.Key).Where(i => !i.IsExternal()) ?? Enumerable.Empty<IModuleDescriptor>(),
                out var order)
                ? CreateSortedModuleGraph(order, inner, inputs, outputs)
                : CreateModuleGraphWithCycle(inner, inputs, outputs, order);
        }

        static IModuleGraph CreateSortedModuleGraph(
            IEnumerable<IModuleDescriptor> result,
            IImmutableDictionary<IModuleDescriptor, IModuleDependencies> inner,
            ILookup<Type, IModuleDescriptor> inputs,
            ILookup<Type, IModuleDescriptor> outputs) =>
            new Implementation(inner, inputs, outputs, result.ToImmutableList(), ImmutableList<IModuleDescriptor>.Empty);

        static IModuleGraph CreateModuleGraphWithCycle(
            IImmutableDictionary<IModuleDescriptor, IModuleDependencies> inner,
            ILookup<Type, IModuleDescriptor> inputs, ILookup<Type, IModuleDescriptor> outputs, IEnumerable<IModuleDescriptor> path) =>
            new Implementation(inner, inputs, outputs, ImmutableList<IModuleDescriptor>.Empty, path.ToImmutableList());

        sealed class Implementation : IModuleGraph
        {
            public Implementation(
                IImmutableDictionary<IModuleDescriptor, IModuleDependencies> dependencies,
                ILookup<Type, IModuleDescriptor> input,
                ILookup<Type, IModuleDescriptor> output,
                IImmutableList<IModuleDescriptor> order,
                IImmutableList<IModuleDescriptor> cycle)
            {
                Dependencies = dependencies;
                Input = input;
                Output = output;
                Order = order;
                Cycle = cycle;
            }

            public IImmutableDictionary<IModuleDescriptor, IModuleDependencies> Dependencies { get; }
            public ILookup<Type, IModuleDescriptor> Input { get; }
            public ILookup<Type, IModuleDescriptor> Output { get; }
            public IImmutableList<IModuleDescriptor> Order { get; }
            public IImmutableList<IModuleDescriptor> Cycle { get; }

            public override string ToString() =>
                (Order?.Count ?? 0) > 0
                    // ReSharper disable once AssignNullToNotNullAttribute
                    ? $"ModuleGraph Order{{{string.Join("; ", Order.Select(d => d.Name))}}}"
                    : $"ModuleGraph Cycle{{{string.Join("=>", Cycle.Select(d => d.Name))}}}";
        }

        sealed class ModuleDependencies : IModuleDependencies
        {
            internal ModuleDependencies(IImmutableDictionary<Type, IModuleInputDependency> dependencies, IModuleDescriptor client)
            {
                _dependencies = dependencies;
                Client = client;
                Links = _dependencies.Values.
                    SelectMany(d => d.Output.Select(o => new ModuleLink(d.Input, o.Output, d.Client, o.Implementation))).
                    ToLookup(l => l.Implementation);
            }

            public IModuleDescriptor Client { get; }
            public int Count => _dependencies.Count;
            public IModuleInputDependency this[Type @interface] => _dependencies[@interface];
            public ILookup<IModuleDescriptor, ModuleLink> Links { get; }

            public IEnumerator<IModuleInputDependency> GetEnumerator() => _dependencies.Values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            readonly IImmutableDictionary<Type, IModuleInputDependency> _dependencies;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using static FluentHelium.Module.ModuleDependencyExtensions;

namespace FluentHelium.Module
{
    public static class ModuleGraphExtensions
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
            {
                writer.WriteLine(line);
            }
        }

        private static string ToPlantUml(IModuleDescriptor client, IModuleDescriptor implementation, Type @type) =>
            $"[{client.Name}] ..> [{implementation.Name}] : {@type.Name}";

        public static string ToPlantUml(this IModuleGraph graph)
        {
            using (var writer = new StringWriter())
            {
                graph.Dependencies.WritePlantUml(writer, (t, l, r) => t.Take(1));
                return writer.ToString();
            }
        }

        public static IModule ToSuperModule(this IModuleGraph graph,
            Func<Type, IEnumerable<IModuleDescriptor>, IModuleDescriptor> tryChoiceOutput, string name, Guid id, IImmutableDictionary<IModuleDescriptor, IModule> modules)
        {
            if (graph.Cycle != null)
                throw new ArgumentException("Cannot create module from dependency graph with cycle");
            var output = graph.Output.
                Select(g => g.Key.LinkKey(tryChoiceOutput(g.Key, g))).
                Where(p => p.Key != null).
                ToImmutableList();
            var descriptor = graph.Input.Select(g => g.Key).ToModuleDescriptor(name, id, output.Select(p => p.Value));
            return descriptor.ToModule(dependencies =>
                graph.Activate(dependencies, modules).
                    Select(p => p.ToDependencyProvider(output.ToLookup(g => g.Key, g => g.Value))));
        }

        public static IDependencyProvider ToDependencyProvider(
            this IReadOnlyDictionary<IModuleDescriptor, IDependencyProvider> providers,
            ILookup<IModuleDescriptor, Type> selector) =>
                providers.Select(p => p.Value.Restrict(selector[p.Key])).Aggregate((l, r) => l.Union(r));

        public static Usable<IReadOnlyDictionary<IModuleDescriptor, IDependencyProvider>> Activate(
            this IModuleGraph graph, IDependencyProvider kernel, IImmutableDictionary<IModuleDescriptor, IModule> modules)
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
                providers[module] = result.Value;
            }

            return ((IReadOnlyDictionary<IModuleDescriptor, IDependencyProvider>)providers).ToUsable(() =>
            {
                while (activated.Count > 0)
                {
                    activated.Pop().Dispose();
                }
            });
        }

        public static IModuleController ToModuleController(
            this IModuleGraph graph, IImmutableDictionary<IModuleDescriptor, IModule> modules, IDependencyProvider input) =>
            new ModuleController(graph, input, modules);

        public static IModuleGraph ToSimpleModuleGraph(this IEnumerable<IModuleDescriptor> modules) =>
            modules.ToModuleGraph(DependencyBuilder().Simple().ElseExternal());

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
                m => (IModuleDependencies)new ModuleDependencies(m.Input.ToImmutableDictionary(t => t, t => builder.Build(m, t, outputs)), m));
            var inputs = inner.
                SelectMany(p => p.Value.
                    SelectMany(d => d.Output.
                        Where(o => o.Implementation.IsExternal()).
                        Select(o => o.Output)).
                    Distinct().
                    Select(t => t.LinkValue(p.Key))).
                ToLookup(p => p.Key, p => p.Value);
            return TryTopologySort(
                moduleList,
                m => inner.GetValueOrDefault(m)?.Links.Select(l => l.Key).Where(i => !i.IsExternal()) ?? Enumerable.Empty<IModuleDescriptor>(),
                out IEnumerable<IModuleDescriptor> order)
                ? CreateSortedModuleGraph(order, inner, inputs, outputs)
                : CreateModuleGraphWithCycle(inner, inputs, outputs, order);
        }

        public enum Color { White, Gray, Black }

        public static bool TryTopologySort<T>(
            IEnumerable<T> modules,
            Func<T, IEnumerable<T>> getLinks,
            out IEnumerable<T> order) where T : class
        {
            var moduleList = modules.ToImmutableList();
            var path = new Stack<T>();
            var result = new List<T>();
            var colors = Enumerable.Range(0, moduleList.Count).ToDictionary(i => moduleList[i], i => Color.White);

            foreach (var m in moduleList.Where(m => colors[m] == Color.White))
            {
                var current = m;
                colors[current] = Color.Gray;
                for (;;)
                {
                    var next = getLinks(current).FirstOrDefault(l => colors[l] != Color.Black);
                    if (next == null)
                    {
                        colors[current] = Color.Black;
                        result.Add(current);
                        if (path.Count == 0)
                            break;
                        current = path.Pop();
                        continue;
                    }
                    path.Push(current);
                    if (colors[next] == Color.Gray)
                    {
                        order = path.TakeWhile(c => !ReferenceEquals(c, next)).Concat(new []{next});
                        return false;
                    }
                    current = next;
                    colors[current] = Color.Gray;
                }
            }
            order = result;
            return true;
        }

        private static IModuleGraph CreateSortedModuleGraph(
            IEnumerable<IModuleDescriptor> result,
            IImmutableDictionary<IModuleDescriptor, IModuleDependencies> inner,
            ILookup<Type, IModuleDescriptor> inputs,
            ILookup<Type, IModuleDescriptor> outputs) =>
            new ModuleGraph(inner, inputs, outputs, result.ToImmutableList(), null);

        private static IModuleGraph CreateModuleGraphWithCycle(
            IImmutableDictionary<IModuleDescriptor, IModuleDependencies> inner,
            ILookup<Type, IModuleDescriptor> inputs, ILookup<Type, IModuleDescriptor> outputs, IEnumerable<IModuleDescriptor> path) =>
            new ModuleGraph(inner, inputs, outputs, null, path.ToImmutableList());
    }
}

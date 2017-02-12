using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace FluentHelium.Module
{
    public static class ModuleExtensions
    { 
        public enum Color { White, Gray, Black }
        
        public static void WritePlantUml(
            this IImmutableDictionary<IModuleDescriptor, ILookup<IModuleDescriptor, Type>> dependencies, 
            TextWriter writer,
            Func<IEnumerable<Type>, IModuleDescriptor, IModuleDescriptor, IEnumerable<Type>> filter)
        {
            foreach (var line in dependencies.SelectMany(p => p.Value.SelectMany(g => filter(g, p.Key, g.Key).Select(t => ToPlantUml(p.Key, g.Key, t)))))
            {
                writer.WriteLine(line);
            }
        }

        private static string ToPlantUml(IModuleDescriptor source, IModuleDescriptor dest, Type @type)
        {
            return $"[{source.Name}] -> [{dest.Name}] : {@type.Name}";
        }

        public static string ToPlantUml(this IModuleGraph graph)
        {
            using (var writer = new StringWriter())
            {
                graph.InnerDependencies.WritePlantUml(writer, (t, l, r) => t.Take(1));
                return writer.ToString();
            }
        }

        public static IModuleDescriptor ToModuleDescriptor(
            this IEnumerable<Type> input, string name, Guid id, params Type[] output) => 
                new ModuleDescriptor(name, id, input.ToImmutableHashSet(), output.ToImmutableHashSet());

        public static IModuleDescriptor ToModuleDescriptor(
            this Type input, string name, Guid id, params Type[] output) =>
                new ModuleDescriptor(name, id, new [] { input }.ToImmutableHashSet(), output.ToImmutableHashSet());

        public static IModuleDescriptor ToModuleDescriptor(
            this IEnumerable<Type> input, string name, Guid id, IEnumerable<Type> output) =>
                new ModuleDescriptor(name, id, input.ToImmutableHashSet(), output.ToImmutableHashSet());

        public static IModule ToModule(
            this IModuleDescriptor descriptor,
            Func<IDependencyProvider, Usable<IDependencyProvider>> activator) =>
                new Module(descriptor, activator);

        public static IModule ToModule(this IModuleGraph graph,
            Func<Type, IEnumerable<IModuleDescriptor>, IModuleDescriptor> tryChoiceOutput, string name, Guid id, IImmutableDictionary<IModuleDescriptor, IModule> modules)
        {
            if (graph.Cycle != null)
                throw new ArgumentException("Cannot create module from dependency graph with cycle");
            var output = graph.Output.
                Select(g => g.Key.LinkKey(tryChoiceOutput(g.Key, g))).
                Where(p => p.Key != null).
                ToImmutableArray();
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
                var innerDependencies
                    = graph.InnerDependencies[module];
                var inputProvider = innerDependencies.Aggregate(
                    kernel.Except(innerDependencies.SelectMany(m => m)),
                    (p, m) => p.Union(providers[m.Key].Restrict(m)));
                var result = modules[module].Activate(inputProvider);
                activated.Push(result);
                providers[module] = result.Value;
            }

            return ((IReadOnlyDictionary<IModuleDescriptor, IDependencyProvider>) providers).ToUsable(() =>
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

        public static IModuleGraph ToModuleGraph(
            this IEnumerable<IModuleDescriptor> modules, 
            Func<IModuleDescriptor, Type, IEnumerable<IModuleDescriptor>, IModuleDescriptor> tryChoiceImplementation)
        {
            var modulesArray = modules.ToImmutableArray();
            var outputs = modulesArray.SelectMany(m => m.Output.Select(m.LinkKey)).ToLookup(p => p.Key, p => p.Value);
            var innerByType = modulesArray.SelectMany(m => m.Input.Select(m.LinkValue)).Select(p => new
            {
                Client = p.Key,
                Type = p.Value,
                Implementations = outputs[p.Value],
            }).Select(l => new
            {
                l.Client, l.Type, l.Implementations,
                Choosed = l.Implementations.Any() ? tryChoiceImplementation(l.Client, l.Type, l.Implementations) : null
            }).ToImmutableArray();
            var links = innerByType.GroupBy(p => p.Choosed).ToImmutableDictionary(g => g.Key, g => g.GroupBy(l => l.Client, l => l.Type));
            var inputs = innerByType.Where(p => p.Choosed == null).ToLookup(p => p.Type, p => p.Client);
            var path = new Stack<IModuleDescriptor>();
            var result = new Stack<IModuleDescriptor>();
            var colors = Enumerable.Range(0, modulesArray.Length).ToDictionary(i => modulesArray[i], i => Color.White);

            var inner = innerByType.Where(t => t.Choosed != null).GroupBy(t => t.Client).
                ToImmutableDictionary(g => g.Key, g => g.ToLookup(p => p.Choosed, p => p.Type));
            var excessive = innerByType.Where(p => p.Choosed == null).SelectMany(p => p.Implementations.Select(i => new
            {
                p.Client,
                p.Type,
                Implementation = i
            })).ToLookup(p => p.Client.LinkValue(p.Type), p => p.Implementation);
            foreach (var m in modulesArray.Where(m => colors[m] == Color.White))
            {
                var current = m;
                colors[current] = Color.Gray;
                for(;;)
                {
                    var next = links.GetValueOrDefault(current)?.FirstOrDefault(g => colors[g.Key] != Color.Black);
                    if (next == null)
                    {
                        colors[current] = Color.Black;
                        result.Push(current);
                        if (path.Count == 0)
                            break;
                        current = path.Pop();
                        continue;
                    }
                    path.Push(current);
                    if (colors[next.Key] == Color.Gray)
                        return CreateModuleGraphWithCycle(
                            inner,
                            excessive,
                            inputs,
                            outputs,
                            path);
                    current = next.Key;
                    colors[current] = Color.Gray;
                }
            }
            return CreateSortedModuleGraph(result, inner, excessive, inputs, outputs);
        }

        private static IModuleGraph CreateSortedModuleGraph(
            Stack<IModuleDescriptor> result, 
            IImmutableDictionary<IModuleDescriptor, ILookup<IModuleDescriptor, Type>> inner, 
            ILookup<KeyValuePair<IModuleDescriptor, Type>, IModuleDescriptor> excessive, 
            ILookup<Type, IModuleDescriptor> inputs, 
            ILookup<Type, IModuleDescriptor> outputs)
        {
            return new ModuleGraph(inner, inputs, outputs, result.ToImmutableList(), excessive, null);
        }

        private static IModuleGraph CreateModuleGraphWithCycle(
            IImmutableDictionary<IModuleDescriptor, ILookup<IModuleDescriptor, Type>> inner, 
            ILookup<KeyValuePair<IModuleDescriptor, Type>, IModuleDescriptor> excessive, 
            ILookup<Type, IModuleDescriptor> inputs, ILookup<Type, IModuleDescriptor> outputs, Stack<IModuleDescriptor> path)
        {
            var modules = path.ToImmutableArray();
            var cycle = path.Select((m, i) => m.LinkValue(modules[(i + 1)%modules.Length])).
                Select(p => p.Key.LinkValue(inner[p.Key][p.Value]));

            return new ModuleGraph(inner, inputs, outputs, null, excessive, cycle.ToImmutableList());
        }
    }
}
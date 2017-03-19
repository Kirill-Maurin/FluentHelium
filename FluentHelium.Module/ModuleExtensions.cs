using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace FluentHelium.Module
{
    public static class ModuleExtensions
    { 
        public static readonly Guid External = new Guid("{6671930C-FC8F-4148-A596-097D94285279}");
        public static bool IsExternal(this IModuleDescriptor descriptor) => descriptor.Id == External;
        public static IModuleDescriptor ExternalModule { get; } = new ModuleDescriptor("External", External, null, null);
        public static ExternalDependencyBuilder ExternalDependencyBuilder { get; } = new ExternalDependencyBuilder();

        public static IModuleDependencyBuilder ElseExternal(
            this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> linkFallback) =>
            linkFallback(ExternalDependencyBuilder);

        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> DependencyBuilder() => b => b;
        public static Func<IModuleDependencyBuilder, IModuleDependencyBuilder> Simple(this Func<IModuleDependencyBuilder, IModuleDependencyBuilder> linkFallback) =>
            b => new SimpleDependencyBuilder(b);

        public enum Color { White, Gray, Black }

        public static ModuleOutputDependency ToModuleOutputDependency(
            this Type @interface, IModuleDescriptor descriptor) =>
            new ModuleOutputDependency(descriptor, @interface);

        public static IModuleInputDependency ToModuleInputDependency(
            this IEnumerable<ModuleOutputDependency> dependencies,
            IModuleDescriptor client,
            Type @interface, 
            Func<Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, dependencies, resolver, client);

        public static IModuleInputDependency ToModuleInputDependency(
            this Type @interface,
            IModuleDescriptor client,
            Func<Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, Enumerable.Empty<ModuleOutputDependency>(), provider => resolver(), client);

        public static IModuleInputDependency ToModuleInputDependency(
            this ModuleOutputDependency source,
            IModuleDescriptor client,
            Type @interface,
            Func<Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, new [] {source}, resolver, client);

        public static IModuleInputDependency ToModuleInputDependency(
            this IModuleDescriptor source,
            IModuleDescriptor client,
            Type @interface,
            Func<Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, new[] { @interface.ToModuleOutputDependency(source) }, resolver, client);

        public static IModuleInputDependency ToModuleInputDependency(
            this IModuleDescriptor source,
            IModuleDescriptor client,
            Type @interface,
            Func<IModuleDescriptor, Func<IModuleDescriptor, IDependencyProvider>, Usable<object>> resolver) =>
            new ModuleInputDependency(@interface, new[] { @interface.ToModuleOutputDependency(source) }, r => resolver(source, r), client);

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

        public static IModuleGraph ToModuleGraphSimple(this IEnumerable<IModuleDescriptor> modules) =>
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
            IEnumerable<IModuleDescriptor> order;
            return TryTopologySort(
                moduleList,
                m => inner.GetValueOrDefault(m)?.Links.Select(l => l.Key).Where(i => !i.IsExternal()) ?? Enumerable.Empty<IModuleDescriptor>(),
                out order)
                ? CreateSortedModuleGraph(order, inner, inputs, outputs)
                : CreateModuleGraphWithCycle(inner, inputs, outputs, order);
        }

        public static bool TryTopologySort<T>(
            IEnumerable<T> modules,
            Func<T, IEnumerable<T>> getLinks,
            out IEnumerable<T> order) where T: class
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
                        order = path;
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
            ILookup<Type, IModuleDescriptor> outputs)
        {
            return new ModuleGraph(inner, inputs, outputs, result.ToImmutableList(), null);
        }

        private static IModuleGraph CreateModuleGraphWithCycle(
            IImmutableDictionary<IModuleDescriptor, IModuleDependencies> inner, 
            ILookup<Type, IModuleDescriptor> inputs, ILookup<Type, IModuleDescriptor> outputs, IEnumerable<IModuleDescriptor> path)
        {
            return new ModuleGraph(inner, inputs, outputs, null, path.ToImmutableList());
        }
    }
}
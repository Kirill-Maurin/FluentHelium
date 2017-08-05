using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FluentHelium.Module
{
    internal sealed class ModuleController : IModuleController
    {
        public ModuleController(IModuleGraph graph, IDependencyProvider input, IReadOnlyDictionary<IModuleDescriptor, IModule> modules)
        {
            if (graph.Cycle != null)
                throw new ArgumentException("Module graph contains dependency cycle", nameof(graph));
            _graph = graph;
            _input = input;
            _modules = modules;
            _activeChanged = new Subject<KeyValuePair<IModuleDescriptor, bool>>();
            ActiveChanged = _activeChanged.AsObservable();
        }

        public IEnumerable<IModuleDescriptor> Modules => _modules.Keys;

        public Usable<IDependencyProvider> GetProvider(IModuleDescriptor descriptor) =>
            _providers.GetOrAdd(descriptor, CreateProvider)();

        private Func<Usable<IDependencyProvider>> CreateProvider(IModuleDescriptor descriptor)
        {
            var dependecies = _graph.Dependencies[descriptor];
            var provider = GetDependencyProviders(descriptor).
                Select(i =>
                {
                    var result = _modules[descriptor].Activate(
                        descriptor.Input.ToDependencyProvider(t => dependecies[t].Resolve(d => i[d])));
                    _activeChanged.OnNext(descriptor.LinkValue(true));
                    return result;
                }).
                ToUsable(() =>
                {
                    Func<Usable<IDependencyProvider>> r;
                    _providers.TryRemove(descriptor, out r);
                    _activeChanged.OnNext(descriptor.LinkValue(false));
                });
            return provider.ToRefCount();
        }

        private Usable<ImmutableDictionary<IModuleDescriptor, IDependencyProvider>> GetDependencyProviders(IModuleDescriptor descriptor) =>
            _graph.
                Dependencies[descriptor].
                SelectMany(d => d.Output.Select(o => o.Implementation)).
                Distinct().
                Select(d => (d.IsExternal() ? _input.ToUsable() : GetProvider(d)).Select(p => new { Descriptor = d, Provider = p})).
                ToAggregatedUsable().
                Select(l => l.ToImmutableDictionary(p => p.Descriptor, p => p.Provider));

        public bool IsActive(IModuleDescriptor descriptor) => _providers.ContainsKey(descriptor);

        public IObservable<KeyValuePair<IModuleDescriptor, bool>> ActiveChanged { get; }

        private readonly ConcurrentDictionary<IModuleDescriptor, Func<Usable<IDependencyProvider>>> _providers = 
            new ConcurrentDictionary<IModuleDescriptor, Func<Usable<IDependencyProvider>>>();

        private readonly IDependencyProvider _input;
        private readonly IReadOnlyDictionary<IModuleDescriptor, IModule> _modules;
        private readonly Subject<KeyValuePair<IModuleDescriptor, bool>> _activeChanged;
        private readonly IModuleGraph _graph;
    }
}
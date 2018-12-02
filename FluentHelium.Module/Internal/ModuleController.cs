using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentHelium.Base;

namespace FluentHelium.Module
{
    sealed class ModuleController : IModuleController
    {
        public ModuleController(IModuleGraph graph, IDependencyProvider input, IReadOnlyDictionary<IModuleDescriptor, IModule> modules)
        {
            if (graph.Cycle.Count > 0)
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

        Func<Usable<IDependencyProvider>> CreateProvider(IModuleDescriptor descriptor)
        {
            var dependecies = _graph.Dependencies[descriptor];
            var provider = GetDependencyProviders(descriptor).
                SelectMany(i =>
                {
                    var result = _modules[descriptor].Activate(
                        descriptor.Input.ToDependencyProvider(t => dependecies[t].Resolve(d => i[d])));
                    _activeChanged.OnNext(descriptor.LinkValue(true));
                    return result;
                }).
                Wrap(() =>
                {
                    _providers.TryRemove(descriptor, out var _);
                    _activeChanged.OnNext(descriptor.LinkValue(false));
                });
            return provider.ToRefCount();
        }

        Usable<ImmutableDictionary<IModuleDescriptor, IDependencyProvider>> GetDependencyProviders(IModuleDescriptor descriptor) =>
            _graph.
                Dependencies[descriptor].
                SelectMany(d => d.Output.Select(o => o.Implementation)).
                Distinct().
                Select(d => (d.IsExternal() ? _input.ToUsable() : GetProvider(d)).Select(p => new { Descriptor = d, Provider = p})).
                ToAggregatedUsable().
                Select(l => l.ToImmutableDictionary(p => p.Descriptor, p => p.Provider));

        public bool IsActive(IModuleDescriptor descriptor) => _providers.ContainsKey(descriptor);

        public IObservable<KeyValuePair<IModuleDescriptor, bool>> ActiveChanged { get; }

        readonly ConcurrentDictionary<IModuleDescriptor, Func<Usable<IDependencyProvider>>> _providers = 
            new ConcurrentDictionary<IModuleDescriptor, Func<Usable<IDependencyProvider>>>();

        readonly IDependencyProvider _input;
        readonly IReadOnlyDictionary<IModuleDescriptor, IModule> _modules;
        readonly Subject<KeyValuePair<IModuleDescriptor, bool>> _activeChanged;
        readonly IModuleGraph _graph;
    }
}
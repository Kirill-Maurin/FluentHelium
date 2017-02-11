using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FluentHelium.Module
{
    internal sealed class ModuleController : IModuleController
    {
        public ModuleController(IModuleGraph graph, IDependencyProvider input, IImmutableDictionary<IModuleDescriptor, IModule> modules)
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

        public Usable<IDependencyProvider> GetProvider(IModuleDescriptor descriptor)
        {
            var record = _providers.GetOrAdd(descriptor, CreateProvider);
            var disposable = record.Value.GetDisposable();
            record.Value.Dispose();
            return record.Key.ToUsable(disposable);
        }

        private KeyValuePair<IDependencyProvider, RefCountDisposable> CreateProvider(IModuleDescriptor descriptor)
        {
            var input = _graph.InnerDependencies.GetValue(descriptor).
                Select(g => GetProvider(g.Key).Select(p => p.Restrict(g))).
                ToImmutableArray();
            var provider = input.
                Aggregate(_input.Except(input.SelectMany(p => p.Value.Dependencies)), (l, r) => l.Union(r)).
                Select(i => _modules[descriptor].Activate(i));
            _activeChanged.OnNext(descriptor.LinkValue(true));

            var disposable = new RefCountDisposable(Disposable.Create(() =>
            {
                provider.Dispose();
                KeyValuePair<IDependencyProvider, RefCountDisposable> r;
                _providers.TryRemove(descriptor, out r);
                _activeChanged.OnNext(descriptor.LinkValue(false));
            }));
            return provider.Unwrap(p => p.LinkValue(disposable));
        }

        public bool IsActive(IModuleDescriptor descriptor) => _providers.ContainsKey(descriptor);

        public IObservable<KeyValuePair<IModuleDescriptor, bool>> ActiveChanged { get; }

        private readonly ConcurrentDictionary<IModuleDescriptor, KeyValuePair<IDependencyProvider, RefCountDisposable>> _providers = 
            new ConcurrentDictionary<IModuleDescriptor, KeyValuePair<IDependencyProvider, RefCountDisposable>>();

        private readonly IDependencyProvider _input;
        private readonly IImmutableDictionary<IModuleDescriptor, IModule> _modules;
        private readonly Subject<KeyValuePair<IModuleDescriptor, bool>> _activeChanged;
        private readonly IModuleGraph _graph;
    }
}
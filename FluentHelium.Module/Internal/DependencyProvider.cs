using System;
using System.Collections.Immutable;

namespace FluentHelium.Module
{
    internal class DependencyProvider : IDependencyProvider
    {
        public DependencyProvider(Func<Type, Usable<object>> resolver, IImmutableSet<Type> dependencies)
        {
            _resolver = resolver;
            Dependencies = dependencies;
        }

        public Usable<object> Resolve(Type type) => _resolver(type);

        public IImmutableSet<Type> Dependencies { get; }

        private readonly Func<Type, Usable<object>> _resolver;
    }
}
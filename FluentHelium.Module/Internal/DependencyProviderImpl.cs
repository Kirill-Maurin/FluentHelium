using System;
using System.Collections.Immutable;

namespace FluentHelium.Module
{
    internal sealed class DependencyProviderImpl : IDependencyProvider
    {
        public DependencyProviderImpl(Func<Type, Usable<object>> resolver, IImmutableSet<Type> dependencies)
        {
            _resolver = resolver;
            Dependencies = dependencies;
        }

        public Usable<object> Resolve(Type type) => _resolver(type);

        public IImmutableSet<Type> Dependencies { get; }

        public override string ToString() => DependencyProvider.ToString(this);

        private readonly Func<Type, Usable<object>> _resolver;
    }
}
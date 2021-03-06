using FluentHelium.Base;
using NullGuard;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FluentHelium.Module
{
    public static class DependencyProvider
    {
        public static Usable<T> Resolve<T>(this IDependencyProvider provider) =>
            provider.Resolve(typeof(T)).Select(o => (T)o);

        public static readonly IDependencyProvider Empty =
            new Implementation(t => throw new NotImplementedException(t.Name), ImmutableHashSet<Type>.Empty);

        public static IDependencyProvider Restrict(this IDependencyProvider provider, IEnumerable<Type> restrictions)
        {
            var r = restrictions.ToImmutableHashSet();
            if (r.Count > 0 && !provider.Dependencies.IsSupersetOf(r))
                throw new ArgumentException($"Not all restrictions are supported by base provider");
            return new Implementation(provider.Resolve, r);
        }

        public static IDependencyProvider Except(this IDependencyProvider provider, IEnumerable<Type> exceptions)
        {
            var e = exceptions.ToImmutableHashSet();
            return new Implementation(provider.Resolve, provider.Dependencies.Except(e));
        }

        public static IDependencyProvider Union(this IDependencyProvider left, IDependencyProvider right)
        {
            var m = left.Dependencies.Intersect(right.Dependencies);
            if (m.Count > 0)
                throw new ArgumentException(
                    $"left provider contains same dependecies as right:{string.Join(";", m.Select(t => t.Name))}");
            return new Implementation(
                t => left.Dependencies.Contains(t) ? left.Resolve(t) : right.Resolve(t),
                left.Dependencies.Union(right.Dependencies));
        }

        public static IDependencyProvider ToDependencyProvider(this IImmutableSet<Type> types, Func<Type, Usable<object>> resolver) =>
            new Implementation(resolver, types);

        public static IDependencyProvider ToDependencyProvider(this IEnumerable<Type> types, Func<Type, Usable<object>> resolver) =>
            types.ToImmutableHashSet().ToDependencyProvider(resolver);

        public static IDependencyProvider ToDependencyProvider(this Type type, Func<Type, Usable<object>> resolver) =>
            new[] { type }.ToDependencyProvider(resolver);

        public static IDependencyProvider ToDependencyProvider<T>([AllowNull] this T value) =>
            typeof(T).ToDependencyProvider(t => value.ToUsable().Select(v => (object)v));

        public static string ToString(this IDependencyProvider provider) =>
            $"DependencyProvider({provider.Dependencies.Count}){{{string.Join("; ", provider.Dependencies.Select(t => t.Name))}}}";

        sealed class Implementation : IDependencyProvider
        {
            public Implementation(Func<Type, Usable<object>> resolver, IImmutableSet<Type> dependencies)
            {
                _resolver = resolver;
                Dependencies = dependencies;
            }

            public Usable<object> Resolve(Type type)
                => Dependencies.Contains(type)
                ? _resolver(type)
                : throw new ArgumentException($"Dependency provider don't support type {type.Name}", nameof(type));

            public IImmutableSet<Type> Dependencies { get; }

            public override string ToString() => DependencyProvider.ToString(this);

            readonly Func<Type, Usable<object>> _resolver;
        }
    }
}
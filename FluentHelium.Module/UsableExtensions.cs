using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;

namespace FluentHelium.Module
{
    public static class UsableExtensions
    {
        public static Action DoNothing { get; } = () => {};

        public static Usable<T> ToUsable<T>(this T resource, IDisposable usageTime) where T : class => new Usable<T>(resource, usageTime);

        public static Usable<T> ToUsable<T>(this Usable<T> resource, IDisposable usageTime) where T : class => new Usable<T>(resource.Value, () =>
        {
            resource.Dispose();
            usageTime.Dispose();
        });

        public static Usable<T> ToSelfUsable<T>(this T resource) where T : class, IDisposable => resource.ToUsable(resource);

        public static Usable<T> ToUsable<T>(this T resource) where T : class => resource.ToUsable(DoNothing);

        public static Usable<T> ToUsable<T>(this T resource, Func<T, IDisposable> usageTimeFactory) where T : class => resource.ToUsable(usageTimeFactory(resource));

        public static Usable<T> ToUsable<T>(this T resource, Action dispose) where T : class => new Usable<T>(resource, dispose);

        public static Usable<T> ToUsable<T>(this T resource, Action<T> dispose) where T : class => resource.ToUsable(() => dispose(resource));

        public static Usable<T> ToUsable<T>(this T resource, Action<T> init, Action<T> dispose) where T : class
        {
            init(resource);
            return resource.ToUsable(() => dispose(resource));
        }

        public static Usable<T> ToUsable<T>(this T resource, Func<T, Action> disposeFactory) where T : class => resource.ToUsable(() => disposeFactory(resource));

        public static Usable<T> Wrap<T>(this IDisposable usageTime, T resource) where T : class => resource.ToUsable(usageTime);

        public static Usable<T> Wrap<T, TDisposable>(this TDisposable usageTime, Func<TDisposable, T> factory) where TDisposable : IDisposable where T : class => 
            factory(usageTime).ToUsable(usageTime);

        public static Usable<T> Select<TSource, T>(this Usable<TSource> source, Func<TSource, T> selector)
            where T : class where TSource : class =>
                selector(source.Value).ToUsable(source.Dispose);

        public static Usable<T> Select<TSource, T>(this Usable<TSource> source, Func<TSource, Usable<T>> selector)
            where T : class where TSource : class =>
                selector(source.Value).ToUsable(source);

        public static void Using<T>(this Usable<T> usable, Action<T> action) where T : class
        {
            using (usable)
            {
                action(usable.Value);
            }
        }

        public static TResult Using<T, TResult>(this Usable<T> usable, Func<T, TResult> func) where T : class
        {
            using (usable)
            {
                return func(usable.Value);
            }
        }

        public static void Using<T>(this Func<Usable<T>> factory, Action<T> action) where T : class => factory().Using(action);

        public static TResult Using<T, TResult>(this Func<Usable<T>> factory, Func<T, TResult> func) where T : class => factory().Using(func);

        public static Usable<T> Do<T>(this Usable<T> usable, Action<T> action) where T : class
        {
            action(usable.Value);
            return usable;
        }

        public static TResult Unwrap<T, TResult>(this Usable<T> usable, Func<T, TResult> func) where T : class => func(usable.Value);

        public static Usable<T> Replace<T>(this Usable<T> usable, Usable<T> @new) where T : class
        {
            usable?.Dispose();
            return @new;
        } 

        public static Usable<T> Aggregate<TSource, T>(this IEnumerable<Usable<TSource>> source, T seed, Func<T, TSource, T> add) where T : class where TSource : class
        {
            var disposables = source.ToImmutableArray();
            return disposables.Select(u => u.Value).Aggregate(seed, add).ToUsable(new CompositeDisposable(disposables));
        }
    }
}
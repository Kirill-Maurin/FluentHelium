using System;

namespace FluentHelium.Module
{
    /// <summary>
    /// Reactive property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProperty<out T> : IObservable<T>
    {
        T Value { get; }
    }
}
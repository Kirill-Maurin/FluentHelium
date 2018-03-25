using System;

namespace FluentHelium.Base
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
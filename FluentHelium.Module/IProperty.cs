using System;

namespace FluentHelium.Module
{
    public interface IProperty<out T> : IObservable<T>
    {
        T Value { get; }
    }
}
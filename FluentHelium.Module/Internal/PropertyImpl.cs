using System;
using System.Reactive.Linq;

namespace FluentHelium.Module {
    internal sealed class PropertyImpl<T> : IProperty<T>
    {
        public PropertyImpl(IObservable<T> subject, T value = default(T))
        {
            Value = value;
            _subject = subject;
            subject.Subscribe(v => Value = v);
        }

        public T Value { get; private set; }
        public IDisposable Subscribe(IObserver<T> observer) => _subject.StartWith(Value).Subscribe(observer);

        private readonly IObservable<T> _subject;
    }
}
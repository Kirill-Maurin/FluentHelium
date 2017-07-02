using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FluentHelium.Module
{
    internal sealed class MutablePropertyImpl<T> : IMutableProperty<T>
    {
        public MutablePropertyImpl(ISubject<T> subject, T value = default(T))
        {
            _value = value;
            _subject = subject;
        }

        public T Value { get => _value; set { _value = value; _subject.OnNext(_value); } }
        private T _value;

        public IDisposable Subscribe(IObserver<T> observer) => _subject.StartWith(Value).Subscribe(observer);

        public void OnCompleted() => _subject.OnCompleted();

        public void OnError(Exception error) => _subject.OnError(error);

        public void OnNext(T value) => Value = value;

        private readonly ISubject<T> _subject;
    }
}
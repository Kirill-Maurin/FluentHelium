using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FluentHelium.Module
{
    internal sealed class PropertyImpl<T> : IMutableProperty<T>
    {
        public PropertyImpl(T value = default(T))
        {
            _value = value;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (ReferenceEquals(_value, value) || (_value?.Equals(value) ?? false))
                {
                    return;
                }
                _value = value;
                _subject?.OnNext(_value);
            }
        }

        private T _value;

        public IDisposable Subscribe(IObserver<T> observer) => 
            (_subject ?? (_subject = new Subject<T>())).StartWith(Value).Subscribe(observer);

        public void OnCompleted() => _subject?.OnCompleted();

        public void OnError(Exception error) => _subject?.OnError(error);

        public void OnNext(T value) => Value = value;

        private ISubject<T> _subject;
    }
}
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FluentHelium.Module
{
    public static class Property
    {
        public static IMutableProperty<T> ToProperty<T>(this T @default) => new Property<T>(@default);
        public static IProperty<T> Create<T>() => default(T).ToProperty();
    }

    internal sealed class Property<T> : IMutableProperty<T>
    {
        public Property(T value = default)
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

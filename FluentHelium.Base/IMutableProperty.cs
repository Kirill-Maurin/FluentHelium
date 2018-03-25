using System.Reactive.Subjects;

namespace FluentHelium.Base
{
    /// <summary>
    /// Mutable reactive property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMutableProperty<T> : IProperty<T>, ISubject<T>
    {
        new T Value { get; set; }
    }
}

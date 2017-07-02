using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Text;

namespace FluentHelium.Module
{
    public interface IMutableProperty<T> : IProperty<T>, ISubject<T>
    {
        new T Value { get; set; }
    }
}

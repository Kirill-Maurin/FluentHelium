using System;


namespace FluentHelium.Module
{
    public sealed class Usable<T> : IDisposable where T: class
    {
        internal Usable(T resource, IDisposable usageTime) : this(resource, usageTime.Dispose) {}

        internal Usable(T resource, Action dispose)
        {
            _dispose = dispose;
            _value = resource;
        }

        public void Dispose()
        {
            if (Value == null)
                throw new ObjectDisposedException("");
            _dispose();
            _dispose = null;
            _value = null;
        }

        internal T Value
        {
            get
            {
                if (_value == null)
                    throw new ObjectDisposedException("");
                return _value;
            }
        }

        private Action _dispose;
        private T _value;
    }
}

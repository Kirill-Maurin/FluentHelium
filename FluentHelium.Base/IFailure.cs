using System;

namespace FluentHelium.Base
{
    public interface IFailure<T, out TResult> : IFailure<T> where TResult : struct, IFailure<T>
    {
        TResult Failure(T error);
    }

    public interface IFailure: IFailure<Exception> {}

    public interface IFailure<T>
    {
        bool TryGetError(out T failure);
    }
}
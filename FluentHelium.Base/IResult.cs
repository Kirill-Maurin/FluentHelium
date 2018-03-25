using System;

namespace FluentHelium.Base
{
    public interface IResult<T> : IResult<T, Exception>, IFailure {}

    public interface IResult<T, TFailure, out TResult> : IResult<T, TFailure>, IOption<T, TResult>, IFailure<TFailure, TResult> 
        where TResult : struct, IResult<T, TFailure>, IOption<T, TResult> {}

    public interface IResult<T, TFailure> : IOption<T>, IFailure<TFailure>
    {
        bool TryGet(out T success, out TFailure failure);
    }
}
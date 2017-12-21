using System;

namespace FluentHelium.Module
{
    public static class ChainOfResponsibility
    {
        public static Func<T, T> Insert<T>(this Func<T, T> chain, Func<T, T> prevChain) => b => prevChain(chain(b));
        public static Func<T, T> Or<T>(this Func<T, T> chain, Func<T, T> nextChain) => b => chain(nextChain(b));
        public static T Else<T>(this Func<T, T> chain, T finalHandler) => chain(finalHandler);
    }
}

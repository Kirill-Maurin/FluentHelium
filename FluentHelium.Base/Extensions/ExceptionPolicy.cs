using System;
using System.Threading.Tasks;

namespace FluentHelium.Base
{
    public static class ExceptionPolicy
    {
        public static TOutput ApplyWithOnThrown<T, TOutput>(this T source, Func<T, TOutput> func, Action<T> onThrown)
        {
            try
            {
                return func(source);
            }
            catch 
            {
                onThrown(source);
                throw;
            }
        }

        public static async Task<TOutput> ApplyWithOnThrown<T, TOutput>(this T source, Func<T, Task<TOutput>> func, Action<T> onThrown)
        {
            try
            {
                return await func(source);
            }
            catch
            {
                onThrown(source);
                throw;
            }
        }

        public static void DoWithOnThrown<T>(this T source, Action<T> action, Action<T> onThrown)
        {
            try
            {
                action(source);
            }
            catch
            {
                onThrown(source);
                throw;
            }
        }
    }
}

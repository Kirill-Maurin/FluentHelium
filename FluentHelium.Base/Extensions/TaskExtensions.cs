using System;
using System.Threading.Tasks;
using NullGuard;

namespace FluentHelium.Base
{
    public static class TaskExtensions
    {
        public static T Then<T>(this T source, Action<T> action)
        {
            action(source);
            return source;
        }

        public static Task<T> Canceled<T>() => CanceledTask<T>.Instance;
        
        private class CanceledTask<T>
        {
            public static Task<T> Instance { get; } = new TaskCompletionSource<T>().Then(s => s.SetCanceled()).Task;
        }

        public static Task<T> ToTask<T>(this Exception e)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.TrySetException(e);
            return tcs.Task;
        }

        public static bool TryGetException(this Task task, [AllowNull]out Exception exception)
        {
            exception = task.Exception?.Flatten().InnerExceptions[0];
            return exception != null;
        }

        public static bool TryGetException<T>(this ValueTask<T> task, [AllowNull]out Exception exception)
        {
            exception = default;
            return task.IsFaulted && task.AsTask().TryGetException(out exception);
        }
    }
}

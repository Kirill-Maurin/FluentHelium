using NullGuard;
using System;
using System.Threading.Tasks;

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

        class CanceledTask<T>
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

        public static Task<TResult> SelectMany<T, TResult>(this Task<T> task, Func<T, Task<TResult>> selector)
        {
            if (task.CheckCanceled<TResult>(out var r) || task.CheckException(out r))
                return r;
            var tcs = new TaskCompletionSource<TResult>();
            task.SelectManyWithSource(selector, tcs);
            return tcs.Task;
        }

        public static Task<TResult> Select<T, TResult>(this Task<T> task, Func<T, TResult> selector)
        {
            if (task.CheckCanceled<TResult>(out var r) || task.CheckException(out r))
                return r;
            var tcs = new TaskCompletionSource<TResult>();
            task.SelectWithSource(selector, tcs);
            return tcs.Task;
        }

        public static Task<T> Do<T>(this Task<T> task, Action<T> action)
            => task.Select(t =>
            {
                action(t);
                return t;
            });

        public static Task<TResult> Using<T, TResult>(this Task<T> task, Func<T, Task<TResult>> selector)
            where T : IDisposable
            => task.SelectMany(r => selector(r).Do(t => r.Dispose()));

        static void SelectWithSource<T, TResult>(
            this Task<T> task,
            Func<T, TResult> selector,
            TaskCompletionSource<TResult> tcs)
        {
            if (!task.IsCompleted)
                task.ContinueWith(t =>
                {
                    if (!task.CheckCanceled(tcs) && !task.CheckException(tcs))
                        task.SelectWithSource(selector, tcs);
                });

            try
            {
                tcs.TrySetResult(selector(task.Result));
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
        }

        static void SelectManyWithSource<T, TResult>(
            this Task<T> task,
            Func<T, Task<TResult>> selector,
            TaskCompletionSource<TResult> tcs)
        {
            if (!task.IsCompleted)
                task.ContinueWith(t =>
                {
                    if (!task.CheckCanceled(tcs) && !task.CheckException(tcs))
                        task.SelectManyWithSource(selector, tcs);
                });
            try
            {
                selector(task.Result).ToCompletionSource(tcs);
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
        }

        public static bool CheckCanceled<T>(this Task task, out Task<T> result)
        {
            var c = task.IsCanceled;
            result = c ? Canceled<T>() : default;
            return c;
        }

        public static bool CheckException<T>(this Task task, out Task<T> result)
        {
            var c = task.TryGetException(out var e);
            result = c ? Task.FromException<T>(e) : default;
            return c;
        }

        public static bool CheckCanceled<T>(this Task task, TaskCompletionSource<T> tcs)
        {
            var result = task.IsCanceled;
            if (result)
                tcs.TrySetCanceled();
            return result;
        }

        public static bool CheckException<T>(this Task task, TaskCompletionSource<T> tcs)
        {
            var result = task.TryGetException(out var e);
            if (result)
                tcs.TrySetException(e);
            return result;
        }

        public static Task<T> ToCompletionSource<T>(this Task<T> task, TaskCompletionSource<T> tcs)
        {
            if (!task.CheckCanceled(tcs) && !task.CheckException(tcs) && task.IsCompleted)
                tcs.TrySetResult(task.Result);
            else
                task.ContinueWith(t => t.ToCompletionSource(tcs));
            return tcs.Task;
        }
    }
}

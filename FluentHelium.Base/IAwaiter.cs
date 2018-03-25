using System.Runtime.CompilerServices;

namespace FluentHelium.Base
{
    public interface IAwaiter<out T> : ICriticalNotifyCompletion
    {
        bool IsCompleted { get; } 
        T GetResult();
    }
}
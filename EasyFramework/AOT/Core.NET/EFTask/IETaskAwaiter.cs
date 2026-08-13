/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public interface IETaskAwaiter
    {
        ETaskStatus GetStatus(Guid token);
        void OnCompleted(Action<object> continuation, object state, Guid token);
        void SetException(Guid token, Exception exception);
        void GetResult(Guid token);
        void SetResult(Guid token);
    }
    public interface IETaskAwaiter<T> : IETaskAwaiter
    {
        new T GetResult(Guid token);
        void SetResult(T result, Guid token);
    }
}
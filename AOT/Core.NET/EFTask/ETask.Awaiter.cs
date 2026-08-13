/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        internal static readonly Action<object> InvokeContinuationDelegate = Continuation;
        internal static readonly Action<object> s_sentinel = CompletionSentinel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Continuation(object state)
        {
            ((Action)state).Invoke();
        }
        
        private static void CompletionSentinel(object _) // named method to aid debugging
        {
            throw new InvalidOperationException("The sentinel delegate should never be invoked.");
        }
        
        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly ETask _task;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Awaiter(in ETask task)
            {
                _task = task;
            }

            public bool IsCompleted => _task.Status != ETaskStatus.Pending;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetResult()
            {
                if (_task._source == null) return;
                _task._source.GetResult(_task._token);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action continuation)
            {
                if (_task._source == null)
                {
                    continuation();
                }
                else
                {
                    _task._source.OnCompleted(ETask.InvokeContinuationDelegate, continuation, _task._token);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsafeOnCompleted(Action continuation)
            {
                if (_task._source == null)
                {
                    continuation();
                }
                else
                {
                    _task._source.OnCompleted(ETask.InvokeContinuationDelegate, continuation, _task._token);
                }
            }
        }
    }

    public readonly partial struct ETask<T>
    {
        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly ETask<T> _task;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Awaiter(in ETask<T> task)
            {
                _task = task;
            }

            public bool IsCompleted => _task.Status != ETaskStatus.Pending;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T GetResult()
            {
                if (_task._source == null) return _task._result;
                return _task._source.GetResult(_task._token);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action continuation)
            {
                if (_task._source == null)
                {
                    continuation();
                }
                else
                {
                    _task._source.OnCompleted(ETask.InvokeContinuationDelegate, continuation, _task._token);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsafeOnCompleted(Action continuation)
            {
                if (_task._source == null)
                {
                    continuation();
                }
                else
                {
                    _task._source.OnCompleted(ETask.InvokeContinuationDelegate, continuation, _task._token);
                }
            }
        }
    }
}
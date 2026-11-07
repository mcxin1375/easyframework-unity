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
                    _task._source.OnCompleted(continuation, _task._token);
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
                    _task._source.OnCompleted(continuation, _task._token);
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
                    _task._source.OnCompleted(continuation, _task._token);
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
                    _task._source.OnCompleted(continuation, _task._token);
                }
            }
        }
    }
}

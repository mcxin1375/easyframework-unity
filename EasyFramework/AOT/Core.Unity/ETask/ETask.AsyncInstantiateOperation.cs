/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Threading;
using UnityEngine;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public static ETask<GameObject[]> AsyncInstantiateOperationAsync(AsyncInstantiateOperation<GameObject> asyncInstantiateOperation, CancellationToken cancellationToken = default)
        {
            return new ETask<GameObject[]>(AsyncInstantiateOperationTask.Create(asyncInstantiateOperation, cancellationToken, out var token), token);
        }

        private sealed class AsyncInstantiateOperationTask : TaskAwaiter<GameObject[]>
        {
            private AsyncInstantiateOperation<GameObject> _asyncInstantiateOperation;
            private CancellationToken _cancellationToken;

            protected override bool OnTaskTick()
            {
                if (_asyncInstantiateOperation == null)
                {
                    TrySetResult(null);
                    return false;
                }
                if (_asyncInstantiateOperation.isDone)
                {
                    TrySetResult(_asyncInstantiateOperation.Result);
                    return false;
                }
                if (_cancellationToken.IsCancellationRequested)
                {
                    TrySetResult(null);
                    return false;
                }
                return true;
            }

            protected override void OnTaskStart()
            {
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                _asyncInstantiateOperation = null;
                ObjectPool<AsyncInstantiateOperationTask>.Shared.Return(this);
            }

            public static AsyncInstantiateOperationTask Create(AsyncInstantiateOperation<GameObject> asyncInstantiateOperation, CancellationToken cancellationToken, out Guid token)
            {
                var task = ObjectPool<AsyncInstantiateOperationTask>.Shared.Rent();
                task._asyncInstantiateOperation = asyncInstantiateOperation;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }
    }

    public static class AsyncInstantiateOperationAwaiterExtensions
    {
        public static ETask<GameObject[]>.Awaiter GetAwaiter(this AsyncInstantiateOperation<GameObject> asyncInstantiateOperation)
        {
            return ETask.AsyncInstantiateOperationAsync(asyncInstantiateOperation).GetAwaiter();
        }
    }
}
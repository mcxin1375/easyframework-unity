/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public static ETask<UnityEngine.Object> AssetBundleRequestAsync(AssetBundleRequest assetBundleRequest, CancellationToken cancellationToken = default)
        {
            return new ETask<UnityEngine.Object>(AssetBundleRequestTask.Create(assetBundleRequest, cancellationToken, out var token), token);
        }

        private sealed class AssetBundleRequestTask : TaskAwaiter<UnityEngine.Object>
        {
            private AssetBundleRequest _assetBundleRequest;
            private CancellationToken _cancellationToken;

            protected override bool OnTaskTick()
            {
                if (_assetBundleRequest == null)
                {
                    TrySetResult(null);
                    return false;
                }
                if (_assetBundleRequest.isDone)
                {
                    TrySetResult(_assetBundleRequest.asset);
                    return false;
                }
                if (_cancellationToken.IsCancellationRequested)
                {
                    TrySetResult(null);
                    return false;
                }
                return true;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                _assetBundleRequest = null;
                ObjectPool<AssetBundleRequestTask>.Shared.Return(this);
            }

            public static AssetBundleRequestTask Create(AssetBundleRequest assetBundleRequest, CancellationToken cancellationToken, out Guid token)
            {
                var task = ObjectPool<AssetBundleRequestTask>.Shared.Rent();
                task._assetBundleRequest = assetBundleRequest;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }
    }

    public static class AssetBundleRequestAwaiterExtensions
    {
        public static ETask<UnityEngine.Object>.Awaiter GetAwaiter(this AssetBundleRequest assetBundleRequest)
        {
            return ETask.AssetBundleRequestAsync(assetBundleRequest).GetAwaiter();
        }
    }
}
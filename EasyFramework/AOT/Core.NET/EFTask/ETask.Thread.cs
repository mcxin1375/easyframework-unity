/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public static async ETask RunOnThreadPool(Action action, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ETask.SwitchToThreadPool();

            cancellationToken.ThrowIfCancellationRequested();
            action();
            cancellationToken.ThrowIfCancellationRequested();
        }
        
        public static async ETask RunOnThreadPool(Func<ETask> action, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ETask.SwitchToThreadPool();
            cancellationToken.ThrowIfCancellationRequested();
            await action();
            cancellationToken.ThrowIfCancellationRequested();
        }
        
        public static SwitchToMainThreadAwaitable SwitchToMainThread() => new SwitchToMainThreadAwaitable();
        public static SwitchToThreadPoolAwaitable SwitchToThreadPool()
        {
            return new SwitchToThreadPoolAwaitable();
        }

        // /// <summary>
        // /// Note: use SwitchToThreadPool is recommended.
        // /// </summary>
        // public static SwitchToTaskPoolAwaitable SwitchToTaskPool()
        // {
        //     return new SwitchToTaskPoolAwaitable();
        // }
    }

    public struct SwitchToMainThreadAwaitable
    {
        public Awaiter GetAwaiter() => new Awaiter();
        
        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            // private readonly CancellationToken _cancellationToken;
            
            // [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // public Awaiter(CancellationToken cancellationToken)
            // {
            //     _cancellationToken = cancellationToken;
            // }
    
            public bool IsCompleted
            {
                get
                {
                    var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    if (ETask.ThreadId == currentThreadId)
                    {
                        return true; // run immediate.
                    }
                    else
                    {
                        return false; // register continuation.
                    }
                }
            }
    
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetResult()
            {
                // _cancellationToken.ThrowIfCancellationRequested();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action continuation)
            {
                ETask.AddTick(continuation, false);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsafeOnCompleted(Action continuation)
            {
                ETask.AddTick(continuation, false);
            }
        }
    }
    
    public struct SwitchToThreadPoolAwaitable
    {
        public Awaiter GetAwaiter() => new Awaiter();

        public struct Awaiter : ICriticalNotifyCompletion
        {
            static readonly WaitCallback switchToCallback = Callback;

            public bool IsCompleted => false;
            public void GetResult() { }

            public void OnCompleted(Action continuation)
            {
                ThreadPool.QueueUserWorkItem(switchToCallback, continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
#if NETCOREAPP3_1
                ThreadPool.UnsafeQueueUserWorkItem(ThreadPoolWorkItem.Create(continuation), false);
#else
                ThreadPool.UnsafeQueueUserWorkItem(switchToCallback, continuation);
#endif
            }

            static void Callback(object state)
            {
                var continuation = (Action)state;
                continuation();
            }
        }

#if NETCOREAPP3_1

        sealed class ThreadPoolWorkItem : IThreadPoolWorkItem, ITaskPoolNode<ThreadPoolWorkItem>
        {
            static TaskPool<ThreadPoolWorkItem> pool;
            ThreadPoolWorkItem nextNode;
            public ref ThreadPoolWorkItem NextNode => ref nextNode;

            static ThreadPoolWorkItem()
            {
                TaskPool.RegisterSizeGetter(typeof(ThreadPoolWorkItem), () => pool.Size);
            }

            Action continuation;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ThreadPoolWorkItem Create(Action continuation)
            {
                if (!pool.TryPop(out var item))
                {
                    item = new ThreadPoolWorkItem();
                }

                item.continuation = continuation;
                return item;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Execute()
            {
                var call = continuation;
                continuation = null;
                if (call != null)
                {
                    pool.TryPush(this);
                    call.Invoke();
                }
            }
        }

#endif
    }
    
}
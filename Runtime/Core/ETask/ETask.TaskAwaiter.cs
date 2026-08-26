/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ArgumentException = System.ArgumentException;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public abstract class TaskAwaiter : IETaskAwaiter, ITickerNode
        {
            protected Guid Token;
            protected int Status;
            protected Action<object> _continuation;
            protected object _continuationState;

            protected void Start(out Guid token)
            {
                Token = Guid.NewGuid();
                token = Token;
                Status = ETaskStatusValue.Pending;
                _continuation = null;
                _continuationState = null;
                
                AddTick(this);
                OnTaskStart();
            }

            ETaskStatus IETaskAwaiter.GetStatus(Guid token)
            {
                // Log.Info($"TaskAwaiter.GetStatus: {token}");
                if (Token == Guid.Empty || Token != token) return ETaskStatus.Succeeded;
                return (ETaskStatus)Status;
            }

            void IETaskAwaiter.OnCompleted(Action<object> continuation, object state, Guid token)
            {
                // Log.Info($"TaskAwaiter.OnCompleted");
                if (continuation == null) throw new ArgumentNullException(nameof(continuation));
                if (token != Token) throw new ArgumentException($"Task has already been recycled. token = {token}");

                object oldContinuation = _continuation;
                if (oldContinuation == null)
                {
                    _continuationState = state;
                    oldContinuation = Interlocked.CompareExchange(ref _continuation, continuation, null);
                }

                if (oldContinuation != null)
                {
                    // already running continuation in TrySet.
                    // It will cause call OnCompleted multiple time, invalid.
                    if (!ReferenceEquals(oldContinuation, ETask.s_sentinel))
                    {
                        throw new InvalidOperationException("Already continuation registered, can not await twice or get Status after await.");
                    }
                    continuation(state);
                }
            }

            void IETaskAwaiter.GetResult(Guid token)
            {
                // Log.Info($"TaskAwaiter.GetResult: {token}");
                if (token != Token) throw new ArgumentException($"Task has already been recycled. token = {token}");
                if (Status == ETaskStatusValue.Pending) throw new InvalidOperationException("Not yet completed, UniTask only allow to use await.");

                Token = Guid.Empty;
            }
            void IETaskAwaiter.SetResult(Guid token)
            {
                // Log.Info($"TaskAwaiter.SetResult: {token}");
                if (token != Token) throw new ArgumentException($"Task has already been recycled. token = {token}");
                TrySetResult();
            }

            public void SetException(Guid token, Exception exception)
            {
                // Log.Error($"TaskAwaiter.SetException: {token}, {exception}");
                if (token != Token) throw new ArgumentException($"Task has already been recycled. token = {token}");

                if (exception is OperationCanceledException)
                {
                    FDebug.LogWarning(exception.ToString());
                }
                else
                {
                    FDebug.LogException(exception);
                }
                TrySetException(exception);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TrySetResult()
            {
                // Log.Info($"TaskAwaiter.TrySetResult: {_continuation != null}");
                if (Interlocked.CompareExchange(ref Status, ETaskStatusValue.Succeeded, ETaskStatusValue.Pending) == ETaskStatusValue.Pending)
                {
                    _continuation?.Invoke(_continuationState);
                    // Log.Info($"TaskAwaiter.TrySetResult End");
                    return true;
                }
                return false;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TrySetException(Exception error)
            {
                // Log.Error($"TaskAwaiter.TrySetException: {error}");
                if (Interlocked.CompareExchange(ref Status, ETaskStatusValue.Faulted, ETaskStatusValue.Pending) == ETaskStatusValue.Pending)
                {
                    _continuation?.Invoke(_continuationState);
                    return true;
                }
                return false;
            }

            bool ITickerNode.OnTick()
            {
                if (Token == Guid.Empty) return false;
                if (Status != ETaskStatusValue.Pending) return false;

                return OnTaskTick();
            }

            void ITickerNode.OnRemove()
            {
                // Log.Info("TaskAwaiter.OnRemove");
                Token = Guid.Empty;
                var status = (ETaskStatus)Status;
                Status = ETaskStatusValue.Pending;
                _continuation = null;
                _continuationState = null;
                OnTaskResult(status);
            }

            protected virtual void OnTaskStart() { }
            protected virtual bool OnTaskTick() => true;
            protected virtual void OnTaskResult(ETaskStatus status) { }
        }
        
        public abstract class TaskAwaiter<TResult> : TaskAwaiter, IETaskAwaiter<TResult>
        {
            private TResult _result;
            
            void IETaskAwaiter<TResult>.SetResult(TResult result, Guid token)
            {
                if (token != Token) throw new ArgumentException($"Task has already been recycled. token = {token}");
                if (!TrySetResult(result))
                {
                    throw new ArgumentException($"Task has already been set result. token = {token}");
                }
            }
            TResult IETaskAwaiter<TResult>.GetResult(Guid token)
            {
                if (token != Token) throw new ArgumentException($"Task has already been recycled. token = {token}");
                if (Status == ETaskStatusValue.Pending) throw new InvalidOperationException("Not yet completed, UniTask only allow to use await.");
                return _result;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TrySetResult(TResult result)
            {
                if (Interlocked.CompareExchange(ref Status, ETaskStatusValue.Succeeded, ETaskStatusValue.Pending) == ETaskStatusValue.Pending)
                {
                    _result = result;
                    _continuation?.Invoke(_continuationState);
                    return true;
                }
                return false;
            }
        }
    }
}
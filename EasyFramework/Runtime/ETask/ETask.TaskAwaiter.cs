/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public abstract class TaskAwaiter : IETaskAwaiter, ITickerNode
        {
            private const int Completing = -1;
            // Null/Action: awaiting completion. The remaining markers track the producer's handoff.
            private static readonly Action HandingOff = () => { };
            private static readonly Action Available = () => { };
            private static readonly Action Running = () => { };
            private static readonly Action Invoked = () => { };

            protected Guid Token;
            protected int Status;
            protected ExceptionDispatchInfo Error;
            private Action _continuation;
            private int _consumed; // 0: unread, 1: publishing consumption, 2: consumed.

            protected bool IsPending => Volatile.Read(ref Status) == ETaskStatusValue.Pending;
            protected virtual bool IsCompleted => Volatile.Read(ref _consumed) == 2 && HandoffFinished;
            private bool HandoffFinished
            {
                get
                {
                    var continuation = Volatile.Read(ref _continuation);
                    return ReferenceEquals(continuation, Available) || ReferenceEquals(continuation, Invoked);
                }
            }

            protected void Start(out Guid token)
            {
                // The creator owns initialization; publish only after its start hook has finished.
                token = Token = Guid.NewGuid();
                Status = ETaskStatusValue.Pending;
                _continuation = null;
                Error = null;
                _consumed = 0;
                try { OnTaskStart(); }
                catch (Exception error) { TrySetException(error); }
                AddTick(this);
            }

            ETaskStatus IETaskAwaiter.GetStatus(Guid token)
            {
                ValidateToken(token);
                int status = Volatile.Read(ref Status);
                return status == Completing ? ETaskStatus.Pending : (ETaskStatus)status;
            }

            void IETaskAwaiter.OnCompleted(Action continuation, Guid token)
            {
                if (continuation == null) throw new ArgumentNullException(nameof(continuation));
                ValidateToken(token);
                var current = Interlocked.CompareExchange(ref _continuation, continuation, null);
                if (current == null) return; // The completion producer will take this callback.

                var spinner = new SpinWait();
                while (ReferenceEquals(current, HandingOff))
                {
                    spinner.SpinOnce();
                    current = Volatile.Read(ref _continuation);
                }
                if (!ReferenceEquals(current, Available)
                    || !ReferenceEquals(Interlocked.CompareExchange(ref _continuation, Running, Available), Available))
                    throw new InvalidOperationException("A task can only be awaited once.");
                InvokeContinuation(continuation);
            }

            void IETaskAwaiter.GetResult(Guid token)
            {
                ValidateResult(token);
                var error = Error;
                ConsumeResult();
                error?.Throw();
            }

            void IETaskAwaiter.SetResult(Guid token)
            {
                ValidateToken(token);
                if (!TrySetResult()) throw new InvalidOperationException("The task is already complete.");
            }

            public void SetException(Guid token, Exception exception)
            {
                ValidateToken(token);
                if (!TrySetException(exception)) throw new InvalidOperationException("The task is already complete.");
            }

            public bool TrySetResult()
            {
                if (!TryBeginCompletion()) return false;
                PublishCompletion(null);
                return true;
            }

            public bool TrySetException(Exception error)
            {
                if (error == null) throw new ArgumentNullException(nameof(error));
                if (!TryBeginCompletion()) return false;
                PublishCompletion(error);
                return true;
            }

            protected bool TryBeginCompletion() =>
                Interlocked.CompareExchange(ref Status, Completing, ETaskStatusValue.Pending) == ETaskStatusValue.Pending;

            protected void PublishCompletion(Exception error)
            {
                Error = error == null ? null : ExceptionDispatchInfo.Capture(error);
                Volatile.Write(ref Status, error == null ? ETaskStatusValue.Succeeded
                    : error is OperationCanceledException ? ETaskStatusValue.Canceled : ETaskStatusValue.Faulted);
                var continuation = Interlocked.Exchange(ref _continuation, HandingOff);
                if (continuation == null)
                {
                    // This publication is the producer's last access to this generation.
                    Volatile.Write(ref _continuation, Available);
                    return;
                }
                Volatile.Write(ref _continuation, Running);
                InvokeContinuation(continuation);
            }

            private void InvokeContinuation(Action continuation)
            {
                try { continuation(); }
                catch (Exception error) { EVoidScheduler.PublishUnobservedTaskException(error); }
                finally
                {
                    if (Volatile.Read(ref _consumed) != 0) AddTick(this);
                    // Do not touch this instance after publishing the end of the callback.
                    Volatile.Write(ref _continuation, Invoked);
                }
            }

            protected void ValidateToken(Guid token)
            {
                if (token != Token || token == Guid.Empty || Volatile.Read(ref _consumed) != 0)
                    throw new InvalidOperationException("The task has already been consumed or released.");
            }

            protected void ValidateResult(Guid token)
            {
                ValidateToken(token);
                if (Volatile.Read(ref Status) <= ETaskStatusValue.Pending)
                    throw new InvalidOperationException("The task has not completed.");
            }

            protected void ConsumeResult()
            {
                if (Interlocked.CompareExchange(ref _consumed, 1, 0) != 0)
                    throw new InvalidOperationException("A task can only be consumed once.");
                AddTick(this);
                // Tick must not recycle the source before the new registration is published.
                Volatile.Write(ref _consumed, 2);
            }

            bool ITickerNode.OnTick()
            {
                if (IsCompleted)
                {
                    OnTaskResult(Finish());
                    return false;
                }
                try { return OnTaskTick(); }
                catch (Exception error)
                {
                    TrySetException(error);
                    return false;
                }
            }

            private ETaskStatus Finish()
            {
                var status = (ETaskStatus)Status;
                RemoveTick(this);
                ResetResult();
                Error = null;
                _continuation = null;
                _consumed = 0;
                Token = Guid.Empty;
                return status;
            }

            protected virtual void ResetResult() { }
            protected virtual void OnTaskStart() { }
            protected virtual bool OnTaskTick() =>
                Volatile.Read(ref Status) <= ETaskStatusValue.Pending || Volatile.Read(ref _consumed) != 0 || !HandoffFinished;
            protected virtual void OnTaskResult(ETaskStatus status) { }
        }

        public abstract class TaskAwaiter<TResult> : TaskAwaiter, IETaskAwaiter<TResult>
        {
            private TResult _result;

            void IETaskAwaiter<TResult>.SetResult(TResult result, Guid token)
            {
                ValidateToken(token);
                if (!TrySetResult(result)) throw new InvalidOperationException("The task is already complete.");
            }

            TResult IETaskAwaiter<TResult>.GetResult(Guid token)
            {
                ValidateResult(token);
                var result = _result;
                var error = Error;
                ConsumeResult();
                error?.Throw();
                return result;
            }

            public bool TrySetResult(TResult result)
            {
                if (!TryBeginCompletion()) return false;
                _result = result;
                PublishCompletion(null);
                return true;
            }

            protected override void ResetResult() => _result = default;
        }
    }
}

/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Threading;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public static ETask WaitUntil(Func<bool> predicate)
        {
            return WaitUntil(predicate, default);
        }

        public static ETask WaitUntil(Func<bool> predicate, CancellationToken cancellationToken)
        {
            return new ETask(WaitUntilTask.Create(predicate, false, cancellationToken, out var token), token);
        }

        public static ETask WaitUntil<TState>(TState state, Func<TState, bool> predicate, CancellationToken cancellationToken = default)
        {
            return new ETask(WaitUntilTask<TState>.Create(state, predicate, false, cancellationToken, out var token), token);
        }

        public static ETask WaitWhile(Func<bool> predicate)
        {
            return WaitWhile(predicate, default);
        }

        public static ETask WaitWhile(Func<bool> predicate, CancellationToken cancellationToken)
        {
            return new ETask(WaitUntilTask.Create(predicate, true, cancellationToken, out var token), token);
        }

        public static ETask WaitWhile<TState>(TState state, Func<TState, bool> predicate, CancellationToken cancellationToken = default)
        {
            return new ETask(WaitUntilTask<TState>.Create(state, predicate, true, cancellationToken, out var token), token);
        }

        private sealed class WaitUntilTask : TaskAwaiter
        {
            private Func<bool> _predicate;
            private CancellationToken _cancellationToken;
            private bool _waitWhile;

            protected override bool OnTaskTick()
            {
                if (!IsPending) return base.OnTaskTick();
                if (_cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(_cancellationToken);
                    return false;
                }

                if (_predicate == null || (_waitWhile ? !_predicate() : _predicate()))
                {
                    TrySetResult();
                    return false;
                }
                return true;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                _predicate = null;
                _cancellationToken = default;
                _waitWhile = false;
                ObjectPool<WaitUntilTask>.Shared.Return(this);
            }
            
            public static WaitUntilTask Create(Func<bool> predicate, bool waitWhile, CancellationToken cancellationToken, out Guid token)
            {
                var task = ObjectPool<WaitUntilTask>.Shared.Rent();
                task._predicate = predicate;
                task._waitWhile = waitWhile;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }

        private sealed class WaitUntilTask<TState> : TaskAwaiter
        {
            private TState _state;
            private Func<TState, bool> _predicate;
            private CancellationToken _cancellationToken;
            private bool _waitWhile;

            protected override bool OnTaskTick()
            {
                if (!IsPending) return base.OnTaskTick();
                if (_cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(_cancellationToken);
                    return false;
                }

                bool completed = _predicate == null || (_waitWhile ? !_predicate(_state) : _predicate(_state));
                if (completed)
                {
                    TrySetResult();
                    return false;
                }
                return true;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                _state = default;
                _predicate = null;
                _cancellationToken = default;
                _waitWhile = false;
                ObjectPool<WaitUntilTask<TState>>.Shared.Return(this);
            }

            public static WaitUntilTask<TState> Create(TState state, Func<TState, bool> predicate, bool waitWhile, CancellationToken cancellationToken, out Guid token)
            {
                var task = ObjectPool<WaitUntilTask<TState>>.Shared.Rent();
                task._state = state;
                task._predicate = predicate;
                task._waitWhile = waitWhile;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }
    }
}

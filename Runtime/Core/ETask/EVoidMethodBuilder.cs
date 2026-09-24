using System;
using System.Runtime.CompilerServices;

namespace EasyFramework
{
    public struct EVoidMethodBuilder
    {
        private IEVoidStateMachine _stateMachine;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EVoidMethodBuilder Create()
        {
            return default;
        }

        public EVoid Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult()
        {
            ReturnStateMachine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            ReturnStateMachine();
            EVoidScheduler.PublishUnobservedTaskException(exception);
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (_stateMachine == null)
            {
                EVoid.EVoidStateMachine<TStateMachine>.SetStateMachine(ref stateMachine, ref _stateMachine);
            }

            awaiter.OnCompleted(_stateMachine.MoveNext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (_stateMachine == null)
            {
                EVoid.EVoidStateMachine<TStateMachine>.SetStateMachine(ref stateMachine, ref _stateMachine);
            }

            awaiter.UnsafeOnCompleted(_stateMachine.MoveNext);
        }

        private void ReturnStateMachine()
        {
            var stateMachine = _stateMachine;
            if (stateMachine == null) return;

            _stateMachine = null;
            stateMachine.Return();
        }
    }

    internal interface IEVoidStateMachine
    {
        Action MoveNext { get; }
        void Return();
    }
}

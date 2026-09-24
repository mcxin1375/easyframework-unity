using System;
using System.Runtime.CompilerServices;

namespace EasyFramework
{
    public readonly partial struct EVoid
    {
        internal sealed class EVoidStateMachine<TStateMachine> : IEVoidStateMachine
            where TStateMachine : IAsyncStateMachine
        {
            private TStateMachine _stateMachine;

            public Action MoveNext { get; }

            public EVoidStateMachine()
            {
                MoveNext = Run;
            }

            public static void SetStateMachine(
                ref TStateMachine stateMachine,
                ref IEVoidStateMachine stateMachineRunner)
            {
                var runner = ObjectPool<EVoidStateMachine<TStateMachine>>.Shared.Rent();
                stateMachineRunner = runner;
                runner._stateMachine = stateMachine;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Run()
            {
                _stateMachine.MoveNext();
            }

            public void Return()
            {
                _stateMachine = default;
                ObjectPool<EVoidStateMachine<TStateMachine>>.Shared.Return(this);
            }
        }
    }
}

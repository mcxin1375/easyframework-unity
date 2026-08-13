/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
// using System.Diagnostics;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public class ETaskStateMachine : TaskAwaiter
        {
            // public string DebugText;
            // public string GetDebugText() => DebugText;

            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<ETaskStateMachine>.Shared.Return(this);
            }

            // protected override void OnTickResultCompleted()
            // {
            //     base.OnTickResultCompleted();
            //     
            //     ObjectPool.Return(this);
            // }

            public static ETaskStateMachine Create(out Guid token)
            {
                var task = ObjectPool<ETaskStateMachine>.Shared.Rent();
                task.Start(out token);
                // sm.DebugText = new StackTrace().ToString();
                return task;
            }
        }
        
        public class ETaskStateMachine<T> : TaskAwaiter<T>
        {
            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<ETaskStateMachine<T>>.Shared.Return(this);
            }

            public static ETaskStateMachine<T> Create(out Guid token)
            {
                var task = ObjectPool<ETaskStateMachine<T>>.Shared.Rent();
                task.Start(out token);
                return task;
            }
        }
    }
}
/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        public static ETask WaitUntil(Func<bool> predicate)
        {
            return new ETask(WaitUntilTask.Create(predicate, out var token), token);
        }

        private sealed class WaitUntilTask : TaskAwaiter
        {
            private Func<bool> _predicate;

            protected override bool OnTaskTick()
            {
                if (_predicate == null || _predicate())
                {
                    TrySetResult();
                    return false;
                }
                return true;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<WaitUntilTask>.Shared.Return(this);
            }
            
            public static WaitUntilTask Create(Func<bool> predicate, out Guid token)
            {
                var task = ObjectPool<WaitUntilTask>.Shared.Rent();
                task._predicate = predicate;
                task.Start(out token);
                return task;
            }
        }
    }
}
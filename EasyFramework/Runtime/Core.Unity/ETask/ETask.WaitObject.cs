/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public static class IObjectTaskExtensions
    {
        public static ETask WaitTaskCompleted(this IObjectTask task)
        {
            if (task == null) return ETask.CompletedTask;
            return ETask.WaitObject(task);
        }
    }
    public interface IObjectTask
    {
        bool IsCompleted { get; }
    }

    public readonly partial struct ETask
    {
        public static ETask WaitObject(IObjectTask obj)
        {
            return new ETask(WaitObjectTask.Create(obj, out var token), token);
        }

        private sealed class WaitObjectTask : ETask.TaskAwaiter
        {
            private IObjectTask _objectTask;

            protected override bool OnTaskTick()
            {
                if (_objectTask == null || _objectTask.IsCompleted)
                {
                    TrySetResult();
                    return false;
                }
                return true;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<WaitObjectTask>.Shared.Return(this);
            }
            
            public static WaitObjectTask Create(IObjectTask objectTask, out Guid token)
            {
                var task = ObjectPool<WaitObjectTask>.Shared.Rent();
                task._objectTask = objectTask;
                task.Start(out token);
                return task;
            }
        }
    }
}
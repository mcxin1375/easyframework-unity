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

        public static ETask DelayFrame(int frame = 1) => new (DelayFrameTask.Create(frame, out var token), token);

        public static ETask Delay(TimeSpan timeSpan) => DelaySeconds((float)timeSpan.TotalSeconds);
        public static ETask DelayMilliSeconds(int milliseconds) => Delay(TimeSpan.FromMilliseconds(milliseconds));
        public static ETask DelaySeconds(float duration) => new (DelayTask.Create(duration, out var token), token);

        private sealed class DelayTask : TaskAwaiter
        {
            private float _triggerTime;

            protected override bool OnTaskTick()
            {
                if (ETask.Time >= _triggerTime)
                {
                    TrySetResult();
                    return false;
                }
                return true;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<DelayTask>.Shared.Return(this);
            }
            
            public static DelayTask Create(float duration, out Guid token)
            {
                var task = ObjectPool<DelayTask>.Shared.Rent();
                task._triggerTime = ETask.Time + duration;
                task.Start(out token);
                return task;
            }
        }
        
        private sealed class DelayFrameTask : TaskAwaiter
        {
            private int _delayFrame;

            protected override bool OnTaskTick()
            {
                if (_delayFrame == 0)
                {
                    TrySetResult();
                    return false;
                }
                _delayFrame--;
                return true;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<DelayFrameTask>.Shared.Return(this);
            }
            
            public static DelayFrameTask Create(int delayFrame, out Guid token)
            {
                var task = ObjectPool<DelayFrameTask>.Shared.Rent();
                task._delayFrame = delayFrame;
                task.Start(out token);
                return task;
            }
        }

    }
}
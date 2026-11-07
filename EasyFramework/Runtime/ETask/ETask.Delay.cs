/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Threading;

namespace EasyFramework
{
    public enum ETaskDelayType
    {
        Scaled,
        Unscaled,
        Realtime
    }

    public readonly partial struct ETask
    {

        public static ETask DelayFrame(int frame = 1) => DelayFrame(frame, default);
        public static ETask DelayFrame(int frame, CancellationToken cancellationToken) => new (DelayFrameTask.Create(frame, cancellationToken, out var token), token);

        public static ETask Yield(CancellationToken cancellationToken = default) => DelayFrame(0, cancellationToken);
        public static ETask NextFrame(CancellationToken cancellationToken = default) => DelayFrame(1, cancellationToken);

        public static ETask Delay(TimeSpan timeSpan) => Delay(timeSpan, ETaskDelayType.Scaled);
        public static ETask Delay(TimeSpan timeSpan, CancellationToken cancellationToken) => Delay(timeSpan, ETaskDelayType.Scaled, cancellationToken);
        public static ETask Delay(TimeSpan timeSpan, ETaskDelayType delayType, CancellationToken cancellationToken = default) =>
            DelaySeconds((float)timeSpan.TotalSeconds, delayType, cancellationToken);
        public static ETask DelayMilliSeconds(int milliseconds) => DelayMilliSeconds(milliseconds, ETaskDelayType.Scaled);
        public static ETask DelayMilliSeconds(int milliseconds, CancellationToken cancellationToken) =>
            DelayMilliSeconds(milliseconds, ETaskDelayType.Scaled, cancellationToken);
        public static ETask DelayMilliSeconds(int milliseconds, ETaskDelayType delayType, CancellationToken cancellationToken = default) =>
            Delay(TimeSpan.FromMilliseconds(milliseconds), delayType, cancellationToken);
        public static ETask DelaySeconds(float duration) => DelaySeconds(duration, ETaskDelayType.Scaled);
        public static ETask DelaySeconds(float duration, CancellationToken cancellationToken) =>
            DelaySeconds(duration, ETaskDelayType.Scaled, cancellationToken);
        public static ETask DelaySeconds(float duration, ETaskDelayType delayType, CancellationToken cancellationToken = default) =>
            new (DelayTask.Create(duration, delayType, cancellationToken, out var token), token);

        private sealed class DelayTask : TaskAwaiter
        {
            private float _triggerTime;
            private CancellationToken _cancellationToken;
            private ETaskDelayType _delayType;

            protected override bool OnTaskTick()
            {
                if (!IsPending) return base.OnTaskTick();
                if (_cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(_cancellationToken);
                    return false;
                }
                if (ETask.GetTime(_delayType) >= _triggerTime)
                {
                    TrySetResult();
                    return false;
                }
                return true;
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                _cancellationToken = default;
                _delayType = default;
                ObjectPool<DelayTask>.Shared.Return(this);
            }
            
            public static DelayTask Create(float duration, ETaskDelayType delayType, CancellationToken cancellationToken, out Guid token)
            {
                if (delayType < ETaskDelayType.Scaled || delayType > ETaskDelayType.Realtime)
                    throw new ArgumentOutOfRangeException(nameof(delayType), delayType, null);
                var task = ObjectPool<DelayTask>.Shared.Rent();
                task._triggerTime = ETask.GetTime(delayType) + duration;
                task._cancellationToken = cancellationToken;
                task._delayType = delayType;
                task.Start(out token);
                return task;
            }
        }
        
        private sealed class DelayFrameTask : TaskAwaiter
        {
            private int _delayFrame;
            private CancellationToken _cancellationToken;

            protected override bool OnTaskTick()
            {
                if (!IsPending) return base.OnTaskTick();
                if (_cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(_cancellationToken);
                    return false;
                }
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
                _cancellationToken = default;
                ObjectPool<DelayFrameTask>.Shared.Return(this);
            }
            
            public static DelayFrameTask Create(int delayFrame, CancellationToken cancellationToken, out Guid token)
            {
                var task = ObjectPool<DelayFrameTask>.Shared.Rent();
                task._delayFrame = delayFrame;
                task._cancellationToken = cancellationToken;
                task.Start(out token);
                return task;
            }
        }

    }
}

/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:计时器
//----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EasyFramework
{
    public interface ITimerObject
    {
        bool IsTimerAlive => true;
    }

    public class Timer
    {
        private readonly ConcurrentDictionary<Guid, TimerTask> _timerDict = new();
        private readonly Queue<Guid> _updateList = new(128);
        private bool _needRefresh = true;

        public void Dispose()
        {
            _timerDict.Clear();
            _updateList.Clear();
        }

        public void Tick()
        {
            if (_needRefresh)
            {
                _needRefresh = false;
                _updateList.Clear();
                foreach (var key in _timerDict.Keys) _updateList.Enqueue(key);
            }

            foreach (var guid in _updateList)
            {
                if (_timerDict.TryGetValue(guid, out var task)) task.Tick();
            }
        }

        public Guid Add(float interval, Action action, ITimerObject obj = null)
        {
            _needRefresh = true;
            var task = TimerTask.Create(this, interval, action, obj);
            _timerDict[task.Token] = task;
            
            FDebug.Log($"Timer.Add(interval: {interval}, action: {action}), obj: {obj}) Guid: {task.Token}");
            return task.Token;
        }

        public Guid Add(float interval, Func<bool> func, ITimerObject obj = null)
        {
            _needRefresh = true;
            var task = TimerTask.Create(this, interval, func, obj);
            _timerDict[task.Token] = task;
            
            FDebug.Log($"Timer.Add(interval: {interval}, func: {func}), obj: {obj}) Guid: {task.Token}");
            return task.Token;
        }

        public void Remove(Guid guid)
        {
            FDebug.Log($"Timer.Remove(guid: {guid})");
            
            _needRefresh = true;
            if (_timerDict.TryRemove(guid, out var timerData)) timerData.Return();
        }

        sealed class TimerTask
        {
            public Guid Token;
            private Timer _timer;
            private float _interval;
            private float _triggerTime;
            private ITimerObject _timerObject;
            private Action _action;
            private Func<bool> _delegate;

            public void Tick()
            {
                if (_timerObject is { IsTimerAlive: false })
                {
                    _timer.Remove(Token);
                    return;
                }

                if (ETask.Time < _triggerTime) return;

                try
                {
                    if (_delegate != null)
                    {
                        if (_delegate())
                        {
                            _triggerTime += _interval;
                            return;
                        }
                    }
                    _action?.Invoke();
                    _timer.Remove(Token);
                }
                catch (Exception e)
                {
                    FDebug.LogError(e.ToString());
                    
                    _timer.Remove(Token);
                }
            }

            public void Return()
            {
                ObjectPool<TimerTask>.Shared.Return(this);
            }

            public static TimerTask Create(Timer timer, float interval, Action action, ITimerObject timerObject) => Create(timer, interval, action, null, timerObject);
            public static TimerTask Create(Timer timer, float interval, Func<bool> del, ITimerObject timerObject) => Create(timer, interval, null, del, timerObject);

            private static TimerTask Create(Timer timer, float interval, Action action, Func<bool> del, ITimerObject timerObject)
            {
                var source = ObjectPool<TimerTask>.Shared.Rent();
                source.Token = Guid.NewGuid();
                source._timer = timer;
                source._interval = interval;
                source._triggerTime = ETask.Time + interval;
                source._action = action;
                source._delegate = del;
                source._timerObject = timerObject;
                return source;
            }
        }
    }
}
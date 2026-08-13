/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public readonly partial struct EasyTask
    {
        public static Guid AddTimer(float interval, Action action, ITimerObject obj = null) => ETask.AddTimer(interval, action, obj);
        public static Guid AddTimer(float interval, Func<bool> timerDelegate, ITimerObject obj = null) => ETask.AddTimer(interval, timerDelegate, obj);
        public static void RemoveTimer(Guid guid) => ETask.RemoveTimer(guid);
        
        public static void AddTick(ITickerNode obj) => ETask.AddTick(obj);
        public static void RemoveTick(ITickerNode obj) => ETask.RemoveTick(obj);
        public static void AddTick(Func<bool> func) => ETask.AddTick(func);
        public static void RemoveTick(Func<bool> func) => ETask.RemoveTick(func);
        public static void AddTick(Action action, bool loop = true) => ETask.AddTick(action, loop);
        public static void RemoveTick(Action action) => ETask.RemoveTick(action);
    }
}
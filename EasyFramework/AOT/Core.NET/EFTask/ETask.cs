/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        
        public static int ThreadId => MainThread?.ThreadId ?? 0;
        public static Type ThreadType => MainThread?.GetType();
        public static float Time => MainThread?.Time ?? 0;

        private static readonly IThread MainThread;
        
        private static readonly Timer Timer = new();
        private static readonly Ticker Ticker = new();
        
        static ETask()
        {
#if UNITY_2022_1_OR_NEWER
            MainThread = new UnityThread();
#else
            MainThread = new ETaskThread();
#endif
            FDebug.Log($"[EasyTask] ThreadType: {MainThread.GetType()} ThreadId: {MainThread.ThreadId}");
        }

        private static void Tick()
        {
            try
            {
                Timer.Tick();
                Ticker.Tick();
            }
            catch (Exception e)
            {
                FDebug.LogError(e.ToString());
            }
        }

        private static void Dispose()
        {
            Timer.Dispose();
            Ticker.Dispose();
        }

        public static Guid AddTimer(float interval, Action action, ITimerObject obj = null) => Timer.Add(interval, action, obj);
        public static Guid AddTimer(float interval, Func<bool> timerDelegate, ITimerObject obj = null) => Timer.Add(interval, timerDelegate, obj);
        public static void RemoveTimer(Guid guid) => Timer.Remove(guid);
        
        public static void AddTick(ITickerNode obj) => Ticker.Add(obj);
        public static void RemoveTick(ITickerNode obj) => Ticker.Remove(obj);
        public static void AddTick(Func<bool> func) => Ticker.Add(func);
        public static void RemoveTick(Func<bool> func) => Ticker.Remove(func);
        public static void AddTick(Action action, bool loop = true) => Ticker.Add(action, loop);
        public static void RemoveTick(Action action) => Ticker.Remove(action);
    }

    [AsyncMethodBuilder(typeof(ETaskMethodBuilder))]
    public readonly partial struct ETask
    {
        private readonly IETaskAwaiter _source;
        private readonly Guid _token;

        public ETask(IETaskAwaiter source, Guid token)
        {
            _source = source;
            _token = token;
        }

        public Awaiter GetAwaiter() => new (this);

        public ETaskStatus Status
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_source == null) return ETaskStatus.Succeeded;
                return _source.GetStatus(_token);
            }
        }
        
        public static readonly ETask CompletedTask = new ();
        public static ETask<TResult> FromResult<TResult>(TResult result) => new (result);
    }
    
    [AsyncMethodBuilder(typeof(ETaskMethodBuilder<>))]
    public readonly partial struct ETask<T>
    {
        private readonly IETaskAwaiter<T> _source;
        private readonly Guid _token;
        private readonly T _result;

        public ETask(IETaskAwaiter<T> source, Guid token)
        {
            _source = source;
            _token = token;
            _result = default;
        }
        public ETask(T result)
        {
            _source = null;
            _token = Guid.Empty;
            _result = result;
        }

        public Awaiter GetAwaiter() => new (this);

        public ETaskStatus Status
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_source == null) return ETaskStatus.Succeeded;
                return _source.GetStatus(_token);
            }
        }
    }
}
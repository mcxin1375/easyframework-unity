/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasyFramework
{
    public readonly partial struct ETask
    {
        
        public static int ThreadId => MainThread?.ThreadId ?? 0;
        public static Type ThreadType => MainThread?.GetType();
        public static float Time => MainThread?.Time ?? 0;
        internal static float GetTime(ETaskDelayType delayType) => MainThread?.GetTime(delayType) ?? 0;

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
            }
            catch (Exception e)
            {
                FDebug.LogError(e.ToString());
            }
            try { Ticker.Tick(); }
            catch (Exception e) { FDebug.LogError(e.ToString()); }
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

        private struct TaskExceptionCollector
        {
            private List<Exception> _faults;
            private OperationCanceledException _cancellation;

            public void Add(Exception exception)
            {
                if (exception is OperationCanceledException cancellation)
                {
                    _cancellation ??= cancellation;
                    return;
                }

                (_faults ??= new List<Exception>()).Add(exception);
            }

            public void ThrowIfAny()
            {
                if (_faults != null)
                {
                    if (_faults.Count == 1) ExceptionDispatchInfo.Capture(_faults[0]).Throw();
                    throw new AggregateException(_faults);
                }

                if (_cancellation != null) ExceptionDispatchInfo.Capture(_cancellation).Throw();
            }
        }

        private sealed class WhenAnyState<TResult>
        {
            private WhenAnyTask<TResult> _promise;

            public WhenAnyState(WhenAnyTask<TResult> promise)
            {
                _promise = promise;
            }

            public void TrySetResult(TResult result)
            {
                var promise = Interlocked.Exchange(ref _promise, null);
                promise?.TrySetResult(result);
            }

            public void TrySetException(Exception error)
            {
                var promise = Interlocked.Exchange(ref _promise, null);
                promise?.TrySetException(error);
            }
        }

        private sealed class WhenAnyTask<TResult> : TaskAwaiter<TResult>
        {
            protected override bool OnTaskTick() => false;

            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<WhenAnyTask<TResult>>.Shared.Return(this);
            }

            public static WhenAnyTask<TResult> Create(out Guid token)
            {
                var task = ObjectPool<WhenAnyTask<TResult>>.Shared.Rent();
                task.Start(out token);
                return task;
            }
        }
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

        private async EVoid InnerForget()
        {
            await this;
        }

        public void Forget()
        {
            InnerForget().Forget();
        }

        public static readonly ETask CompletedTask = new ();
        public static ETask<TResult> FromResult<TResult>(TResult result) => new (result);

        /// <summary>Consumes each input once and completes after every input has completed.</summary>
        public static ETask WhenAll(params ETask[] tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0) return CompletedTask;
            return WhenAllCore((ETask[])tasks.Clone());
        }

        public static ETask WhenAll(ETask first, ETask second) => WhenAllCore(first, second);
        public static ETask WhenAll(ETask first, ETask second, ETask third) => WhenAllCore(first, second, third);

        public static ETask<TResult[]> WhenAll<TResult>(params ETask<TResult>[] tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0) return FromResult(Array.Empty<TResult>());
            return WhenAllCore((ETask<TResult>[])tasks.Clone());
        }

        public static ETask<TResult[]> WhenAll<TResult>(ETask<TResult> first, ETask<TResult> second)
            => WhenAllCore(first, second);

        public static ETask<TResult[]> WhenAll<TResult>(ETask<TResult> first, ETask<TResult> second, ETask<TResult> third)
            => WhenAllCore(first, second, third);

        /// <summary>Consumes every input once; later-completing inputs are observed and discarded.</summary>
        public static ETask<int> WhenAny(params ETask[] tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0) throw new ArgumentException("At least one task is required.", nameof(tasks));
            return WhenAnyCore(tasks);
        }

        public static ETask<int> WhenAny(ETask first, ETask second) => WhenAnyCore(first, second);
        public static ETask<int> WhenAny(ETask first, ETask second, ETask third) => WhenAnyCore(first, second, third);

        public static ETask<(int Index, TResult Result)> WhenAny<TResult>(params ETask<TResult>[] tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0) throw new ArgumentException("At least one task is required.", nameof(tasks));
            return WhenAnyCore(tasks);
        }

        public static ETask<(int Index, TResult Result)> WhenAny<TResult>(ETask<TResult> first, ETask<TResult> second)
            => WhenAnyCore(first, second);

        public static ETask<(int Index, TResult Result)> WhenAny<TResult>(ETask<TResult> first, ETask<TResult> second, ETask<TResult> third)
            => WhenAnyCore(first, second, third);

        private static async ETask WhenAllCore(ETask[] tasks)
        {
            var exceptions = default(TaskExceptionCollector);
            for (int i = 0; i < tasks.Length; i++)
            {
                try { await tasks[i]; }
                catch (Exception error) { exceptions.Add(error); }
            }
            exceptions.ThrowIfAny();
        }

        private static async ETask WhenAllCore(ETask first, ETask second)
        {
            var exceptions = default(TaskExceptionCollector);
            try { await first; }
            catch (Exception error) { exceptions.Add(error); }
            try { await second; }
            catch (Exception error) { exceptions.Add(error); }
            exceptions.ThrowIfAny();
        }

        private static async ETask WhenAllCore(ETask first, ETask second, ETask third)
        {
            var exceptions = default(TaskExceptionCollector);
            try { await first; }
            catch (Exception error) { exceptions.Add(error); }
            try { await second; }
            catch (Exception error) { exceptions.Add(error); }
            try { await third; }
            catch (Exception error) { exceptions.Add(error); }
            exceptions.ThrowIfAny();
        }

        private static async ETask<TResult[]> WhenAllCore<TResult>(ETask<TResult>[] tasks)
        {
            var results = new TResult[tasks.Length];
            var exceptions = default(TaskExceptionCollector);
            for (int i = 0; i < tasks.Length; i++)
            {
                try { results[i] = await tasks[i]; }
                catch (Exception error) { exceptions.Add(error); }
            }
            exceptions.ThrowIfAny();
            return results;
        }

        private static async ETask<TResult[]> WhenAllCore<TResult>(ETask<TResult> first, ETask<TResult> second)
        {
            var results = new TResult[2];
            var exceptions = default(TaskExceptionCollector);
            try { results[0] = await first; }
            catch (Exception error) { exceptions.Add(error); }
            try { results[1] = await second; }
            catch (Exception error) { exceptions.Add(error); }
            exceptions.ThrowIfAny();
            return results;
        }

        private static async ETask<TResult[]> WhenAllCore<TResult>(ETask<TResult> first, ETask<TResult> second, ETask<TResult> third)
        {
            var results = new TResult[3];
            var exceptions = default(TaskExceptionCollector);
            try { results[0] = await first; }
            catch (Exception error) { exceptions.Add(error); }
            try { results[1] = await second; }
            catch (Exception error) { exceptions.Add(error); }
            try { results[2] = await third; }
            catch (Exception error) { exceptions.Add(error); }
            exceptions.ThrowIfAny();
            return results;
        }

        private static ETask<int> WhenAnyCore(ETask[] tasks)
        {
            var promise = WhenAnyTask<int>.Create(out var token);
            var state = new WhenAnyState<int>(promise);
            for (int i = 0; i < tasks.Length; i++) RegisterWhenAny(tasks[i], i, state);
            return new ETask<int>(promise, token);
        }

        private static ETask<int> WhenAnyCore(ETask first, ETask second)
        {
            var promise = WhenAnyTask<int>.Create(out var token);
            var state = new WhenAnyState<int>(promise);
            RegisterWhenAny(first, 0, state);
            RegisterWhenAny(second, 1, state);
            return new ETask<int>(promise, token);
        }

        private static ETask<int> WhenAnyCore(ETask first, ETask second, ETask third)
        {
            var promise = WhenAnyTask<int>.Create(out var token);
            var state = new WhenAnyState<int>(promise);
            RegisterWhenAny(first, 0, state);
            RegisterWhenAny(second, 1, state);
            RegisterWhenAny(third, 2, state);
            return new ETask<int>(promise, token);
        }

        private static void RegisterWhenAny(ETask task, int index, WhenAnyState<int> state)
        {
            try
            {
                task.GetAwaiter().OnCompleted(() =>
                {
                    try
                    {
                        task.GetAwaiter().GetResult();
                        state.TrySetResult(index);
                    }
                    catch (Exception error) { state.TrySetException(error); }
                });
            }
            catch (Exception error) { state.TrySetException(error); }
        }

        private static ETask<(int Index, TResult Result)> WhenAnyCore<TResult>(ETask<TResult>[] tasks)
        {
            var promise = WhenAnyTask<(int Index, TResult Result)>.Create(out var token);
            var state = new WhenAnyState<(int Index, TResult Result)>(promise);
            for (int i = 0; i < tasks.Length; i++) RegisterWhenAny(tasks[i], i, state);
            return new ETask<(int Index, TResult Result)>(promise, token);
        }

        private static ETask<(int Index, TResult Result)> WhenAnyCore<TResult>(ETask<TResult> first, ETask<TResult> second)
        {
            var promise = WhenAnyTask<(int Index, TResult Result)>.Create(out var token);
            var state = new WhenAnyState<(int Index, TResult Result)>(promise);
            RegisterWhenAny(first, 0, state);
            RegisterWhenAny(second, 1, state);
            return new ETask<(int Index, TResult Result)>(promise, token);
        }

        private static ETask<(int Index, TResult Result)> WhenAnyCore<TResult>(ETask<TResult> first, ETask<TResult> second, ETask<TResult> third)
        {
            var promise = WhenAnyTask<(int Index, TResult Result)>.Create(out var token);
            var state = new WhenAnyState<(int Index, TResult Result)>(promise);
            RegisterWhenAny(first, 0, state);
            RegisterWhenAny(second, 1, state);
            RegisterWhenAny(third, 2, state);
            return new ETask<(int Index, TResult Result)>(promise, token);
        }

        private static void RegisterWhenAny<TResult>(ETask<TResult> task, int index, WhenAnyState<(int Index, TResult Result)> state)
        {
            try
            {
                task.GetAwaiter().OnCompleted(() =>
                {
                    try { state.TrySetResult((index, task.GetAwaiter().GetResult())); }
                    catch (Exception error) { state.TrySetException(error); }
                });
            }
            catch (Exception error) { state.TrySetException(error); }
        }
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

        private async EVoid InnerForget()
        {
            await this;
        }

        public void Forget()
        {
            InnerForget().Forget();
        }
    }
}

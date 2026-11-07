using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace EasyFramework
{
    public sealed class ETaskCompletionSource : ETask.TaskAwaiter
    {
        public ETask Task { get; }

        public ETaskCompletionSource()
        {
            Start(out var token);
            Task = new ETask(this, token);
        }

        public void SetResult()
        {
            if (!TrySetResult()) throw new InvalidOperationException("The task is already complete.");
        }

        public void SetException(Exception exception)
        {
            if (!TrySetException(exception)) throw new InvalidOperationException("The task is already complete.");
        }

        public void SetCanceled(CancellationToken cancellationToken = default)
        {
            if (!TrySetCanceled(cancellationToken)) throw new InvalidOperationException("The task is already complete.");
        }
    }

    public sealed class ETaskCompletionSource<TResult> : ETask.TaskAwaiter<TResult>
    {
        public ETask<TResult> Task { get; }

        public ETaskCompletionSource()
        {
            Start(out var token);
            Task = new ETask<TResult>(this, token);
        }

        public void SetResult(TResult result)
        {
            if (!TrySetResult(result)) throw new InvalidOperationException("The task is already complete.");
        }

        public void SetException(Exception exception)
        {
            if (!TrySetException(exception)) throw new InvalidOperationException("The task is already complete.");
        }

        public void SetCanceled(CancellationToken cancellationToken = default)
        {
            if (!TrySetCanceled(cancellationToken)) throw new InvalidOperationException("The task is already complete.");
        }
    }

    internal sealed class SharedETaskSource<TResult>
    {
        private readonly object _gate = new object();
        private List<Action> _continuations;
        private ExceptionDispatchInfo _exception;
        private TResult _result;
        private bool _completed;

        public bool IsCompleted
        {
            get { lock (_gate) return _completed; }
        }

        public bool TrySetResult(TResult result) => Complete(result, null);

        public bool TrySetException(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            return Complete(default, ExceptionDispatchInfo.Capture(exception));
        }

        public TResult GetResult()
        {
            TResult result;
            ExceptionDispatchInfo exception;
            lock (_gate)
            {
                if (!_completed) throw new InvalidOperationException("The task has not completed.");
                result = _result;
                exception = _exception;
            }
            exception?.Throw();
            return result;
        }

        public void OnCompleted(Action continuation)
        {
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));
            lock (_gate)
            {
                if (!_completed)
                {
                    (_continuations ??= new List<Action>()).Add(continuation);
                    return;
                }
            }
            Invoke(continuation);
        }

        private bool Complete(TResult result, ExceptionDispatchInfo exception)
        {
            List<Action> continuations;
            lock (_gate)
            {
                if (_completed) return false;
                _result = result;
                _exception = exception;
                _completed = true;
                continuations = _continuations;
                _continuations = null;
            }

            if (continuations != null)
                foreach (var continuation in continuations) Invoke(continuation);
            return true;
        }

        private static void Invoke(Action continuation)
        {
            try { continuation(); }
            catch (Exception exception) { EVoidScheduler.PublishUnobservedTaskException(exception); }
        }
    }

    public readonly struct SharedETask
    {
        private readonly SharedETaskSource<bool> _source;

        internal SharedETask(SharedETaskSource<bool> source) => _source = source;

        public Awaiter GetAwaiter() => new Awaiter(_source);

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly SharedETaskSource<bool> _source;

            internal Awaiter(SharedETaskSource<bool> source) => _source = source;

            public bool IsCompleted => _source == null || _source.IsCompleted;
            public void GetResult() { if (_source != null) _source.GetResult(); }
            public void OnCompleted(Action continuation)
            {
                if (continuation == null) throw new ArgumentNullException(nameof(continuation));
                if (_source == null) continuation();
                else _source.OnCompleted(continuation);
            }
            public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
        }
    }

    public readonly struct SharedETask<TResult>
    {
        private readonly SharedETaskSource<TResult> _source;

        internal SharedETask(SharedETaskSource<TResult> source) => _source = source;

        public Awaiter GetAwaiter() => new Awaiter(_source);

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly SharedETaskSource<TResult> _source;

            internal Awaiter(SharedETaskSource<TResult> source) => _source = source;

            public bool IsCompleted => _source == null || _source.IsCompleted;
            public TResult GetResult() => _source == null ? default : _source.GetResult();
            public void OnCompleted(Action continuation)
            {
                if (continuation == null) throw new ArgumentNullException(nameof(continuation));
                if (_source == null) continuation();
                else _source.OnCompleted(continuation);
            }
            public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
        }
    }

    public readonly partial struct ETask
    {
        public SharedETask Preserve()
        {
            var source = new SharedETaskSource<bool>();
            ObserveShared(this, source).Forget();
            return new SharedETask(source);
        }

        internal static SharedETask<TResult> PreserveShared<TResult>(ETask<TResult> task)
        {
            var source = new SharedETaskSource<TResult>();
            ObserveShared(task, source).Forget();
            return new SharedETask<TResult>(source);
        }

        private static async EVoid ObserveShared(ETask task, SharedETaskSource<bool> source)
        {
            try
            {
                await task;
                source.TrySetResult(true);
            }
            catch (Exception exception) { source.TrySetException(exception); }
        }

        private static async EVoid ObserveShared<TResult>(ETask<TResult> task, SharedETaskSource<TResult> source)
        {
            try { source.TrySetResult(await task); }
            catch (Exception exception) { source.TrySetException(exception); }
        }
    }

    public readonly partial struct ETask<T>
    {
        public SharedETask<T> Preserve() => ETask.PreserveShared(this);
    }
}

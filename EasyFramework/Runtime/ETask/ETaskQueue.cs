/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/11/28
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EasyFramework
{
    public enum ECommandResult
    {
        Succeed = 0,
        Failed = 1,
        Cancelled = 2,
    }

    public interface ICommand
    {
        ETask OnExecuteAsync(CancellationToken token);
        void OnCancel();
    }
    public interface ICommandTask
    {
        ICommand Command { get; }
        void Cancel();
    }
    public class ETaskQueue : ITickerNode
    {
        public readonly bool RunOnNewThread;
        public readonly string Name;

        public IReadOnlyCollection<ICommandTask> Tasks
        {
            get
            {
                lock (_lock)
                {
                    var tasks = new List<ICommandTask>();
                    if (_current != null) tasks.Add(_current);
                    tasks.AddRange(_tasks);
                    return tasks;
                }
            }
        }

        private readonly ConcurrentQueue<CommandTask> _tasks = new();
        private readonly object _lock = new();
        private CommandTask _current;
        private bool _busying;

        private ETaskQueue(bool runOnNewThread = true, string name = "EasyTaskQueue")
        {
            RunOnNewThread = runOnNewThread;
            Name = name;
        }

        public static ETaskQueue Create(bool runOnNewThread = true, string name = "EasyTaskQueue")
        {
            var task = new ETaskQueue(runOnNewThread, name);
            ETask.AddTick(task);
            return task;
        }

        public void Dispose()
        {
            ETask.RemoveTick(this);
            Clear();
        }

        public bool OnTick()
        {
            lock (_lock)
            {
                if (_busying || _tasks.IsEmpty) return true;
                _busying = true;
            }
            DoCommandAsync().Forget();
            return true;
        }

        public void Clear()
        {
            var pending = new List<CommandTask>();
            CommandTask current;
            lock (_lock)
            {
                current = _current;
                current?.Retain();
                while (_tasks.TryDequeue(out var task)) pending.Add(task);
            }

            if (current != null) CancelAndRelease(current);
            foreach (var task in pending) CancelAndRelease(task);
        }

        private static void CancelAndRelease(CommandTask task)
        {
            try { task.Cancel(); }
            catch (Exception error) { FDebug.LogError(error.ToString()); }
            finally { task.Release(); }
        }

        public ETask<ECommandResult> ExecuteAsync(ICommand command, bool forceTick = false)
        {
            var task = CommandTask.Create(command, out var token);
            lock (_lock) _tasks.Enqueue(task);
            if (forceTick) OnTick();
            return new ETask<ECommandResult>(task, token);
        }

        public ETask<ECommandResult> ExecuteAsync(Action action, bool forceTick = false)
        {
            var task = CommandTask.Create(action, out var token);
            lock (_lock) _tasks.Enqueue(task);
            if (forceTick) OnTick();
            return new ETask<ECommandResult>(task, token);
        }

        private async ETask DoCommandAsync()
        {
            try
            {
                while (true)
                {
                    CommandTask task;
                    lock (_lock)
                    {
                        if (!_tasks.TryDequeue(out task)) return;
                        _current = task;
                    }

                    try
                    {
                        if (RunOnNewThread)
                            await ETask.RunOnThreadPool(task.ExecuteAsync);
                        else
                            await task.ExecuteAsync();
                    }
                    finally
                    {
                        lock (_lock)
                        {
                            _current = null;
                            task.Release();
                        }
                    }
                }
            }
            finally
            {
                lock (_lock) _busying = false;
            }
        }

        class CommandTask : ETask.TaskAwaiter<ECommandResult>, ICommandTask
        {
            public ICommand Command { get; private set; }
            public Action Action { get; private set; }
            private readonly object _commandLock = new();
            private CancellationTokenSource _tokenSource;
            private int _holds;
            private bool _cancelRequested;

            protected override bool IsCompleted
            {
                get
                {
                    lock (_commandLock) return _holds == 0 && base.IsCompleted;
                }
            }

            protected override void OnTaskResult(ETaskStatus status)
            {
                Command = null;
                Action = null;
                _tokenSource.Dispose();
                _tokenSource = null;
                ObjectPool<CommandTask>.Shared.Return(this);
            }

            public void Retain()
            {
                lock (_commandLock) _holds++;
            }

            public void Release()
            {
                lock (_commandLock)
                {
                    // Re-register before publishing the last release; a terminal task may be detached.
                    if (_holds == 1) ETask.AddTick(this);
                    _holds--;
                }
            }

            public async ETask ExecuteAsync()
            {
                ICommand command;
                Action action;
                CancellationToken token;
                lock (_commandLock)
                {
                    if (_cancelRequested || !IsPending) return;
                    command = Command;
                    action = Action;
                    token = _tokenSource.Token;
                }

                try
                {
                    if (command != null) await command.OnExecuteAsync(token);
                    action?.Invoke();
                    TrySetResult(ECommandResult.Succeed);
                }
                catch (Exception error)
                {
                    FDebug.LogError(error.ToString());
                    TrySetResult(ECommandResult.Failed);
                }
            }

            public void Cancel()
            {
                ICommand command;
                CancellationTokenSource source;
                lock (_commandLock)
                {
                    if (_holds == 0 || _cancelRequested || !IsPending) return;
                    _cancelRequested = true;
                    _holds++;
                    command = Command;
                    source = _tokenSource;
                }

                try
                {
                    if (!TrySetResult(ECommandResult.Cancelled)) return;
                    try { source.Cancel(); }
                    finally { command?.OnCancel(); }
                }
                finally { Release(); }
            }

            public static CommandTask Create(ICommand command, out Guid token) => Create(command, null, out token);
            public static CommandTask Create(Action action, out Guid token) => Create(null, action, out token);

            private static CommandTask Create(ICommand command, Action action, out Guid token)
            {
                var task = ObjectPool<CommandTask>.Shared.Rent();
                task.Command = command;
                task.Action = action;
                task._tokenSource = new CancellationTokenSource();
                task._holds = 1; // The pending queue owns this hold until execution or Clear ends.
                task._cancelRequested = false;
                task.Start(out token);
                return task;
            }
        }
    }
}

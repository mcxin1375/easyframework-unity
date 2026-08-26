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

        public IReadOnlyCollection<ICommandTask> Tasks => _tasks;
        private readonly ConcurrentQueue<CommandTask> _tasks = new();
        private readonly object _lock = new();
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
            if (!_busying && _tasks.Count > 0)
                _ = DoCommandAsync();

            return true;
        }

        public void Clear()
        {
            foreach (var task in _tasks) task.Cancel();
            _tasks.Clear();
        }

        public ETask<ECommandResult> ExecuteAsync(ICommand command, bool forceTick = false)
        {
            var task = CommandTask.Create(command, out var token);
            _tasks.Enqueue(task);
            if (forceTick) OnTick();
            return new ETask<ECommandResult>(task, token);
        }

        public ETask<ECommandResult> ExecuteAsync(Action action, bool forceTick = false)
        {
            var task = CommandTask.Create(action, out var token);
            _tasks.Enqueue(task);
            if (forceTick) OnTick();
            return new ETask<ECommandResult>(task, token);
        }

        private async ETask DoCommandAsync()
        {
            lock (_lock)
            {
                if (_busying) return;
                _busying = true;
            }

            while (_tasks.Count > 0)
            {
                if (_tasks.TryPeek(out var cmd))
                {
                    if (RunOnNewThread)
                    {
                        await ETask.RunOnThreadPool(cmd.ExecuteAsync);
                    }
                    else
                    {
                        await cmd.ExecuteAsync();
                    }

                    _tasks.TryDequeue(out _);
                }
            }

            _busying = false;
        }

        class CommandTask : ETask.TaskAwaiter<ECommandResult>, ICommandTask
        {
            public ICommand Command { get; private set; }
            public Action Action { get; private set; }
            private CancellationTokenSource _tokenSource;

            protected override void OnTaskResult(ETaskStatus status)
            {
                ObjectPool<CommandTask>.Shared.Return(this);
            }

            protected override bool OnTaskTick()
            {
                return _tokenSource != null && !_tokenSource.IsCancellationRequested;
            }

            public async ETask ExecuteAsync()
            {
                try
                {
                    if (Command != null)
                    {
                        await Command.OnExecuteAsync(_tokenSource.Token);
                    }

                    Action?.Invoke();
                    
                    TrySetResult(ECommandResult.Succeed);
                }
                catch (Exception e)
                {
                    FDebug.LogError(e.ToString());
                    
                    TrySetResult(ECommandResult.Failed);
                }
            }

            public void Cancel()
            {
                TrySetResult(ECommandResult.Cancelled);
                
                _tokenSource?.Cancel();
                _tokenSource = null;
                Command?.OnCancel();
                Command = null;
                Action = null;
            }

            public static CommandTask Create(ICommand command, out Guid token) => Create(command, null, out token);
            public static CommandTask Create(Action action, out Guid token) => Create(null, action, out token);

            private static CommandTask Create(ICommand command, Action action, out Guid token)
            {
                var task = ObjectPool<CommandTask>.Shared.Rent();
                task.Command = command;
                task.Action = action;
                task._tokenSource ??= new CancellationTokenSource();
                task.Start(out token);
                return task;
            }
        }
    }
}
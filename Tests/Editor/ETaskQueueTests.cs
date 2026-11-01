using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class ETaskQueueTests
    {
        private static readonly MethodInfo TickMethod = typeof(ETask).GetMethod("Tick", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly List<ETaskQueue> _queues = new();
        private readonly List<GateCommand> _commands = new();
        private IDebugger _debugger;

        [SetUp]
        public void SetUp()
        {
            _debugger = FDebug.Debugger;
            FDebug.Debugger = new QuietDebugger();
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                foreach (var queue in _queues) queue.Dispose();
                foreach (var command in _commands) command.Complete();
                Tick();
                Tick();
            }
            finally
            {
                _queues.Clear();
                _commands.Clear();
                FDebug.Debugger = _debugger;
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Cancellation_DoesNotReuseTaskWhileItsCommandStillRuns(bool clear)
        {
            var queue = NewQueue();
            var command = NewCommand();
            var first = queue.ExecuteAsync(command);
            var firstHandle = OnlyTask(queue);
            queue.OnTick();
            Assert.That(command.ExecuteCount, Is.EqualTo(1));

            if (clear) queue.Clear();
            else firstHandle.Cancel();
            Assert.That(first.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Cancelled));
            Tick();
            Tick();
            Assert.That(firstHandle.Command, Is.SameAs(command));
            Assert.That(command.CancellationToken.IsCancellationRequested, Is.True);
            Assert.That(command.CancelCount, Is.EqualTo(1));

            int secondCalls = 0;
            var second = queue.ExecuteAsync(() => secondCalls++);
            var secondHandle = LastTask(queue);
            Assert.That(secondHandle, Is.Not.SameAs(firstHandle));
            queue.OnTick();
            Tick();
            Assert.That(secondCalls, Is.Zero);

            command.Complete();
            Assert.That(secondCalls, Is.EqualTo(1));
            Assert.That(second.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Succeed));
            Tick();
            Tick();
            Assert.That(firstHandle.Command, Is.Null);
            Assert.That(secondCalls, Is.EqualTo(1));
        }

        [Test]
        public void Clear_CancelsPendingTasksAndReleasesTheirQueueOwnership()
        {
            var queue = NewQueue();
            var firstCommand = NewCommand();
            var first = queue.ExecuteAsync(firstCommand);
            var firstHandle = OnlyTask(queue);
            firstHandle.Cancel();
            Assert.That(first.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Cancelled));
            TickSource(firstHandle);
            Assert.That(firstHandle.Command, Is.SameAs(firstCommand));

            var secondCommand = NewCommand();
            var second = queue.ExecuteAsync(secondCommand);
            var secondHandle = LastTask(queue);
            Assert.That(secondHandle, Is.Not.SameAs(firstHandle));
            queue.Clear();
            Assert.That(second.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Cancelled));
            Tick();
            Tick();
            Assert.That(queue.Tasks, Is.Empty);
            Assert.That(firstCommand.ExecuteCount, Is.Zero);
            Assert.That(secondCommand.ExecuteCount, Is.Zero);
            Assert.That(firstCommand.CancelCount, Is.EqualTo(1));
            Assert.That(secondCommand.CancelCount, Is.EqualTo(1));
            Assert.That(firstHandle.Command, Is.Null);
            Assert.That(secondHandle.Command, Is.Null);
        }

        [Test]
        public void Clear_DoesNotDiscardCommandsAddedByCancellationCallbacks()
        {
            var queue = NewQueue();
            var command = NewCommand();
            int calls = 0;
            ETask<ECommandResult> reentrant = default;
            command.CancelCallback = () => reentrant = queue.ExecuteAsync(() => calls++);
            var cancelled = queue.ExecuteAsync(command);

            queue.Clear();
            queue.OnTick();
            Assert.That(cancelled.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Cancelled));
            Assert.That(reentrant.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Succeed));
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void ThrowingAction_ReportsFailedAndDoesNotBlockLaterCommands()
        {
            var queue = NewQueue();
            var failed = queue.ExecuteAsync((Action)(() => throw new InvalidOperationException("expected")));
            int calls = 0;
            var succeeding = queue.ExecuteAsync(() => calls++);
            queue.OnTick();

            Assert.That(failed.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Failed));
            Assert.That(succeeding.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Succeed));
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Commands_RunSeriallyUntilThePreviousExecutionActuallyEnds()
        {
            var queue = NewQueue();
            var firstCommand = NewCommand();
            var secondCommand = NewCommand();
            var first = queue.ExecuteAsync(firstCommand);
            var second = queue.ExecuteAsync(secondCommand);
            queue.OnTick();
            queue.OnTick();
            Assert.That(firstCommand.ExecuteCount, Is.EqualTo(1));
            Assert.That(secondCommand.ExecuteCount, Is.Zero);

            firstCommand.Complete();
            Assert.That(first.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Succeed));
            Assert.That(secondCommand.ExecuteCount, Is.EqualTo(1));
            secondCommand.Complete();
            Assert.That(second.GetAwaiter().GetResult(), Is.EqualTo(ECommandResult.Succeed));
            Assert.That(queue.Tasks, Is.Empty);
        }

        private ETaskQueue NewQueue()
        {
            var queue = ETaskQueue.Create(false);
            _queues.Add(queue);
            return queue;
        }

        private GateCommand NewCommand()
        {
            var command = new GateCommand();
            _commands.Add(command);
            return command;
        }

        private static ICommandTask OnlyTask(ETaskQueue queue)
        {
            Assert.That(queue.Tasks.Count, Is.EqualTo(1));
            return LastTask(queue);
        }

        private static ICommandTask LastTask(ETaskQueue queue)
        {
            ICommandTask last = null;
            foreach (var task in queue.Tasks) last = task;
            return last;
        }

        private static void Tick() => TickMethod.Invoke(null, null);
        private static void TickSource(ICommandTask task) => ((ITickerNode)task).OnTick();

        private sealed class GateCommand : ICommand
        {
            private Gate _gate;
            public int ExecuteCount { get; private set; }
            public int CancelCount { get; private set; }
            public CancellationToken CancellationToken { get; private set; }
            public Action CancelCallback { get; set; }

            public ETask OnExecuteAsync(CancellationToken token)
            {
                ExecuteCount++;
                CancellationToken = token;
                _gate = new Gate();
                return _gate.Begin();
            }

            public void OnCancel()
            {
                CancelCount++;
                CancelCallback?.Invoke();
            }

            public void Complete() => _gate?.TrySetResult();
        }

        private sealed class Gate : ETask.TaskAwaiter
        {
            public ETask Begin()
            {
                Start(out var token);
                return new ETask(this, token);
            }
        }

        private sealed class QuietDebugger : IDebugger
        {
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
            public void LogException(Exception exception) { }
        }
    }
}

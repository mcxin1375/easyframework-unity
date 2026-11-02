using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class AsyncEventTests
    {
        private static readonly MethodInfo TickMethod = typeof(ETask).GetMethod("Tick", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly List<Func<Args, ETask>> _actions = new();
        private readonly List<IAsyncEvent<Args>> _handlers = new();
        private readonly List<Action<Args>> _syncActions = new();
        private readonly List<IEvent<Args>> _syncHandlers = new();
        private readonly List<Gate> _gates = new();
        private readonly List<string> _calls = new();
        private IDebugger _previousDebugger;
        private RecordingDebugger _debugger;

        [SetUp]
        public void SetUp()
        {
            _previousDebugger = FDebug.Debugger;
            _debugger = new RecordingDebugger();
            FDebug.Debugger = _debugger;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var action in _actions) F.Event.RemoveAsync(action);
            foreach (var handler in _handlers) F.Event.RemoveAsync(handler);
            foreach (var action in _syncActions) F.Event.Remove(action);
            foreach (var handler in _syncHandlers) F.Event.Remove(handler);
            foreach (var gate in _gates)
            {
                gate.TrySetResult();
                ETask.RemoveTick(gate);
            }
            _actions.Clear();
            _handlers.Clear();
            _syncActions.Clear();
            _syncHandlers.Clear();
            _gates.Clear();
            _calls.Clear();
            Tick();
            Tick();
            FDebug.Debugger = _previousDebugger;
        }

        [Test]
        public void SyncAndAsyncEvents_SameArgumentType_HaveIndependentSubscriptions()
        {
            var syncHandler = new SyncHandler(_ => _calls.Add("sync-object"));
            Action<Args> syncAction = _ => _calls.Add("sync-action");
            _syncHandlers.Add(syncHandler);
            _syncActions.Add(syncAction);
            F.Event.Add<Args>(syncHandler);
            F.Event.Add(syncAction);
            AddHandler(_ => { _calls.Add("async-object"); return ETask.CompletedTask; });
            var asyncAction = AddAction(_ => { _calls.Add("async-action"); return ETask.CompletedTask; });

            F.Event.Invoke(new Args(0));
            Consume(F.Event.InvokeAsync(new Args(0)));
            F.Event.RemoveAsync(asyncAction);
            F.Event.Invoke(new Args(0));
            F.Event.Remove(syncAction);
            Consume(F.Event.InvokeAsync(new Args(0)));

            CollectionAssert.AreEqual(new[]
            {
                "sync-object", "sync-action", "async-object", "async-action",
                "sync-object", "sync-action", "async-object"
            }, _calls);
        }

        [Test]
        public void InvokeAsync_SuspendedDispatch_SynchronousChangesStayIndependent()
        {
            var gate = NewGate();
            AddHandler(async args =>
            {
                _calls.Add("async-start" + args.Value);
                await gate.Task;
                _calls.Add("async-end" + args.Value);
            });
            AddAction(args => { _calls.Add("async-action" + args.Value); return ETask.CompletedTask; });
            var removed = new SyncHandler(args => _calls.Add("removed-sync" + args.Value));
            var added = new SyncHandler(args => _calls.Add("added-sync" + args.Value));
            Action<Args> action = args => _calls.Add("sync-action" + args.Value);
            _syncHandlers.Add(removed);
            _syncHandlers.Add(added);
            _syncActions.Add(action);
            F.Event.Add<Args>(removed);
            F.Event.Add(action);

            var dispatch = F.Event.InvokeAsync(new Args(7));
            Assert.That(dispatch.GetAwaiter().IsCompleted, Is.False);
            F.Event.Invoke(new Args(1));
            F.Event.Remove<Args>(removed);
            F.Event.Add<Args>(added);
            F.Event.Remove(action);
            F.Event.Invoke(new Args(2));
            Assert.That(dispatch.GetAwaiter().IsCompleted, Is.False);
            gate.TrySetResult();
            Consume(dispatch);
            F.Event.Invoke(new Args(3));

            CollectionAssert.AreEqual(new[]
            {
                "async-start7", "removed-sync1", "sync-action1", "added-sync2",
                "async-end7", "async-action7", "added-sync3"
            }, _calls);
        }

        [Test]
        public void InvokeAsync_AwaitsEachObjectBeforeDelegates_AndPreservesArguments()
        {
            var gate = NewGate();
            AddAction(args => { _calls.Add("action" + args.Value); return ETask.CompletedTask; });
            AddHandler(async args =>
            {
                _calls.Add("start" + args.Value);
                await gate.Task;
                _calls.Add("end" + args.Value);
            });
            AddHandler(args => { _calls.Add("object" + args.Value); return ETask.CompletedTask; });

            var dispatch = F.Event.InvokeAsync(new Args(23));

            Assert.That(dispatch.GetAwaiter().IsCompleted, Is.False);
            CollectionAssert.AreEqual(new[] { "start23" }, _calls);
            gate.TrySetResult();
            Consume(dispatch);
            CollectionAssert.AreEqual(new[] { "start23", "end23", "object23", "action23" }, _calls);
        }

        [Test]
        public void InvokeAsync_SubscriptionChangesDuringSuspension_DoNotChangeCapturedGroups()
        {
            var gate = NewGate();
            AsyncHandler later = null;
            Func<Args, ETask> action = null;
            var addedHandler = new AsyncHandler(args => { _calls.Add("added-object" + args.Value); return ETask.CompletedTask; });
            Func<Args, ETask> addedAction = args => { _calls.Add("added-action" + args.Value); return ETask.CompletedTask; };
            _handlers.Add(addedHandler);
            _actions.Add(addedAction);
            AddHandler(args =>
            {
                _calls.Add("first" + args.Value);
                if (args.Value != 1) return ETask.CompletedTask;
                F.Event.RemoveAsync<Args>(later);
                F.Event.RemoveAsync(action);
                F.Event.AddAsync<Args>(addedHandler);
                F.Event.AddAsync(addedAction);
                return gate.Task;
            });
            later = AddHandler(args => { _calls.Add("later" + args.Value); return ETask.CompletedTask; });
            action = AddAction(args => { _calls.Add("action" + args.Value); return ETask.CompletedTask; });

            var dispatch = F.Event.InvokeAsync(new Args(1));
            Assert.That(dispatch.GetAwaiter().IsCompleted, Is.False);
            gate.TrySetResult();
            Consume(dispatch);
            Consume(F.Event.InvokeAsync(new Args(2)));

            CollectionAssert.AreEqual(new[]
            {
                "first1", "later1", "action1", "first2", "added-object2", "added-action2"
            }, _calls);
        }

        [Test]
        public void InvokeAsync_NestedSameTypeDispatch_SuspendsAndResumesItsOwnSnapshot()
        {
            var gate = NewGate();
            AddHandler(async args =>
            {
                _calls.Add("start" + args.Value);
                if (args.Value == 0) await F.Event.InvokeAsync(new Args(1));
                else await gate.Task;
                _calls.Add("end" + args.Value);
            });
            AddHandler(args => { _calls.Add("later" + args.Value); return ETask.CompletedTask; });
            AddAction(args => { _calls.Add("action" + args.Value); return ETask.CompletedTask; });

            var dispatch = F.Event.InvokeAsync(new Args(0));
            Assert.That(dispatch.GetAwaiter().IsCompleted, Is.False);
            CollectionAssert.AreEqual(new[] { "start0", "start1" }, _calls);
            gate.TrySetResult();
            Consume(dispatch);

            CollectionAssert.AreEqual(new[]
            {
                "start0", "start1", "end1", "later1", "action1", "end0", "later0", "action0"
            }, _calls);
        }

        [Test]
        public void InvokeAsync_OverlappingDispatchesCompleteOutOfOrder_AndReuseDoesNotAffectPendingSnapshot()
        {
            var firstGate = NewGate();
            var secondGate = NewGate();
            AddHandler(async args =>
            {
                _calls.Add("start" + args.Value);
                if (args.Value == 1) await firstGate.Task;
                if (args.Value == 2) await secondGate.Task;
                _calls.Add("end" + args.Value);
            });
            AddHandler(args => { _calls.Add("later" + args.Value); return ETask.CompletedTask; });
            AddAction(args => { _calls.Add("action" + args.Value); return ETask.CompletedTask; });

            var first = F.Event.InvokeAsync(new Args(1));
            var second = F.Event.InvokeAsync(new Args(2));
            Assert.That(first.GetAwaiter().IsCompleted, Is.False);
            Assert.That(second.GetAwaiter().IsCompleted, Is.False);
            firstGate.TrySetResult();
            Consume(first);
            Assert.That(second.GetAwaiter().IsCompleted, Is.False);
            AddAction(args => { _calls.Add("added" + args.Value); return ETask.CompletedTask; });
            Consume(F.Event.InvokeAsync(new Args(3)));
            secondGate.TrySetResult();
            Consume(second);

            CollectionAssert.AreEqual(new[]
            {
                "start1", "start2", "end1", "later1", "action1",
                "start3", "end3", "later3", "action3", "added3", "end2", "later2", "action2"
            }, _calls);
        }

        [Test]
        public void InvokeAsync_SynchronousAndAwaitedFailuresAndCancellation_LogAndContinue()
        {
            var failure = NewGate();
            var cancellation = NewGate();
            AddHandler(_ => throw new InvalidOperationException("sync-object"));
            AddHandler(async _ => { await failure.Task; throw new InvalidOperationException("awaited-object"); });
            AddHandler(_ => { _calls.Add("later-object"); return ETask.CompletedTask; });
            AddAction(_ => throw new InvalidOperationException("sync-action"));
            AddAction(_ => cancellation.Task);
            AddAction(_ => { _calls.Add("later-action"); return ETask.CompletedTask; });

            var dispatch = F.Event.InvokeAsync(new Args(0));
            Assert.That(dispatch.GetAwaiter().IsCompleted, Is.False);
            Assert.That(_debugger.Errors, Is.EqualTo(1));
            failure.TrySetResult();
            Assert.That(dispatch.GetAwaiter().IsCompleted, Is.False);
            Assert.That(_debugger.Errors, Is.EqualTo(3));
            cancellation.TrySetException(new OperationCanceledException("cancel-action"));
            Consume(dispatch);

            Assert.That(_debugger.Errors, Is.EqualTo(4));
            CollectionAssert.AreEqual(new[] { "later-object", "later-action" }, _calls);
        }

        [Test]
        public void AddAndRemove_NullDuplicateAndMissingHandlers_AreIgnored()
        {
            F.Event.AddAsync<Args>((IAsyncEvent<Args>)null);
            F.Event.AddAsync<Args>((Func<Args, ETask>)null);
            F.Event.RemoveAsync<Args>((IAsyncEvent<Args>)null);
            F.Event.RemoveAsync<Args>((Func<Args, ETask>)null);
            var handler = AddHandler(_ => { _calls.Add("object"); return ETask.CompletedTask; });
            var action = AddAction(_ => { _calls.Add("action"); return ETask.CompletedTask; });
            F.Event.AddAsync<Args>(handler);
            F.Event.AddAsync(action);

            Consume(F.Event.InvokeAsync(new Args(0)));
            F.Event.RemoveAsync<Args>(handler);
            F.Event.RemoveAsync<Args>(handler);
            F.Event.RemoveAsync(action);
            F.Event.RemoveAsync(action);
            Consume(F.Event.InvokeAsync(new Args(0)));

            CollectionAssert.AreEqual(new[] { "object", "action" }, _calls);
        }

        [Test]
        public void InvokeAsync_CompletedDispatch_ReturnedSnapshotDoesNotRetainUnsubscribedTargets()
        {
            var targets = InvokeAndUnsubscribe(false);
            CollectTargets(targets);
        }

        [Test]
        public void InvokeAsync_LoggerThrows_FinallyReturnsSnapshotAndAllowsRecovery()
        {
            _debugger.ThrowOnError = true;
            var targets = InvokeAndUnsubscribe(true);
            _debugger.ThrowOnError = false;
            CollectTargets(targets);
            int calls = 0;
            Func<RetentionArgs, ETask> recovered = _ => { calls++; return ETask.CompletedTask; };
            F.Event.AddAsync(recovered);
            try { Consume(F.Event.InvokeAsync(default(RetentionArgs))); }
            finally { F.Event.RemoveAsync(recovered); }
            Assert.That(calls, Is.EqualTo(1));
        }

        private static void CollectTargets(WeakReference[] targets)
        {
            Tick();
            Tick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.That(targets[0].IsAlive, Is.False, "The returned snapshot retained its object handler.");
            Assert.That(targets[1].IsAlive, Is.False, "The returned snapshot retained its delegate target.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] InvokeAndUnsubscribe(bool fail)
        {
            var handler = new RetainedHandler { Fail = fail };
            var target = new RetainedAction();
            Func<RetentionArgs, ETask> action = target.Execute;
            F.Event.AddAsync<RetentionArgs>(handler);
            F.Event.AddAsync(action);
            try
            {
                var dispatch = F.Event.InvokeAsync(default(RetentionArgs));
                if (fail) Assert.Throws<InvalidOperationException>(() => dispatch.GetAwaiter().GetResult());
                else Consume(dispatch);
            }
            finally
            {
                F.Event.RemoveAsync<RetentionArgs>(handler);
                F.Event.RemoveAsync(action);
                Tick();
                Tick();
            }
            return new[] { new WeakReference(handler), new WeakReference(target) };
        }

        private AsyncHandler AddHandler(Func<Args, ETask> callback)
        {
            var handler = new AsyncHandler(callback);
            _handlers.Add(handler);
            F.Event.AddAsync<Args>(handler);
            return handler;
        }

        private Func<Args, ETask> AddAction(Func<Args, ETask> action)
        {
            _actions.Add(action);
            F.Event.AddAsync(action);
            return action;
        }

        private Gate NewGate()
        {
            var gate = new Gate();
            gate.Begin();
            _gates.Add(gate);
            return gate;
        }

        private static void Consume(ETask task)
        {
            Assert.That(task.GetAwaiter().IsCompleted, Is.True);
            task.GetAwaiter().GetResult();
            Tick();
            Tick();
        }

        private static void Tick() => TickMethod.Invoke(null, null);
        private readonly struct Args
        {
            public readonly int Value;
            public Args(int value) => Value = value;
        }
        private readonly struct RetentionArgs { }
        private sealed class AsyncHandler : IAsyncEvent<Args>
        {
            private readonly Func<Args, ETask> _callback;
            public AsyncHandler(Func<Args, ETask> callback) => _callback = callback;
            public ETask ExecuteAsync(Args args) => _callback(args);
        }
        private sealed class SyncHandler : IEvent<Args>
        {
            private readonly Action<Args> _callback;
            public SyncHandler(Action<Args> callback) => _callback = callback;
            public void Execute(in Args args) => _callback(args);
        }
        private sealed class Gate : ETask.TaskAwaiter
        {
            public ETask Task { get; private set; }
            public void Begin() { Start(out var token); Task = new ETask(this, token); }
        }
        private sealed class RetainedHandler : IAsyncEvent<RetentionArgs>
        {
            public bool Fail;
            public ETask ExecuteAsync(RetentionArgs args) => Fail
                ? throw new InvalidOperationException("handler") : ETask.CompletedTask;
        }
        private sealed class RetainedAction
        {
            public ETask Execute(RetentionArgs args) => ETask.CompletedTask;
        }
        private sealed class RecordingDebugger : IDebugger
        {
            public int Errors;
            public bool ThrowOnError;
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message)
            {
                Errors++;
                if (ThrowOnError) throw new InvalidOperationException("logger");
            }
            public void LogException(Exception exception) => LogError(exception.ToString());
        }
    }
}

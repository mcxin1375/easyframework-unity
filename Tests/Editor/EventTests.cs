using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class EventTests
    {
        private readonly List<Action<Args>> _actions = new();
        private readonly List<IEvent<Args>> _handlers = new();
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
            foreach (var action in _actions) F.Event.Remove(action);
            foreach (var handler in _handlers) F.Event.Remove(handler);
            _actions.Clear();
            _handlers.Clear();
            _calls.Clear();
            FDebug.Debugger = _previousDebugger;
        }

        [Test]
        public void Invoke_ActionRemovesItself_CurrentDispatchStillCallsLaterAction()
        {
            Action<Args> first = null;
            first = AddAction(_ => { _calls.Add("first"); F.Event.Remove(first); });
            AddAction(_ => _calls.Add("later"));
            Invoke();
            Invoke();
            CollectionAssert.AreEqual(new[] { "first", "later", "later" }, _calls);
            Assert.That(_debugger.Errors, Is.Zero);
        }

        [Test]
        public void Invoke_ObjectRemovesItself_CurrentDispatchStillCallsLaterObject()
        {
            Handler first = null;
            first = AddHandler(_ => { _calls.Add("first"); F.Event.Remove<Args>(first); });
            AddHandler(_ => _calls.Add("later"));
            Invoke();
            Invoke();
            CollectionAssert.AreEqual(new[] { "first", "later", "later" }, _calls);
            Assert.That(_debugger.Errors, Is.Zero);
        }

        [Test]
        public void Invoke_ActionRemovesAnotherAction_RemovalStartsWithNextDispatch()
        {
            Action<Args> later = null;
            AddAction(_ => { _calls.Add("first"); F.Event.Remove(later); });
            later = AddAction(_ => _calls.Add("later"));
            Invoke();
            Invoke();
            CollectionAssert.AreEqual(new[] { "first", "later", "first" }, _calls);
            Assert.That(_debugger.Errors, Is.Zero);
        }

        [Test]
        public void Invoke_ObjectRemovesAnotherObject_RemovalStartsWithNextDispatch()
        {
            Handler later = null;
            AddHandler(_ => { _calls.Add("first"); F.Event.Remove<Args>(later); });
            later = AddHandler(_ => _calls.Add("later"));
            Invoke();
            Invoke();
            CollectionAssert.AreEqual(new[] { "first", "later", "first" }, _calls);
            Assert.That(_debugger.Errors, Is.Zero);
        }

        [Test]
        public void Invoke_ActionAddsAnotherAction_AdditionStartsWithNextDispatch()
        {
            Action<Args> added = _ => _calls.Add("added");
            _actions.Add(added);
            AddAction(_ => { _calls.Add("first"); F.Event.Add(added); });
            Invoke();
            Invoke();
            CollectionAssert.AreEqual(new[] { "first", "first", "added" }, _calls);
        }

        [Test]
        public void Invoke_ObjectAddsAnotherObject_AdditionStartsWithNextDispatch()
        {
            var added = new Handler(_ => _calls.Add("added"));
            _handlers.Add(added);
            AddHandler(_ => { _calls.Add("first"); F.Event.Add<Args>(added); });
            Invoke();
            Invoke();
            CollectionAssert.AreEqual(new[] { "first", "first", "added" }, _calls);
        }

        [Test]
        public void Invoke_ObjectRemovesAction_BothHandlerListsShareEntrySnapshot()
        {
            Action<Args> action = null;
            AddHandler(_ => { _calls.Add("object"); F.Event.Remove(action); });
            action = AddAction(_ => _calls.Add("action"));
            Invoke();
            Invoke();
            CollectionAssert.AreEqual(new[] { "object", "action", "object" }, _calls);
        }

        [Test]
        public void Invoke_ObjectAddsAction_BothHandlerListsShareEntrySnapshot()
        {
            Action<Args> action = _ => _calls.Add("action");
            _actions.Add(action);
            AddHandler(_ => { _calls.Add("object"); F.Event.Add(action); });
            Invoke();
            Invoke();
            CollectionAssert.AreEqual(new[] { "object", "object", "action" }, _calls);
        }

        [Test]
        public void Invoke_NestedActionDispatch_SeesChangesWhileOuterSnapshotIsPreserved()
        {
            Action<Args> later = null;
            Action<Args> added = args => _calls.Add("added" + args.Depth);
            _actions.Add(added);
            AddAction(args =>
            {
                _calls.Add("first" + args.Depth);
                if (args.Depth != 0) return;
                F.Event.Remove(later);
                F.Event.Add(added);
                Invoke(1);
            });
            later = AddAction(args => _calls.Add("later" + args.Depth));
            Invoke();
            CollectionAssert.AreEqual(new[] { "first0", "first1", "added1", "later0" }, _calls);
            Assert.That(_debugger.Errors, Is.Zero);
        }

        [Test]
        public void Invoke_NestedObjectDispatch_SeesChangesWhileOuterSnapshotIsPreserved()
        {
            Handler later = null;
            var added = new Handler(args => _calls.Add("added" + args.Depth));
            _handlers.Add(added);
            AddHandler(args =>
            {
                _calls.Add("first" + args.Depth);
                if (args.Depth != 0) return;
                F.Event.Remove<Args>(later);
                F.Event.Add<Args>(added);
                Invoke(1);
            });
            later = AddHandler(args => _calls.Add("later" + args.Depth));
            Invoke();
            CollectionAssert.AreEqual(new[] { "first0", "first1", "added1", "later0" }, _calls);
            Assert.That(_debugger.Errors, Is.Zero);
        }

        [Test]
        public void Invoke_ThrowingHandlers_DoNotStopEitherHandlerList()
        {
            AddHandler(_ => throw new InvalidOperationException("object"));
            AddHandler(_ => _calls.Add("object"));
            AddAction(_ => throw new InvalidOperationException("action"));
            AddAction(_ => _calls.Add("action"));
            Assert.DoesNotThrow(() => Invoke());
            CollectionAssert.AreEqual(new[] { "object", "action" }, _calls);
            Assert.That(_debugger.Errors, Is.EqualTo(2));
        }

        [Test]
        public void RemoveAndAdd_HandlersMoveToEndOfTheirGroup()
        {
            AddHandler(_ => _calls.Add("object-first"));
            var handler = AddHandler(_ => _calls.Add("object-middle"));
            AddHandler(_ => _calls.Add("object-last"));
            AddAction(_ => _calls.Add("action-first"));
            var action = AddAction(_ => _calls.Add("action-middle"));
            AddAction(_ => _calls.Add("action-last"));
            F.Event.Remove<Args>(handler);
            F.Event.Add<Args>(handler);
            F.Event.Remove(action);
            F.Event.Add(action);

            Invoke();

            CollectionAssert.AreEqual(new[]
            {
                "object-first", "object-last", "object-middle",
                "action-first", "action-last", "action-middle"
            }, _calls);
        }

        [Test]
        public void Invoke_ActionAddsObject_AdditionStartsWithNextDispatch()
        {
            var handler = new Handler(_ => _calls.Add("object"));
            _handlers.Add(handler);
            AddAction(_ => { _calls.Add("action"); F.Event.Add<Args>(handler); });

            Invoke();
            Invoke();

            CollectionAssert.AreEqual(new[] { "action", "object", "action" }, _calls);
        }

        [Test]
        public void AddAndRemove_NullDuplicatesAndMissingHandlers_AreIgnored()
        {
            F.Event.Add<Args>((Action<Args>)null);
            F.Event.Add<Args>((IEvent<Args>)null);
            F.Event.Remove<Args>((Action<Args>)null);
            F.Event.Remove<Args>((IEvent<Args>)null);
            var handler = AddHandler(_ => _calls.Add("object"));
            var action = AddAction(_ => _calls.Add("action"));
            F.Event.Add<Args>(handler);
            F.Event.Add(action);
            Invoke();
            F.Event.Remove<Args>(handler);
            F.Event.Remove<Args>(handler);
            F.Event.Remove(action);
            F.Event.Remove(action);
            Invoke();
            CollectionAssert.AreEqual(new[] { "object", "action" }, _calls);
        }

        [Test]
        public void Invoke_FiveNestedLevelsWithNewInnerHandlers_PreservesBothOuterSnapshots()
        {
            AddHandler(args =>
            {
                _calls.Add("object-first" + args.Depth);
                if (args.Depth == 3)
                {
                    // Grow both inner snapshots while all outer snapshots are still active.
                    for (int i = 0; i < 32; i++)
                    {
                        int index = i;
                        AddHandler(inner => _calls.Add("object-added" + index + ":" + inner.Depth));
                        AddAction(inner => _calls.Add("action-added" + index + ":" + inner.Depth));
                    }
                }
                if (args.Depth < 4) Invoke(args.Depth + 1);
            });
            AddHandler(args => _calls.Add("object-later" + args.Depth));
            AddAction(args => _calls.Add("action-first" + args.Depth));
            AddAction(args => _calls.Add("action-later" + args.Depth));
            var expected = new List<string>();
            for (int depth = 0; depth < 5; depth++) expected.Add("object-first" + depth);
            expected.Add("object-later4");
            for (int i = 0; i < 32; i++) expected.Add("object-added" + i + ":4");
            expected.Add("action-first4");
            expected.Add("action-later4");
            for (int i = 0; i < 32; i++) expected.Add("action-added" + i + ":4");
            for (int depth = 3; depth >= 0; depth--)
            {
                expected.Add("object-later" + depth);
                expected.Add("action-first" + depth);
                expected.Add("action-later" + depth);
            }

            Invoke();

            CollectionAssert.AreEqual(expected, _calls);
            Assert.That(_debugger.Errors, Is.Zero);
        }

        [Test]
        public void Invoke_LoggerThrows_RecoversAndReleasesUnsubscribedSnapshotTargets()
        {
            _debugger.ThrowOnError = true;
            var targets = InvokeWithThrowingLoggerAndUnsubscribe();
            _debugger.ThrowOnError = false;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.That(targets[0].IsAlive, Is.False, "The failed object dispatch retained its target.");
            Assert.That(targets[1].IsAlive, Is.False, "The skipped action snapshot retained its target.");
            int calls = 0;
            Action<RetentionArgs> recovered = _ => calls++;
            F.Event.Add(recovered);
            try
            {
                Assert.DoesNotThrow(() => F.Event.Invoke(default(RetentionArgs)));
                Assert.That(calls, Is.EqualTo(1));
            }
            finally { F.Event.Remove(recovered); }
        }

        [Test]
        public void AddAndRemove_WarmedObjectAndActionSubscriptions_DoNotAllocate()
        {
            var allocated = AllocatedBytesCounter();
            var handler = AddHandler(_ => { });
            var action = AddAction(_ => { });
            for (int i = 0; i < 32; i++)
            {
                F.Event.Remove<Args>(handler);
                F.Event.Add<Args>(handler);
                F.Event.Remove(action);
                F.Event.Add(action);
            }
            allocated();

            long before = allocated();
            for (int i = 0; i < 1024; i++)
            {
                F.Event.Remove<Args>(handler);
                F.Event.Add<Args>(handler);
                F.Event.Remove(action);
                F.Event.Add(action);
            }
            long bytes = allocated() - before;

            Assert.That(bytes, Is.Zero);
        }

        [Test]
        public void Invoke_WarmedStableAndFiveNestedLevels_DoNotAllocate()
        {
            var allocated = AllocatedBytesCounter();
            var stable = AddHandler(_ => { });
            AddAction(_ => { });
            for (int i = 0; i < 32; i++) Invoke();
            allocated();

            long before = allocated();
            for (int i = 0; i < 1024; i++) Invoke();
            long stableBytes = allocated() - before;

            F.Event.Remove<Args>(stable);
            AddHandler(args => { if (args.Depth < 4) Invoke(args.Depth + 1); });
            for (int i = 0; i < 32; i++) Invoke();

            before = allocated();
            for (int i = 0; i < 1024; i++) Invoke();
            long nestedBytes = allocated() - before;

            Assert.That(stableBytes, Is.Zero, "Stable dispatch allocated after warmup.");
            Assert.That(nestedBytes, Is.Zero, "Nested dispatch allocated after warmup.");
        }

        private static Func<long> AllocatedBytesCounter()
        {
            var method = typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread", BindingFlags.Public | BindingFlags.Static);
            if (method == null) Assert.Ignore("This runtime has no per-thread allocation counter.");
            return (Func<long>)Delegate.CreateDelegate(typeof(Func<long>), method);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] InvokeWithThrowingLoggerAndUnsubscribe()
        {
            var handler = new RetainedHandler();
            var actionTarget = new RetainedAction();
            Action<RetentionArgs> action = actionTarget.Receive;
            F.Event.Add<RetentionArgs>(handler);
            F.Event.Add(action);
            try
            {
                Assert.Throws<InvalidOperationException>(() => F.Event.Invoke(default(RetentionArgs)));
            }
            finally
            {
                F.Event.Remove<RetentionArgs>(handler);
                F.Event.Remove(action);
            }
            return new[] { new WeakReference(handler), new WeakReference(actionTarget) };
        }

        private readonly struct RetentionArgs { }

        private sealed class RetainedHandler : IEvent<RetentionArgs>
        {
            public void Execute(in RetentionArgs args) => throw new InvalidOperationException("handler");
        }

        private sealed class RetainedAction
        {
            public void Receive(RetentionArgs args) { }
        }

        private Action<Args> AddAction(Action<Args> action)
        {
            _actions.Add(action);
            F.Event.Add(action);
            return action;
        }

        private Handler AddHandler(Action<Args> callback)
        {
            var handler = new Handler(callback);
            _handlers.Add(handler);
            F.Event.Add<Args>(handler);
            return handler;
        }

        private static void Invoke(int depth = 0) => F.Event.Invoke(new Args(depth));

        private readonly struct Args
        {
            public readonly int Depth;
            public Args(int depth) => Depth = depth;
        }

        private sealed class Handler : IEvent<Args>
        {
            private readonly Action<Args> _callback;
            public Handler(Action<Args> callback) => _callback = callback;
            public void Execute(in Args args) => _callback(args);
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
            public void LogException(Exception exception) => Errors++;
        }
    }
}

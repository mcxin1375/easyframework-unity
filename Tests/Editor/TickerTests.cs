using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class TickerTests
    {
        private const int TimeoutMilliseconds = 5000;
        private IDebugger _debugger;

        [SetUp]
        public void SetUp()
        {
            _debugger = FDebug.Debugger;
            FDebug.Debugger = new QuietDebugger();
        }

        [TearDown]
        public void TearDown() => FDebug.Debugger = _debugger;

        [Test]
        public void Tick_ThrowingNodeStopsButLaterNodesContinue()
        {
            var ticker = new Ticker();
            int throws = 0;
            int laterTicks = 0;
            ticker.Add(new TestNode(() =>
            {
                throws++;
                throw new InvalidOperationException("tick");
            }));
            ticker.Add(new TestNode(() => { laterTicks++; return true; }));

            Assert.DoesNotThrow(ticker.Tick);
            Assert.DoesNotThrow(ticker.Tick);

            Assert.That(throws, Is.EqualTo(1));
            Assert.That(laterTicks, Is.EqualTo(2));
        }

        [Test]
        public void Tick_ReturningFalseStopsRegistration()
        {
            var ticker = new Ticker();
            int ticks = 0;
            ticker.Add(new TestNode(() => { ticks++; return false; }));

            ticker.Tick();
            ticker.Tick();

            Assert.That(ticks, Is.EqualTo(1));
        }

        [Test]
        public void Add_ReplacesPendingRegistrationAtItsNewPosition()
        {
            var ticker = new Ticker();
            var order = new List<string>();
            var first = new TestNode(() => { order.Add("first"); return true; });
            var second = new TestNode(() => { order.Add("second"); return true; });
            ticker.Add(first);
            ticker.Add(second);
            ticker.Add(first);

            ticker.Tick();

            CollectionAssert.AreEqual(new[] { "second", "first" }, order);
        }

        [Test]
        public void Tick_ReplacingLaterRegistrationDefersItUntilNextTick()
        {
            var ticker = new Ticker();
            int laterTicks = 0;
            var later = new TestNode(() => { laterTicks++; return true; });
            ticker.Add(new TestNode(() => { ticker.Add(later); return false; }));
            ticker.Add(later);

            ticker.Tick();
            Assert.That(laterTicks, Is.Zero);
            ticker.Tick();
            Assert.That(laterTicks, Is.EqualTo(1));
        }

        [Test]
        public void Remove_PendingRegistrationIsNeverInvokedAndCanBeAddedAgain()
        {
            var ticker = new Ticker();
            int ticks = 0;
            var node = new TestNode(() => { ticks++; return true; });
            ticker.Add(node);
            ticker.Remove(node);
            ticker.Remove(node);
            ticker.Tick();
            Assert.That(ticks, Is.Zero);

            ticker.Add(node);
            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(1));
        }

        [Test]
        public void Tick_RemovingNextNodeDoesNotSkipFollowingNode()
        {
            var ticker = new Ticker();
            int removedTicks = 0;
            int followingTicks = 0;
            var removed = new TestNode(() => { removedTicks++; return true; });
            ticker.Add(new TestNode(() => { ticker.Remove(removed); return false; }));
            ticker.Add(removed);
            ticker.Add(new TestNode(() => { followingTicks++; return true; }));

            ticker.Tick();
            ticker.Tick();

            Assert.That(removedTicks, Is.Zero);
            Assert.That(followingTicks, Is.EqualTo(2));
        }

        [Test]
        public void Tick_RemovingAndReaddingLaterNodePreservesNextTick()
        {
            var ticker = new Ticker();
            int ticks = 0;
            var target = new TestNode(() => { ticks++; return true; });
            ticker.Add(new TestNode(() =>
            {
                ticker.Remove(target);
                ticker.Add(target);
                return false;
            }));
            ticker.Add(target);

            ticker.Tick();
            Assert.That(ticks, Is.Zero);
            ticker.Tick();
            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Tick_SelfRemovalAndReaddPreservesOnlyNewRegistration(bool oldAlive)
        {
            var ticker = new Ticker();
            int ticks = 0;
            TestNode node = null;
            node = new TestNode(() =>
            {
                ticks++;
                if (ticks == 1)
                {
                    ticker.Remove(node);
                    ticker.Add(node);
                    return oldAlive;
                }
                return false;
            });
            ticker.Add(node);

            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(1));
            ticker.Tick();
            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(2));
        }

        [Test]
        public void Tick_RemovingSelfAndReturningTrueDoesNotRestoreRegistration()
        {
            var ticker = new Ticker();
            int ticks = 0;
            TestNode node = null;
            node = new TestNode(() => { ticks++; ticker.Remove(node); return true; });
            ticker.Add(node);

            ticker.Tick();
            ticker.Tick();

            Assert.That(ticks, Is.EqualTo(1));
        }

        [Test]
        public void Tick_AddingNodeDefersItUntilNextTick()
        {
            var ticker = new Ticker();
            int addedTicks = 0;
            ticker.Add(new TestNode(() =>
            {
                ticker.Add(new TestNode(() => { addedTicks++; return true; }));
                return false;
            }));

            ticker.Tick();
            Assert.That(addedTicks, Is.Zero);
            ticker.Tick();
            Assert.That(addedTicks, Is.EqualTo(1));
        }

        [Test]
        public void Tick_RecursiveCallDoesNotDispatchAgain()
        {
            var ticker = new Ticker();
            int firstTicks = 0;
            int laterTicks = 0;
            ticker.Add(new TestNode(() => { firstTicks++; ticker.Tick(); return true; }));
            ticker.Add(new TestNode(() => { laterTicks++; return true; }));

            ticker.Tick();

            Assert.That(firstTicks, Is.EqualTo(1));
            Assert.That(laterTicks, Is.EqualTo(1));
        }

        [Test]
        public void Tick_UsesReferenceIdentityWithoutCallingTargetEquality()
        {
            var ticker = new Ticker();
            var first = new HostileEqualityNode();
            var second = new HostileEqualityNode();
            ticker.Add(first);
            ticker.Add(second);
            ticker.Add(first);
            ticker.Remove(first);

            ticker.Tick();
            Assert.That(first.Ticks, Is.Zero);
            Assert.That(second.Ticks, Is.EqualTo(1));

            ticker.Add(first);
            ticker.Tick();
            Assert.That(first.Ticks, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Tick_OldCallbackCannotRemoveReusedTargetsNewRegistration(int oldOutcome)
        {
            var ticker = new Ticker();
            var pool = ObjectPool<PooledNode>.Shared;
            pool.Clear();
            using var returned = new ManualResetEventSlim();
            using var releaseOldCall = new ManualResetEventSlim();
            int oldTicks = 0;
            int newTicks = 0;
            int waitTimedOut = 0;
            var node = pool.Rent();
            node.Callback = () =>
            {
                Interlocked.Increment(ref oldTicks);
                pool.Return(node);
                returned.Set();
                if (!releaseOldCall.Wait(TimeoutMilliseconds))
                {
                    Interlocked.Exchange(ref waitTimedOut, 1);
                    throw new TimeoutException("Old callback was not released.");
                }
                if (oldOutcome == 2) throw new InvalidOperationException("old callback");
                return oldOutcome == 1;
            };
            ticker.Add(node);
            var ticking = Task.Run(ticker.Tick);
            try
            {
                Assert.That(returned.Wait(TimeoutMilliseconds), Is.True, "Old callback did not return its target to the pool.");
                var reused = pool.Rent();
                Assert.That(ReferenceEquals(reused, node), Is.True, "The regression requires reuse of the same target instance.");
                reused.Callback = () => { newTicks++; return false; };
                ticker.Add(reused);
                ticker.Tick();
                Assert.That(newTicks, Is.Zero, "A concurrent Tick must not execute the new registration.");
            }
            finally
            {
                releaseOldCall.Set();
                Assert.That(ticking.Wait(TimeoutMilliseconds), Is.True, "Tick did not exit after releasing the old callback.");
                pool.Clear();
            }

            Assert.That(waitTimedOut, Is.Zero);
            ticker.Tick();
            ticker.Tick();
            Assert.That(oldTicks, Is.EqualTo(1));
            Assert.That(newTicks, Is.EqualTo(1));
        }

        [Test]
        public void Remove_WhileCallbackRunsStopsFutureCallsWithoutHoldingCallbackLock()
        {
            var ticker = new Ticker();
            using var entered = new ManualResetEventSlim();
            using var releaseCallback = new ManualResetEventSlim();
            int ticks = 0;
            int waitTimedOut = 0;
            var node = new TestNode(() =>
            {
                Interlocked.Increment(ref ticks);
                entered.Set();
                if (!releaseCallback.Wait(TimeoutMilliseconds)) Interlocked.Exchange(ref waitTimedOut, 1);
                return true;
            });
            ticker.Add(node);
            var ticking = Task.Run(ticker.Tick);
            Task removal = null;
            try
            {
                Assert.That(entered.Wait(TimeoutMilliseconds), Is.True, "Tick callback did not start.");
                removal = Task.Run(() => ticker.Remove(node));
                Assert.That(removal.Wait(TimeoutMilliseconds), Is.True, "Remove must complete while the callback is blocked.");
            }
            finally
            {
                releaseCallback.Set();
                Assert.That(ticking.Wait(TimeoutMilliseconds), Is.True, "Tick did not exit after releasing its callback.");
                if (removal != null) Assert.That(removal.Wait(TimeoutMilliseconds), Is.True);
            }

            Assert.That(waitTimedOut, Is.Zero);
            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(1));
        }

        [Test]
        public void Tick_ThrowingActionAndFuncStopButLaterCallbacksContinue()
        {
            var ticker = new Ticker();
            int laterTicks = 0;
            int throws = 0;
            Action action = () => { throws++; throw new InvalidOperationException("action"); };
            Func<bool> func = () => { throws++; throw new InvalidOperationException("func"); };
            ticker.Add(action, true);
            ticker.Add(func);
            ticker.Add(() => laterTicks++, true);

            ticker.Tick();
            ticker.Tick();

            Assert.That(throws, Is.EqualTo(2));
            Assert.That(laterTicks, Is.EqualTo(2));
        }

        [Test]
        public void Add_ActionReplacementUsesLatestLoopSettingAndPosition()
        {
            var ticker = new Ticker();
            var order = new List<string>();
            Action first = () => order.Add("first");
            Action second = () => order.Add("second");
            ticker.Add(first, true);
            ticker.Add(second, true);
            ticker.Add(first, false);

            ticker.Tick();
            ticker.Tick();

            CollectionAssert.AreEqual(new[] { "second", "first", "second" }, order);
        }

        [Test]
        public void Add_FuncReplacementRunsOnceAtItsNewPosition()
        {
            var ticker = new Ticker();
            var order = new List<string>();
            Func<bool> first = () => { order.Add("first"); return false; };
            Func<bool> second = () => { order.Add("second"); return false; };
            ticker.Add(first);
            ticker.Add(second);
            ticker.Add(first);

            ticker.Tick();
            ticker.Tick();

            CollectionAssert.AreEqual(new[] { "second", "first" }, order);
        }

        [Test]
        public void Tick_ReaddingOneShotActionPreservesNewRegistration()
        {
            var ticker = new Ticker();
            int ticks = 0;
            Action action = null;
            action = () =>
            {
                ticks++;
                if (ticks == 1) ticker.Add(action, false);
            };
            ticker.Add(action, false);

            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(1));
            ticker.Tick();
            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(2));
        }

        [Test]
        public void Tick_ReaddingFuncReturningFalsePreservesNewRegistration()
        {
            var ticker = new Ticker();
            int ticks = 0;
            Func<bool> func = null;
            func = () =>
            {
                ticks++;
                if (ticks == 1) ticker.Add(func);
                return false;
            };
            ticker.Add(func);

            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(1));
            ticker.Tick();
            ticker.Tick();
            Assert.That(ticks, Is.EqualTo(2));
        }

        [Test]
        public void Remove_PendingDelegatesAreNeverInvoked()
        {
            var ticker = new Ticker();
            int ticks = 0;
            Action action = () => ticks++;
            Func<bool> func = () => { ticks++; return true; };
            ticker.Add(action, true);
            ticker.Add(func);
            ticker.Remove(action);
            ticker.Remove(func);
            ticker.Remove(action);
            ticker.Remove(func);

            ticker.Tick();
            ticker.Tick();

            Assert.That(ticks, Is.Zero);
        }

        [Test]
        public void Tick_DisposeSkipsRemainingNodesButAllowsNewRegistrationNextTick()
        {
            var ticker = new Ticker();
            int laterTicks = 0;
            int newTicks = 0;
            ticker.Add(new TestNode(() =>
            {
                ticker.Dispose();
                ticker.Add(new TestNode(() => { newTicks++; return true; }));
                return true;
            }));
            ticker.Add(new TestNode(() => { laterTicks++; return true; }));

            ticker.Tick();
            Assert.That(laterTicks, Is.Zero);
            Assert.That(newTicks, Is.Zero);
            ticker.Tick();
            Assert.That(laterTicks, Is.Zero);
            Assert.That(newTicks, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_DuringLaterCallbackReleasesPreviouslyVisitedTargets()
        {
            var ticker = CreateTickerDisposedDuringTick(out var target);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            bool retained = target.IsAlive;
            GC.KeepAlive(ticker);

            Assert.That(retained, Is.False, "Dispose must release targets even when their callbacks already ran in this tick.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Ticker CreateTickerDisposedDuringTick(out WeakReference target)
        {
            var ticker = new Ticker();
            var first = new TestNode(() => true);
            target = new WeakReference(first);
            ticker.Add(first);
            ticker.Add(new TestNode(() => { ticker.Dispose(); return false; }));
            ticker.Tick();
            return ticker;
        }
        [Test]
        public void Dispose_ClearsActiveAndPendingRegistrationsAndAllowsLaterUse()
        {
            var ticker = new Ticker();
            int oldTicks = 0;
            int newTicks = 0;
            ticker.Add(new TestNode(() => { oldTicks++; return true; }));
            ticker.Tick();
            ticker.Add(new TestNode(() => { oldTicks++; return true; }));

            ticker.Dispose();
            ticker.Dispose();
            ticker.Tick();
            Assert.That(oldTicks, Is.EqualTo(1));

            ticker.Add(() => newTicks++, true);
            ticker.Tick();
            Assert.That(newTicks, Is.EqualTo(1));
        }

        [Test]
        public void ETaskTick_TimerFailureDoesNotBlockTickerOnLaterTicks()
        {
            var tick = typeof(ETask).GetMethod("Tick", BindingFlags.Static | BindingFlags.NonPublic);
            int ticks = 0;
            Action callback = () => ticks++;
            Guid timer = ETask.AddTimer(0, () => { }, new ThrowingTimerObject());
            ETask.AddTick(callback);
            try
            {
                tick.Invoke(null, null);
                tick.Invoke(null, null);
                Assert.That(ticks, Is.EqualTo(2));
            }
            finally
            {
                ETask.RemoveTimer(timer);
                ETask.RemoveTick(callback);
                tick.Invoke(null, null);
            }
        }

        private sealed class ThrowingTimerObject : ITimerObject
        {
            public bool IsTimerAlive => throw new InvalidOperationException("timer lifetime");
        }

        private sealed class QuietDebugger : IDebugger
        {
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
            public void LogException(Exception exception) { }
        }

        private sealed class TestNode : ITickerNode
        {
            private readonly Func<bool> _onTick;

            public TestNode(Func<bool> onTick) => _onTick = onTick;

            public bool OnTick() => _onTick();
        }

        private sealed class HostileEqualityNode : ITickerNode
        {
            public int Ticks { get; private set; }

            public bool OnTick() { Ticks++; return false; }
            public override bool Equals(object obj) => throw new InvalidOperationException("Target equality must not be called.");
            public override int GetHashCode() => throw new InvalidOperationException("Target hash code must not be called.");
        }

        private sealed class PooledNode : ITickerNode
        {
            public Func<bool> Callback;

            public PooledNode() { }

            public bool OnTick() => Callback();
        }
    }
}

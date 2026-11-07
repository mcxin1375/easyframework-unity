using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class ETaskAwaiterTests
    {
        private static readonly MethodInfo TickMethod = typeof(ETask).GetMethod("Tick", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly List<ETask.TaskAwaiter> _sources = new List<ETask.TaskAwaiter>();
        private IDebugger _debugger;

        [SetUp]
        public void SetUp()
        {
            _debugger = FDebug.Debugger;
            FDebug.Debugger = new QuietDebugger();
            Probe.Pool.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var source in _sources)
                if (source is IProbe probe && probe.CleanupCount == 0)
                    ETask.RemoveTick(source);
            _sources.Clear();
            Tick();
            FDebug.Debugger = _debugger;
            Probe.Pool.Clear();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Exception_NonGeneric_IsRethrownBeforeAndAfterSuspension(bool suspended)
        {
            var source = NewProbe();
            var task = source.Begin();
            var expected = CaptureOriginalException();
            Exception observed = null;
            if (suspended) task.GetAwaiter().OnCompleted(() => observed = Catch(() => task.GetAwaiter().GetResult()));
            source.TrySetException(expected);
            if (!suspended) observed = Catch(() => task.GetAwaiter().GetResult());
            Assert.That(observed, Is.SameAs(expected));
            Assert.That(observed.StackTrace, Does.Contain(nameof(ThrowOriginalException)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Exception_Generic_IsRethrownBeforeAndAfterSuspension(bool suspended)
        {
            var source = NewGenericProbe<int>();
            var task = source.Begin();
            var expected = CaptureOriginalException();
            Exception observed = null;
            if (suspended) task.GetAwaiter().OnCompleted(() => observed = Catch(() => task.GetAwaiter().GetResult()));
            source.TrySetException(expected);
            if (!suspended) observed = Catch(() => task.GetAwaiter().GetResult());
            Assert.That(observed, Is.SameAs(expected));
            Assert.That(observed.StackTrace, Does.Contain(nameof(ThrowOriginalException)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Cancellation_NonGeneric_ReportsCanceledAndThrows(bool suspended)
        {
            var source = NewProbe();
            var task = source.Begin();
            var expected = new OperationCanceledException("expected cancellation");
            Exception observed = null;
            ETaskStatus status = ETaskStatus.Pending;
            if (suspended) task.GetAwaiter().OnCompleted(() => { status = task.Status; observed = Catch(() => task.GetAwaiter().GetResult()); });
            source.TrySetException(expected);
            if (!suspended) { status = task.Status; observed = Catch(() => task.GetAwaiter().GetResult()); }
            Assert.That(status, Is.EqualTo(ETaskStatus.Canceled));
            Assert.That(observed, Is.SameAs(expected));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Cancellation_Generic_ReportsCanceledAndThrows(bool suspended)
        {
            var source = NewGenericProbe<int>();
            var task = source.Begin();
            var expected = new OperationCanceledException("expected cancellation");
            Exception observed = null;
            ETaskStatus status = ETaskStatus.Pending;
            if (suspended) task.GetAwaiter().OnCompleted(() => { status = task.Status; observed = Catch(() => task.GetAwaiter().GetResult()); });
            source.TrySetException(expected);
            if (!suspended) { status = task.Status; observed = Catch(() => task.GetAwaiter().GetResult()); }
            Assert.That(status, Is.EqualTo(ETaskStatus.Canceled));
            Assert.That(observed, Is.SameAs(expected));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Forget_NonGeneric_PublishesOriginalFailure(bool suspended)
        {
            var source = NewProbe();
            var task = source.Begin();
            var expected = CaptureOriginalException();
            Exception observed = null;
            Action<Exception> handler = error => observed = error;
            EVoidScheduler.UnobservedTaskException += handler;
            try
            {
                if (suspended) task.Forget();
                source.TrySetException(expected);
                if (!suspended) task.Forget();
                Assert.That(observed, Is.SameAs(expected));
            }
            finally { EVoidScheduler.UnobservedTaskException -= handler; }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Forget_Generic_PublishesOriginalFailure(bool suspended)
        {
            var source = NewGenericProbe<int>();
            var task = source.Begin();
            var expected = CaptureOriginalException();
            Exception observed = null;
            Action<Exception> handler = error => observed = error;
            EVoidScheduler.UnobservedTaskException += handler;
            try
            {
                if (suspended) task.Forget();
                source.TrySetException(expected);
                if (!suspended) task.Forget();
                Assert.That(observed, Is.SameAs(expected));
            }
            finally { EVoidScheduler.UnobservedTaskException -= handler; }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Continuation_RegisteredBeforeOrAfterCompletion_RunsExactlyOnce(bool completedFirst)
        {
            var source = NewProbe();
            var task = source.Begin();
            int count = 0;
            if (completedFirst) source.TrySetResult();
            task.GetAwaiter().OnCompleted(() => count++);
            if (!completedFirst) source.TrySetResult();
            Assert.That(count, Is.EqualTo(1));
            task.GetAwaiter().GetResult();
        }

        [Test]
        public void Continuation_CompletionRacesRegistration_DoesNotLoseCallback()
        {
            for (int i = 0; i < 128; i++)
            {
                var source = NewProbe();
                var task = source.Begin();
                int count = 0;
                using (var start = new ManualResetEventSlim())
                {
                    var registration = Task.Run(() => { start.Wait(); task.GetAwaiter().OnCompleted(() => Interlocked.Increment(ref count)); });
                    var completion = Task.Run(() => { start.Wait(); source.TrySetResult(); });
                    start.Set();
                    Assert.That(Task.WaitAll(new[] { registration, completion }, 5000), Is.True);
                }
                Assert.That(count, Is.EqualTo(1), "Lost or duplicated continuation at iteration " + i);
                task.GetAwaiter().GetResult();
            }
        }

        [Test]
        public void GenericCompletion_PublishesValueBeforeSucceededStatus()
        {
            for (int i = 1; i <= 128; i++)
            {
                var source = NewGenericProbe<int>();
                var task = source.Begin();
                int expected = i;
                var completion = Task.Run(() => source.TrySetResult(expected));
                Assert.That(SpinWait.SpinUntil(() => task.Status != ETaskStatus.Pending, 5000), Is.True);
                int actual = task.GetAwaiter().GetResult();
                Assert.That(completion.Wait(5000), Is.True);
                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [Test]
        public void CompletedTask_RemainsReadableAcrossTicksUntilFirstConsumption()
        {
            var source = NewGenericProbe<int>();
            var task = source.Begin();
            source.TrySetResult(42);
            Tick();
            Tick();
            Assert.That(source.CleanupCount, Is.Zero);
            Assert.That(task.Status, Is.EqualTo(ETaskStatus.Succeeded));
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(42));
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        [Test]
        public void DelayFrame_FirstAwaitAfterSeveralCompletedTicks_Succeeds()
        {
            var task = ETask.DelayFrame(0);
            Tick();
            Tick();
            Tick();
            Assert.That(task.Status, Is.EqualTo(ETaskStatus.Succeeded));
            Assert.That(Catch(() => task.GetAwaiter().GetResult()), Is.Null);
            Assert.That(Catch(() => task.GetAwaiter().GetResult()), Is.Not.Null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Consumption_RejectsRepeatedGetResultAndOldTokenStatus(bool generic)
        {
            if (generic)
            {
                var source = NewGenericProbe<int>();
                var task = source.Begin();
                source.TrySetResult(42);
                Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(42));
                Assert.That(Catch(() => task.GetAwaiter().GetResult()), Is.Not.Null);
                Assert.That(Catch(() => { var ignored = task.Status; }), Is.Not.Null);
            }
            else
            {
                var source = NewProbe();
                var task = source.Begin();
                source.TrySetResult();
                task.GetAwaiter().GetResult();
                Assert.That(Catch(() => task.GetAwaiter().GetResult()), Is.Not.Null);
                Assert.That(Catch(() => { var ignored = task.Status; }), Is.Not.Null);
            }
        }

        [Test]
        public void CompletedSource_DoesNotRunBusinessTickBeforeFirstConsumption()
        {
            var source = NewProbe();
            var task = source.Begin();
            source.TrySetResult();

            Tick();
            Tick();
            Tick();

            Assert.That(source.TickCount, Is.Zero);
            Assert.That(source.CleanupCount, Is.Zero);
            task.GetAwaiter().GetResult();
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }
        [Test]
        public void Consumption_CleansExactlyOnceAndClearsGenericResult()
        {
            var source = NewGenericProbe<object>();
            var task = source.Begin();
            source.TrySetResult(new object());
            Tick();
            Assert.That(source.CleanupCount, Is.Zero);
            task.GetAwaiter().GetResult();
            Tick();
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
            var resultField = typeof(ETask.TaskAwaiter<object>).GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(resultField.GetValue(source), Is.Null, "A pooled source must release its result reference.");
        }

        [Test]
        public void Consumption_WaitsForFinalCompletion()
        {
            var source = NewProbe();
            source.AllowCompletion = false;
            var task = source.Begin();
            source.TrySetResult();
            task.GetAwaiter().GetResult();
            Tick();
            Assert.That(source.CleanupCount, Is.Zero);
            source.AllowCompletion = true;
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        [Test]
        public void Continuation_ConsumingResult_DoesNotRecycleSourceWhileCallbackRuns()
        {
            var source = NewProbe();
            var task = source.Begin();
            Probe rentedInside = null;
            bool cleanedInside = false;
            task.GetAwaiter().OnCompleted(() =>
            {
                task.GetAwaiter().GetResult();
                cleanedInside = source.CleanupCount != 0;
                rentedInside = Probe.Rent();
            });
            source.TrySetResult();
            Assert.That(cleanedInside, Is.False);
            Assert.That(rentedInside, Is.Not.SameAs(source));
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        [Test]
        public void ExternalRemove_CompletedSource_PreservesUnconsumedResult()
        {
            var source = NewGenericProbe<int>();
            var task = source.Begin();
            source.TrySetResult(42);
            ETask.RemoveTick(source);
            Assert.That(source.CleanupCount, Is.Zero);
            Assert.That(task.Status, Is.EqualTo(ETaskStatus.Succeeded));
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(42));
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        [Test]
        public void ExternalRemove_PendingSource_OnlyStopsScheduling()
        {
            var source = NewProbe();
            var task = source.Begin();
            ETask.RemoveTick(source);
            Assert.That(source.CleanupCount, Is.Zero);
            Tick();
            Assert.That(task.Status, Is.EqualTo(ETaskStatus.Pending));
            Assert.That(source.TickCount, Is.Zero);
            var canceled = new OperationCanceledException("explicit cancellation");
            source.TrySetException(canceled);
            Assert.That(Catch(() => task.GetAwaiter().GetResult()), Is.SameAs(canceled));
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AsyncMethod_NonGeneric_PropagatesBodyException(bool suspended)
        {
            var expected = new InvalidOperationException("async method failure");
            var task = FailAsync(expected, suspended);
            if (suspended)
            {
                Assert.That(task.Status, Is.EqualTo(ETaskStatus.Pending));
                Tick();
            }
            Assert.That(task.Status, Is.EqualTo(ETaskStatus.Faulted));
            var observed = Catch(() => task.GetAwaiter().GetResult());
            Assert.That(observed, Is.SameAs(expected));
            Assert.That(observed.StackTrace, Does.Contain(nameof(FailAsync)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AsyncMethod_Generic_PropagatesBodyException(bool suspended)
        {
            var expected = new InvalidOperationException("generic async method failure");
            var task = FailGenericAsync(expected, suspended);
            if (suspended)
            {
                Assert.That(task.Status, Is.EqualTo(ETaskStatus.Pending));
                Tick();
            }
            Assert.That(task.Status, Is.EqualTo(ETaskStatus.Faulted));
            var observed = Catch(() => task.GetAwaiter().GetResult());
            Assert.That(observed, Is.SameAs(expected));
            Assert.That(observed.StackTrace, Does.Contain(nameof(FailGenericAsync)));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Forget_Cancellation_RespectsPublicationPolicy(bool generic, bool propagate)
        {
            var expected = new OperationCanceledException("expected async cancellation");
            var originalPolicy = EVoidScheduler.PropagateOperationCanceledException;
            Exception observed = null;
            int count = 0;
            Action<Exception> handler = error => { observed = error; count++; };
            EVoidScheduler.UnobservedTaskException += handler;
            try
            {
                EVoidScheduler.PropagateOperationCanceledException = propagate;
                if (generic) FailGenericAsync(expected, true).Forget();
                else FailAsync(expected, true).Forget();
                Tick();
                if (propagate) Assert.That(observed, Is.SameAs(expected));
                else Assert.That(observed, Is.Null);
                Assert.That(count, Is.EqualTo(propagate ? 1 : 0));
            }
            finally
            {
                EVoidScheduler.PropagateOperationCanceledException = originalPolicy;
                EVoidScheduler.UnobservedTaskException -= handler;
            }
        }

        [Test]
        public void WaitUntil_PredicateThrows_FaultsWithoutStoppingLaterDelay()
        {
            var expected = new InvalidOperationException("predicate failed");
            var failed = ETask.WaitUntil(() => throw expected);
            var later = ETask.DelayFrame(0);
            Tick();
            Assert.That(failed.Status, Is.EqualTo(ETaskStatus.Faulted));
            Assert.That(Catch(() => failed.GetAwaiter().GetResult()), Is.SameAs(expected));
            Assert.That(later.Status, Is.EqualTo(ETaskStatus.Succeeded));
            Assert.That(Catch(() => later.GetAwaiter().GetResult()), Is.Null);
        }

        [Test]
        public void WaitCondition_StateOverloads_WaitWithoutPredicateClosures()
        {
            var untilState = new WaitConditionState { IsReady = false };
            var whileState = new WaitConditionState { IsReady = true };
            var until = ETask.WaitUntil(untilState, IsReady);
            var whileTask = ETask.WaitWhile(whileState, IsReady);

            Tick();
            Assert.That(until.Status, Is.EqualTo(ETaskStatus.Pending));
            Assert.That(whileTask.Status, Is.EqualTo(ETaskStatus.Pending));

            untilState.IsReady = true;
            whileState.IsReady = false;
            Tick();

            Assert.That(until.Status, Is.EqualTo(ETaskStatus.Succeeded));
            Assert.That(whileTask.Status, Is.EqualTo(ETaskStatus.Succeeded));
            until.GetAwaiter().GetResult();
            whileTask.GetAwaiter().GetResult();
        }

        [Test]
        public void YieldAndNextFrame_UseCurrentAndFollowingTickerPasses()
        {
            var yielded = ETask.Yield();
            var nextFrame = ETask.NextFrame();

            Tick();
            Assert.That(yielded.Status, Is.EqualTo(ETaskStatus.Succeeded));
            Assert.That(nextFrame.Status, Is.EqualTo(ETaskStatus.Pending));
            Tick();
            Assert.That(nextFrame.Status, Is.EqualTo(ETaskStatus.Succeeded));
            yielded.GetAwaiter().GetResult();
            nextFrame.GetAwaiter().GetResult();
        }

        [Test]
        public void DelayAndWaitConditions_CancelWithTheOriginalToken()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var tasks = new[]
                {
                    ETask.Delay(TimeSpan.FromHours(1), cancellation.Token),
                    ETask.DelayFrame(100, cancellation.Token),
                    ETask.WaitUntil(() => false, cancellation.Token),
                    ETask.WaitWhile(() => true, cancellation.Token)
                };

                cancellation.Cancel();
                Tick();

                foreach (var task in tasks)
                {
                    Assert.That(task.Status, Is.EqualTo(ETaskStatus.Canceled));
                    var error = Catch(() => task.GetAwaiter().GetResult());
                    Assert.That(error, Is.TypeOf<OperationCanceledException>());
                    Assert.That(((OperationCanceledException)error).CancellationToken, Is.EqualTo(cancellation.Token));
                }
            }
        }

        [Test]
        public void DelayClockTypes_AcceptZeroDuration()
        {
            var tasks = new[]
            {
                ETask.DelaySeconds(0, ETaskDelayType.Scaled),
                ETask.DelaySeconds(0, ETaskDelayType.Unscaled),
                ETask.DelaySeconds(0, ETaskDelayType.Realtime)
            };

            Tick();

            foreach (var task in tasks)
            {
                Assert.That(task.Status, Is.EqualTo(ETaskStatus.Succeeded));
                task.GetAwaiter().GetResult();
            }
        }

        [Test]
        public void CompletionSource_CompletesOnceAndPropagatesCancellation()
        {
            var completion = new ETaskCompletionSource<int>();
            var task = completion.Task;
            Assert.That(completion.TrySetResult(42), Is.True);
            Assert.That(completion.TrySetException(new InvalidOperationException()), Is.False);
            Assert.That(task.Status, Is.EqualTo(ETaskStatus.Succeeded));
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(42));

            using (var cancellation = new CancellationTokenSource())
            {
                var canceled = new ETaskCompletionSource();
                Assert.That(canceled.TrySetCanceled(cancellation.Token), Is.True);
                Assert.That(canceled.Task.Status, Is.EqualTo(ETaskStatus.Canceled));
                var error = Catch(() => canceled.Task.GetAwaiter().GetResult());
                Assert.That(error, Is.TypeOf<OperationCanceledException>());
                Assert.That(((OperationCanceledException)error).CancellationToken, Is.EqualTo(cancellation.Token));
            }
            Tick();
        }

        [Test]
        public void Preserve_AllowsMultipleConsumersAndRepeatedResultReads()
        {
            var source = NewGenericProbe<int>();
            var task = source.Begin();
            var shared = task.Preserve();
            int first = -1;
            int second = -1;
            shared.GetAwaiter().OnCompleted(() => first = shared.GetAwaiter().GetResult());
            shared.GetAwaiter().OnCompleted(() => second = shared.GetAwaiter().GetResult());

            source.TrySetResult(42);

            Assert.That(first, Is.EqualTo(42));
            Assert.That(second, Is.EqualTo(42));
            Assert.That(shared.GetAwaiter().GetResult(), Is.EqualTo(42));
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));

            var failedSource = NewGenericProbe<int>();
            var failedTask = failedSource.Begin();
            var failedShared = failedTask.Preserve();
            var expected = new InvalidOperationException("shared task failure");
            failedSource.TrySetException(expected);
            Assert.That(Catch(() => failedShared.GetAwaiter().GetResult()), Is.SameAs(expected));
            Assert.That(Catch(() => failedShared.GetAwaiter().GetResult()), Is.SameAs(expected));
            Tick();
            Assert.That(failedSource.CleanupCount, Is.EqualTo(1));
        }

        [Test]
        public void RunOnThreadPool_GenericOverloadsReturnValuesFromWorkerThread()
        {
            int callerThreadId = Thread.CurrentThread.ManagedThreadId;
            int synchronousThreadId = CompleteFromWorker(ETask.RunOnThreadPool(() => Thread.CurrentThread.ManagedThreadId));
            int asynchronousThreadId = CompleteFromWorker(ETask.RunOnThreadPool(
                new Func<ETask<int>>(() => ETask.FromResult(Thread.CurrentThread.ManagedThreadId))));

            Assert.That(synchronousThreadId, Is.Not.EqualTo(callerThreadId));
            Assert.That(asynchronousThreadId, Is.Not.EqualTo(callerThreadId));
            Tick();
        }

        [Test]
        public void WhenAllWaitsForAllInputsAndKeepsResultOrder()
        {
            var first = new ETaskCompletionSource<int>();
            var second = new ETaskCompletionSource<int>();
            var all = ETask.WhenAll(first.Task, second.Task);

            second.SetResult(2);
            Assert.That(all.Status, Is.EqualTo(ETaskStatus.Pending));
            first.SetResult(1);

            CollectionAssert.AreEqual(new[] { 1, 2 }, all.GetAwaiter().GetResult());
            Tick();
        }

        [Test]
        public void WhenAnyReturnsWinnerAndConsumesLaterLoserFailure()
        {
            var winner = new ETaskCompletionSource<int>();
            var loser = new ETaskCompletionSource<int>();
            var any = ETask.WhenAny(winner.Task, loser.Task);

            winner.SetResult(7);
            var result = any.GetAwaiter().GetResult();
            Assert.That(result.Index, Is.EqualTo(0));
            Assert.That(result.Result, Is.EqualTo(7));

            loser.SetException(new InvalidOperationException("discarded loser failure"));
            Tick();
        }

        [Test]
        public void StartHook_Throws_CompletesTaskAsFaulted()
        {
            var source = NewProbe();
            var expected = CaptureOriginalException();
            source.StartError = expected;
            ETask task = default;
            Assert.That(Catch(() => task = source.Begin()), Is.Null);
            Assert.That(task.Status, Is.EqualTo(ETaskStatus.Faulted));
            Assert.That(Catch(() => task.GetAwaiter().GetResult()), Is.SameAs(expected));
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        [Test]
        public void GenericSource_NextGenerationFailure_DoesNotRetainPreviousResult()
        {
            var source = NewGenericProbe<object>();
            var first = source.Begin();
            source.TrySetResult(new object());
            first.GetAwaiter().GetResult();
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));

            var second = source.Begin();
            Assert.That(source.CleanupCount, Is.Zero);
            var expected = CaptureOriginalException();
            source.TrySetException(expected);
            Assert.That(Catch(() => second.GetAwaiter().GetResult()), Is.SameAs(expected));
            var resultField = typeof(ETask.TaskAwaiter<object>).GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(resultField.GetValue(source), Is.Null);
            Assert.That(Catch(() => { var ignored = first.Status; }), Is.Not.Null);
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        private static async ETask FailAsync(Exception error, bool suspended)
        {
            if (suspended) await ETask.DelayFrame(0);
            throw error;
        }

        private static async ETask<int> FailGenericAsync(Exception error, bool suspended)
        {
            if (suspended) await ETask.DelayFrame(0);
            throw error;
        }

        [Test]
        public void StrictCompletion_StaleTokensCannotCompleteNextGeneration()
        {
            var source = NewGenericProbe<int>();
            var first = source.Begin();
            var oldToken = source.CurrentToken;
            source.TrySetResult(1);
            first.GetAwaiter().GetResult();
            Tick();

            var second = source.Begin();
            var strict = (IETaskAwaiter)source;
            Assert.That(Catch(() => strict.SetResult(oldToken)), Is.Not.Null);
            Assert.That(Catch(() => strict.SetException(oldToken, new InvalidOperationException("stale"))), Is.Not.Null);
            Assert.That(Catch(() => ((IETaskAwaiter<int>)source).SetResult(99, oldToken)), Is.Not.Null);
            Assert.That(second.Status, Is.EqualTo(ETaskStatus.Pending));
            Assert.That(source.TrySetResult(2), Is.True);
            Assert.That(second.GetAwaiter().GetResult(), Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void StrictCompletion_DuplicateSetThrowsAndTryReturnsFalse(bool generic)
        {
            var error = new InvalidOperationException("duplicate completion");
            if (generic)
            {
                var source = NewGenericProbe<int>();
                var task = source.Begin();
                var strict = (IETaskAwaiter<int>)source;
                strict.SetResult(42, source.CurrentToken);
                Assert.That(Catch(() => strict.SetResult(99, source.CurrentToken)), Is.Not.Null);
                Assert.That(Catch(() => strict.SetException(source.CurrentToken, error)), Is.Not.Null);
                Assert.That(source.TrySetResult(99), Is.False);
                Assert.That(source.TrySetException(error), Is.False);
                Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(42));
            }
            else
            {
                var source = NewProbe();
                var task = source.Begin();
                var strict = (IETaskAwaiter)source;
                strict.SetException(source.CurrentToken, error);
                Assert.That(Catch(() => strict.SetResult(source.CurrentToken)), Is.Not.Null);
                Assert.That(Catch(() => strict.SetException(source.CurrentToken, error)), Is.Not.Null);
                Assert.That(source.TrySetResult(), Is.False);
                Assert.That(source.TrySetException(error), Is.False);
                Assert.That(Catch(() => task.GetAwaiter().GetResult()), Is.SameAs(error));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Continuation_DuplicateRegistrationIsRejected(bool completedFirst)
        {
            var source = NewProbe();
            var task = source.Begin();
            int originalCalls = 0;
            int duplicateCalls = 0;
            if (completedFirst) source.TrySetResult();
            task.GetAwaiter().OnCompleted(() => originalCalls++);
            Assert.That(Catch(() => task.GetAwaiter().OnCompleted(() => duplicateCalls++)), Is.Not.Null);
            if (!completedFirst) source.TrySetResult();
            Assert.That(originalCalls, Is.EqualTo(1));
            Assert.That(duplicateCalls, Is.Zero);
            task.GetAwaiter().GetResult();
        }

        [Test]
        public void Continuation_ReentrantCompletionDoesNotRecycleBeforeCallbackReturns()
        {
            var source = NewProbe();
            var task = source.Begin();
            bool resultAccepted = true;
            bool errorAccepted = true;
            int cleanupInside = -1;
            task.GetAwaiter().OnCompleted(() =>
            {
                resultAccepted = source.TrySetResult();
                errorAccepted = source.TrySetException(new InvalidOperationException("reentrant"));
                task.GetAwaiter().GetResult();
                Tick();
                cleanupInside = source.CleanupCount;
            });
            Assert.That(source.TrySetResult(), Is.True);
            Assert.That(resultAccepted, Is.False);
            Assert.That(errorAccepted, Is.False);
            Assert.That(cleanupInside, Is.Zero);
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        [Test]
        public void Completion_SuccessAndFailureRace_HasOneConsistentWinner()
        {
            for (int i = 0; i < 16; i++)
            {
                var source = NewGenericProbe<int>();
                var task = source.Begin();
                var error = new InvalidOperationException("racing failure");
                bool successWon = false;
                bool errorWon = false;
                int callbackCount = 0;
                task.GetAwaiter().OnCompleted(() => Interlocked.Increment(ref callbackCount));
                using (var start = new ManualResetEventSlim())
                {
                    var success = Task.Run(() => { start.Wait(); successWon = source.TrySetResult(42); });
                    var failure = Task.Run(() => { start.Wait(); errorWon = source.TrySetException(error); });
                    start.Set();
                    Assert.That(Task.WaitAll(new[] { success, failure }, 5000), Is.True);
                }
                Assert.That(successWon ^ errorWon, Is.True);
                Assert.That(callbackCount, Is.EqualTo(1));
                if (successWon)
                {
                    Assert.That(task.Status, Is.EqualTo(ETaskStatus.Succeeded));
                    Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(42));
                }
                else
                {
                    Assert.That(task.Status, Is.EqualTo(ETaskStatus.Faulted));
                    Assert.That(Catch(() => task.GetAwaiter().GetResult()), Is.SameAs(error));
                }
            }
        }

        [Test]
        public void Continuation_LateRegistrationAfterUnscheduling_ConsumesAndCleansOnce()
        {
            var source = NewProbe();
            var task = source.Begin();
            source.TrySetResult();
            Tick();
            Tick();
            Assert.That(source.CleanupCount, Is.Zero);

            int calls = 0;
            task.GetAwaiter().OnCompleted(() => calls++);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(Catch(() => task.GetAwaiter().OnCompleted(() => calls++)), Is.Not.Null);
            task.GetAwaiter().GetResult();
            Tick();
            Tick();
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        [Test]
        public void Cleanup_PoolReuseBeforeOldTickReturns_DoesNotResetOrRemoveNewGeneration()
        {
            var source = NewProbe();
            var first = source.Begin();
            source.TrySetResult();
            first.GetAwaiter().GetResult();
            ETask.RemoveTick(source);
            var ticker = new Ticker();
            ticker.Add(source);
            using (var pooled = new ManualResetEventSlim())
            using (var allowOldReturn = new ManualResetEventSlim())
            {
                source.AfterReturn = () =>
                {
                    pooled.Set();
                    if (!allowOldReturn.Wait(5000)) throw new TimeoutException("Old tick was not released.");
                };
                var oldTick = Task.Run(ticker.Tick);
                try
                {
                    Assert.That(pooled.Wait(5000), Is.True);
                    var reused = Probe.Rent();
                    Assert.That(reused, Is.SameAs(source));
                    var second = reused.Begin();
                    ETask.RemoveTick(reused);
                    ticker.Add(reused);
                    allowOldReturn.Set();
                    Assert.That(oldTick.Wait(5000), Is.True);
                    Assert.That(second.Status, Is.EqualTo(ETaskStatus.Pending));
                    ticker.Tick();
                    Assert.That(reused.TickCount, Is.EqualTo(1));
                    reused.TrySetResult();
                    second.GetAwaiter().GetResult();
                    ticker.Tick();
                    Assert.That(reused.CleanupCount, Is.EqualTo(1));
                }
                finally
                {
                    allowOldReturn.Set();
                    Assert.That(oldTick.Wait(5000), Is.True);
                    ticker.Dispose();
                }
            }
        }

        [Test]
        public void StartHook_FinishesBeforeSourceIsPublishedToTicker()
        {
            var source = NewProbe();
            using (var entered = new ManualResetEventSlim())
            using (var allowStart = new ManualResetEventSlim())
            {
                source.StartAction = () =>
                {
                    entered.Set();
                    if (!allowStart.Wait(5000)) throw new TimeoutException("Start hook was not released.");
                };
                var creation = Task.Run(source.Begin);
                try
                {
                    Assert.That(entered.Wait(5000), Is.True);
                    Tick();
                    Assert.That(source.TickCount, Is.Zero);
                }
                finally
                {
                    allowStart.Set();
                    Assert.That(creation.Wait(5000), Is.True);
                }
                var task = creation.Result;
                source.TrySetResult();
                task.GetAwaiter().GetResult();
                Tick();
                Assert.That(source.CleanupCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Continuation_ConsumeRemoveAndTick_DefersCleanupUntilCallbackReturns()
        {
            var source = NewProbe();
            var task = source.Begin();
            int cleanupInside = -1;
            task.GetAwaiter().OnCompleted(() =>
            {
                task.GetAwaiter().GetResult();
                ETask.RemoveTick(source);
                Tick();
                cleanupInside = source.CleanupCount;
            });
            source.TrySetResult();
            Assert.That(cleanupInside, Is.Zero);
            Tick();
            Tick();
            Assert.That(source.CleanupCount, Is.EqualTo(1));
        }

        private Probe NewProbe()
        {
            var source = new Probe();
            _sources.Add(source);
            return source;
        }

        private GenericProbe<T> NewGenericProbe<T>()
        {
            var source = new GenericProbe<T>();
            _sources.Add(source);
            return source;
        }

        private static TResult CompleteFromWorker<TResult>(ETask<TResult> task)
        {
            using (var completed = new ManualResetEventSlim())
            {
                TResult result = default;
                Exception exception = null;
                task.GetAwaiter().OnCompleted(() =>
                {
                    try { result = task.GetAwaiter().GetResult(); }
                    catch (Exception error) { exception = error; }
                    finally { completed.Set(); }
                });

                if (!completed.Wait(5000)) throw new TimeoutException("The thread-pool task did not complete.");
                if (exception != null) throw exception;
                return result;
            }
        }

        private static bool IsReady(WaitConditionState state) => state.IsReady;

        private sealed class WaitConditionState
        {
            public bool IsReady;
        }

        private static void Tick() => TickMethod.Invoke(null, null);
        private static Exception Catch(Action action)
        {
            try { action(); return null; }
            catch (Exception error) { return error; }
        }

        private static Exception CaptureOriginalException()
        {
            try { ThrowOriginalException(); return null; }
            catch (Exception error) { return error; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowOriginalException() => throw new InvalidOperationException("original awaiter failure");

        private interface IProbe
        {
            int CleanupCount { get; }
        }

        private sealed class Probe : ETask.TaskAwaiter, IProbe
        {
            public static readonly Stack<Probe> Pool = new Stack<Probe>();
            public int CleanupCount { get; private set; }
            public Guid CurrentToken => Token;
            public Exception StartError;
            public Action StartAction;
            public Action AfterReturn;
            public bool AllowCompletion = true;
            public int TickCount { get; private set; }
            public ETask Begin() { CleanupCount = 0; TickCount = 0; Start(out var token); return new ETask(this, token); }
            protected override void OnTaskStart() { StartAction?.Invoke(); if (StartError != null) throw StartError; }
            protected override bool OnTaskTick() { if (!IsPending) return base.OnTaskTick(); TickCount++; return true; }
            protected override bool IsCompleted => AllowCompletion && base.IsCompleted;
            protected override void OnTaskResult(ETaskStatus status)
            {
                CleanupCount++;
                var afterReturn = AfterReturn;
                AfterReturn = null;
                lock (Pool) Pool.Push(this);
                afterReturn?.Invoke();
            }
            public static Probe Rent() { lock (Pool) return Pool.Count > 0 ? Pool.Pop() : new Probe(); }
        }

        private sealed class GenericProbe<T> : ETask.TaskAwaiter<T>, IProbe
        {
            public int CleanupCount { get; private set; }
            public Guid CurrentToken => Token;
            public ETask<T> Begin() { CleanupCount = 0; Start(out var token); return new ETask<T>(this, token); }
            protected override void OnTaskResult(ETaskStatus status) => CleanupCount++;
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

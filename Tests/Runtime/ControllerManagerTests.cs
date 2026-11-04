using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EasyFramework.Tests
{
    public class ControllerManagerTests
    {
        private IControllerManager _manager;
        private object _previousManager;
        private FieldInfo _instanceField;
        private IDebugger _previousDebugger;
        private readonly List<IController> _controllers = new();
        private readonly Dictionary<IController, Func<IController>> _queries = new();
        private readonly List<Action> _removeEventHandlers = new();

        [SetUp]
        public void Setup()
        {
            _previousDebugger = FDebug.Debugger;
            FDebug.Debugger = new QuietDebugger();
            var type = typeof(F).Assembly.GetType("EasyFramework.ControllerManager");
            _instanceField = typeof(Singleton<>).MakeGenericType(type)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            _previousManager = _instanceField.GetValue(null);
            _manager = (IControllerManager)Activator.CreateInstance(type);
            _instanceField.SetValue(null, _manager);
            CreationController.FailCreate = false;
            CreationController.Creations = 0;
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var removeHandler in _removeEventHandlers) removeHandler();
            _removeEventHandlers.Clear();
            foreach (var controller in _controllers)
                if (controller is IProbe probe) probe.Reset();
            for (int i = _controllers.Count - 1; i >= 0; i--)
            {
                var controller = _controllers[i];
                if (!ReferenceEquals(_queries[controller](), controller)) continue;
                _manager.DestroyAsync(controller).GetAwaiter().GetResult();
            }
            _controllers.Clear();
            _queries.Clear();
            _instanceField.SetValue(null, _previousManager);
            FDebug.Debugger = _previousDebugger;
        }

        [Test]
        public void FirstEnter_ControllerOwnsComponentsWhileManagerOrdersLifecycle()
        {
            var controller = Enter<First>();

            Assert.That(First.Instance, Is.SameAs(controller));
            Assert.That(_manager.GetController<First>(), Is.SameAs(controller));
            Assert.That(controller.Component.Controller, Is.SameAs(controller));
            Assert.That(controller.OwnerRegisteredAtCreate, Is.True);
            Assert.That(controller.EnteredAtEnter, Is.True);
            Assert.That(controller.ActiveAtEnter, Is.True);
            Assert.That(controller.Creates, Is.EqualTo(1));
            Assert.That(controller.Component.Creates, Is.EqualTo(1));
            Assert.That(controller.Adds, Is.EqualTo(1));
            Assert.That(controller.Component.Adds, Is.EqualTo(1));
            Before(controller, "controller.create", "component.create");
            Before(controller, "controller.pre", "component.pre");
            Before(controller, "component.pre", "controller.add");
            Before(controller, "controller.add", "component.add");
            Before(controller, "component.add", "controller.enter");
            Before(controller, "controller.enter", "controller.enterAsync");
            Before(controller, "controller.enterAsync", "component.enter");
            Before(controller, "component.enter", "controller.after");
            Before(controller, "controller.after", "component.after");
        }

        [Test]
        public void Reenter_UsesLatestParametersPromotesInstanceAndDoesNotRebindListeners()
        {
            var controller = Track(_manager.EnterAsync<ParameterController, int>(11).GetAwaiter().GetResult());
            var other = Enter<Second>();

            var reentered = _manager.EnterAsync<ParameterController, int>(23).GetAwaiter().GetResult();

            Assert.That(reentered, Is.SameAs(controller));
            Assert.That(controller.T1, Is.EqualTo(23));
            Assert.That(controller.ValueAtEnter, Is.EqualTo(23));
            Assert.That(controller.Pres, Is.EqualTo(2));
            Assert.That(controller.Enters, Is.EqualTo(2));
            Assert.That(controller.Afters, Is.EqualTo(2));
            Assert.That(controller.Component.Enters, Is.EqualTo(2));
            Assert.That(controller.Adds, Is.EqualTo(1));
            Assert.That(controller.Component.Adds, Is.EqualTo(1));
            Assert.That(controller.Exits, Is.Zero);
            Assert.That(_manager.EnterList, Is.EqualTo(new IController[] { other, controller }));
            Assert.That(_manager.TopController, Is.SameAs(controller));
            Assert.That(controller.Activations, Is.EqualTo(new[] { true, false, true }));
            Assert.That(other.Activations, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void NoArgumentReenter_AppliesParameterDefaultsWithoutAnotherListenerBinding()
        {
            var controller = Track(_manager.EnterAsync<ParameterController, int>(41).GetAwaiter().GetResult());

            _manager.EnterAsync<ParameterController>().GetAwaiter().GetResult();

            Assert.That(controller.T1, Is.EqualTo(-1));
            Assert.That(controller.ValueAtEnter, Is.EqualTo(-1));
            Assert.That(controller.Defaults, Is.EqualTo(1));
            Assert.That(controller.Enters, Is.EqualTo(2));
            Assert.That(controller.Adds, Is.EqualTo(1));
        }

        [Test]
        public void GenericParameters_TwoAndThreeArgumentsReachEnterWithTheirTypesIntact()
        {
            var two = Track(_manager.EnterAsync<TwoParameters, int, string>(5, "room").GetAwaiter().GetResult());
            var three = Track(_manager.EnterAsync<ThreeParameters, int, string, bool>(8, "match", true)
                .GetAwaiter().GetResult());

            Assert.That(two.ValuesAtEnter, Is.EqualTo("5:room"));
            Assert.That(three.ValuesAtEnter, Is.EqualTo("8:match:True"));
            Assert.That(two.Enters, Is.EqualTo(1));
            Assert.That(three.Enters, Is.EqualTo(1));
        }

        [Test]
        public void InstanceEnter_ForwardsZeroToThreeParametersToTheSameManagedInstances()
        {
            var one = Track(ParameterController.Instance);
            var firstEntry = one.EnterAsync(13).GetAwaiter().GetResult();
            var refreshed = one.EnterAsync(29).GetAwaiter().GetResult();
            var two = Track(TwoParameters.Instance);
            var twoEntry = two.EnterAsync(5, "room").GetAwaiter().GetResult();
            var three = Track(ThreeParameters.Instance);
            var threeEntry = three.EnterAsync(8, "match", true).GetAwaiter().GetResult();

            Assert.That(firstEntry, Is.SameAs(one));
            Assert.That(refreshed, Is.SameAs(one));
            Assert.That(_manager.GetController<ParameterController>(), Is.SameAs(one));
            Assert.That(one.ValueAtEnter, Is.EqualTo(29));
            Assert.That(one.Enters, Is.EqualTo(2));
            Assert.That(one.Adds, Is.EqualTo(1));
            Assert.That(one.Component.Adds, Is.EqualTo(1));
            Assert.That(twoEntry, Is.SameAs(two));
            Assert.That(two.ValuesAtEnter, Is.EqualTo("5:room"));
            Assert.That(threeEntry, Is.SameAs(three));
            Assert.That(three.ValuesAtEnter, Is.EqualTo("8:match:True"));

            Assert.That(one.EnterAsync().GetAwaiter().GetResult(), Is.SameAs(one));
            Assert.That(one.ValueAtEnter, Is.EqualTo(-1));
            Assert.That(one.Defaults, Is.EqualTo(1));
            Assert.That(one.Adds, Is.EqualTo(1));
            one.ExitAsync().GetAwaiter().GetResult();
            Assert.That(one.Removes, Is.EqualTo(1));
            Assert.That(one.Component.Removes, Is.EqualTo(1));
        }

        [Test]
        public void InstanceEnter_RejectsUnsupportedParameterTypesAndArityWithoutChangingTheEntry()
        {
            var controller = Track(ParameterController.Instance);
            controller.EnterAsync(11).GetAwaiter().GetResult();

            Assert.That(Capture(() => controller.EnterAsync("invalid").GetAwaiter().GetResult()),
                Is.InstanceOf<ArgumentException>());
            Assert.That(Capture(() => controller.EnterAsync(7, "invalid").GetAwaiter().GetResult()),
                Is.InstanceOf<ArgumentException>());
            Assert.That(Capture(() => controller.EnterAsync(7, "invalid", true).GetAwaiter().GetResult()),
                Is.InstanceOf<ArgumentException>());
            Assert.That(controller.T1, Is.EqualTo(11));
            Assert.That(controller.ParameterSets, Is.EqualTo(1));
            Assert.That(controller.Enters, Is.EqualTo(1));
            Assert.That(controller.IsEnter, Is.True);

            controller.EnterAsync(23).GetAwaiter().GetResult();
            Assert.That(controller.ValueAtEnter, Is.EqualTo(23));
            Assert.That(controller.Enters, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator InstanceEnter_RejectsPendingRefreshWithoutOverwritingParametersOrDefaults()
        {
            var controller = Track(ParameterController.Instance);
            controller.EnterGate = new TaskCompletionSource<bool>();
            var entering = controller.EnterAsync(17);
            var suppliedError = Capture(() => controller.EnterAsync(99).GetAwaiter().GetResult());
            var defaultError = Capture(() => controller.EnterAsync().GetAwaiter().GetResult());
            int valueWhileEntering = controller.T1;
            int defaultsWhileEntering = controller.Defaults;
            controller.EnterGate.SetResult(true);
            yield return Wait(() => entering.GetAwaiter().IsCompleted);
            Assert.That(entering.GetAwaiter().GetResult(), Is.SameAs(controller));

            Assert.That(suppliedError, Is.TypeOf<InvalidOperationException>());
            Assert.That(defaultError, Is.TypeOf<InvalidOperationException>());
            Assert.That(valueWhileEntering, Is.EqualTo(17));
            Assert.That(defaultsWhileEntering, Is.Zero);
            Assert.That(controller.T1, Is.EqualTo(17));
            Assert.That(controller.ParameterSets, Is.EqualTo(1));
            Assert.That(controller.Enters, Is.EqualTo(1));
        }

        [Test]
        public void PublicControllerContract_DoesNotExposeDestroyOrListenerDispatch()
        {
            foreach (var method in typeof(IControllerManager).GetMethods())
                Assert.That(method.Name, Is.Not.EqualTo("DestroyAsync"));
            foreach (var method in _manager.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
                Assert.That(method.Name, Is.Not.EqualTo("DestroyAsync"));
            Assert.That(typeof(Controller<First>).GetMethod("DestroyAsync",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), Is.Null);
            Assert.That(typeof(IController).GetMethod("OnAddListeners"), Is.Null);
            Assert.That(typeof(IController).GetMethod("OnRemoveListeners"), Is.Null);
            Assert.That(typeof(IController).GetMethod("OnAfterEnterAsync"), Is.Null);
            Assert.That(typeof(IController).GetMethod("OnEntered"), Is.Null);
            Assert.That(typeof(IController).GetMethod("OnExited"), Is.Null);
        }

        [Test]
        public void ExitOfUncreatedType_DoesNotCreateAnInstance()
        {
            _manager.ExitAsync<CreationController>().GetAwaiter().GetResult();
            _manager.ExitAsync<CreationController>().GetAwaiter().GetResult();

            Assert.That(CreationController.Creations, Is.Zero);
            Assert.That(_manager.GetController<CreationController>(), Is.Null);
        }

        [Test]
        public void RepeatedSettledExit_UnbindsAndRunsExitOnce()
        {
            var controller = Enter<First>();

            _manager.ExitAsync<First>().GetAwaiter().GetResult();
            _manager.ExitAsync(controller).GetAwaiter().GetResult();

            Assert.That(controller.IsEnter, Is.False);
            Assert.That(controller.Removes, Is.EqualTo(1));
            Assert.That(controller.Component.Removes, Is.EqualTo(1));
            Assert.That(controller.Exits, Is.EqualTo(1));
            Assert.That(controller.Component.Exits, Is.EqualTo(1));
            Assert.That(_manager.GetController<First>(), Is.SameAs(controller));
        }

        [UnityTest]
        public IEnumerator Entering_RejectsSameInstanceOperationsWithoutChangingParameters()
        {
            var controller = Track(ParameterController.Instance);
            controller.EnterGate = new TaskCompletionSource<bool>();
            var entering = _manager.EnterAsync<ParameterController, int>(17);
            bool enteredBeforeCompletion = controller.IsEnter;
            var errors = new[]
            {
                Capture(() => _manager.EnterAsync<ParameterController, int>(99).GetAwaiter().GetResult()),
                Capture(() => _manager.EnterAsync<ParameterController>().GetAwaiter().GetResult()),
                Capture(() => _manager.ExitAsync<ParameterController>().GetAwaiter().GetResult()),
                Capture(() => _manager.DestroyAsync<ParameterController>().GetAwaiter().GetResult())
            };
            int valueWhileEntering = controller.T1;
            controller.EnterGate.SetResult(true);
            yield return Wait(() => entering.GetAwaiter().IsCompleted);
            entering.GetAwaiter().GetResult();

            Assert.That(enteredBeforeCompletion, Is.True, "The controller enters the stack before its Enter callback awaits.");
            foreach (var error in errors) Assert.That(error, Is.TypeOf<InvalidOperationException>());
            Assert.That(valueWhileEntering, Is.EqualTo(17));
            Assert.That(controller.T1, Is.EqualTo(17));
            Assert.That(controller.ParameterSets, Is.EqualTo(1));
            Assert.That(controller.Enters, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PreEnterWait_HasNotChangedStackYetAndRejectsExit()
        {
            var controller = Track(First.Instance);
            controller.PreGate = new TaskCompletionSource<bool>();
            var entering = _manager.EnterAsync<First>();
            bool enteredDuringPreparation = controller.IsEnter;
            var error = Capture(() => _manager.ExitAsync<First>().GetAwaiter().GetResult());
            controller.PreGate.SetResult(true);
            yield return Wait(() => entering.GetAwaiter().IsCompleted);
            entering.GetAwaiter().GetResult();

            Assert.That(enteredDuringPreparation, Is.False);
            Assert.That(error, Is.TypeOf<InvalidOperationException>());
            Assert.That(controller.IsEnter, Is.True);
        }

        [UnityTest]
        public IEnumerator Exiting_RejectsEnterExitAndDestroyUntilCleanupCompletes()
        {
            var controller = Enter<First>();
            controller.ExitGate = new TaskCompletionSource<bool>();
            var exiting = _manager.ExitAsync<First>();
            bool enteredDuringCleanup = controller.IsEnter;
            var errors = new[]
            {
                Capture(() => _manager.EnterAsync<First>().GetAwaiter().GetResult()),
                Capture(() => _manager.ExitAsync<First>().GetAwaiter().GetResult()),
                Capture(() => _manager.DestroyAsync<First>().GetAwaiter().GetResult())
            };
            controller.ExitGate.SetResult(true);
            yield return Wait(() => exiting.GetAwaiter().IsCompleted);
            exiting.GetAwaiter().GetResult();

            Assert.That(enteredDuringCleanup, Is.False);
            foreach (var error in errors) Assert.That(error, Is.TypeOf<InvalidOperationException>());
            Assert.That(controller.Removes, Is.EqualTo(1));
            Assert.That(controller.Exits, Is.EqualTo(1));
            _manager.EnterAsync<First>().GetAwaiter().GetResult();
            Assert.That(controller.IsEnter, Is.True);
            Assert.That(controller.Adds, Is.EqualTo(2));
        }

        [Test]
        public void ParameterSetter_RunsInsideTheEnterOperationGuard()
        {
            var controller = Track(ParameterController.Instance);
            Exception observed = null;
            controller.ParametersAction = () =>
                observed = Capture(() => _manager.EnterAsync<ParameterController, int>(100).GetAwaiter().GetResult());

            _manager.EnterAsync<ParameterController, int>(7).GetAwaiter().GetResult();

            Assert.That(observed, Is.TypeOf<InvalidOperationException>());
            Assert.That(controller.ParameterSets, Is.EqualTo(1));
            Assert.That(controller.T1, Is.EqualTo(7));
        }

        [Test]
        public void ParameterSetterFailure_PropagatesAndReleasesTheOperationGuard()
        {
            var controller = Track(ParameterController.Instance);
            var failure = new InvalidOperationException("parameters failed");
            controller.ParametersAction = () => throw failure;

            Assert.That(Capture(() => _manager.EnterAsync<ParameterController, int>(4).GetAwaiter().GetResult()),
                Is.SameAs(failure));
            Assert.That(controller.IsEnter, Is.False);
            controller.ParametersAction = null;
            _manager.EnterAsync<ParameterController, int>(6).GetAwaiter().GetResult();
            Assert.That(controller.ValueAtEnter, Is.EqualTo(6));
        }

        [Test]
        public void FirstPreEnterFailure_LeavesTheControllerOutOfTheStackAndCanRetry()
        {
            var controller = Track(First.Instance);
            var failure = new InvalidOperationException("prepare failed");
            controller.PreAction = () => throw failure;

            Assert.That(Capture(() => _manager.EnterAsync<First>().GetAwaiter().GetResult()), Is.SameAs(failure));
            Assert.That(controller.IsEnter, Is.False);
            Assert.That(controller.Adds, Is.Zero);
            controller.PreAction = null;
            _manager.EnterAsync<First>().GetAwaiter().GetResult();
            Assert.That(controller.IsEnter, Is.True);
            Assert.That(controller.Creates, Is.EqualTo(1));
        }

        [Test]
        public void ReenterPreFailure_KeepsOldStackPositionButDoesNotRollBackNewParameters()
        {
            var controller = Track(_manager.EnterAsync<ParameterController, int>(1).GetAwaiter().GetResult());
            var other = Enter<Second>();
            var failure = new InvalidOperationException("refresh preparation failed");
            controller.PreAction = () => throw failure;

            Assert.That(Capture(() => _manager.EnterAsync<ParameterController, int>(2).GetAwaiter().GetResult()),
                Is.SameAs(failure));
            Assert.That(_manager.EnterList, Is.EqualTo(new IController[] { controller, other }));
            Assert.That(controller.T1, Is.EqualTo(2));
            Assert.That(controller.Enters, Is.EqualTo(1));
            Assert.That(controller.Adds, Is.EqualTo(1));
        }

        [Test]
        public void EnterFailureAfterCommit_PreservesMembershipAndDoesNotCompensateWithExit()
        {
            var controller = Track(First.Instance);
            var failure = new InvalidOperationException("enter failed");
            controller.EnterAction = () => throw failure;

            Assert.That(Capture(() => _manager.EnterAsync<First>().GetAwaiter().GetResult()), Is.SameAs(failure));
            Assert.That(controller.IsEnter, Is.True);
            Assert.That(controller.IsActive, Is.True);
            Assert.That(controller.Removes, Is.Zero);
            Assert.That(controller.Exits, Is.Zero);
            Assert.That(controller.Afters, Is.Zero);
            controller.EnterAction = null;
            _manager.EnterAsync<First>().GetAwaiter().GetResult();
            Assert.That(controller.Enters, Is.EqualTo(2));
            Assert.That(controller.Adds, Is.EqualTo(1));
        }

        [Test]
        public void ExitFailure_LeavesMembershipRemovedStopsLaterCallbacksAndAllowsAnotherEnter()
        {
            var controller = Enter<First>();
            var failure = new InvalidOperationException("exit failed");
            controller.ExitAction = () => throw failure;
            int completions = 0;
            controller.OnExitEvent += () => completions++;

            Assert.That(Capture(() => controller.ExitAsync().GetAwaiter().GetResult()), Is.SameAs(failure));
            Assert.That(controller.IsEnter, Is.False);
            Assert.That(controller.Exits, Is.EqualTo(1));
            Assert.That(controller.Component.Exits, Is.Zero);
            Assert.That(completions, Is.Zero);
            controller.ExitAction = null;
            _manager.EnterAsync<First>().GetAwaiter().GetResult();
            controller.ExitAsync().GetAwaiter().GetResult();
            Assert.That(controller.Adds, Is.EqualTo(2));
            Assert.That(completions, Is.EqualTo(1));
        }

        [Test]
        public void EnterCompletionEvent_CanExitTheSameControllerAfterItsGuardIsReleased()
        {
            var controller = Track(First.Instance);
            Action exit = () =>
            {
                controller.Events.Add("controller.enterEvent");
                controller.ExitAsync().GetAwaiter().GetResult();
            };
            controller.OnEnterEvent += exit;
            _removeEventHandlers.Add(() => controller.OnEnterEvent -= exit);

            var result = controller.EnterAsync().GetAwaiter().GetResult();

            Assert.That(result, Is.SameAs(controller));
            Assert.That(controller.Afters, Is.EqualTo(1));
            Before(controller, "component.after", "controller.enterEvent");
            Assert.That(controller.IsEnter, Is.False);
            Assert.That(controller.Exits, Is.EqualTo(1));
        }

        [Test]
        public void ExitCompletionEvent_CanReenterTheSameControllerWithNewParameters()
        {
            var controller = Track(_manager.EnterAsync<ParameterController, int>(3).GetAwaiter().GetResult());
            Action reenter = () =>
            {
                controller.Events.Add("controller.exitEvent");
                controller.EnterAsync(9).GetAwaiter().GetResult();
            };
            controller.OnExitEvent += reenter;
            _removeEventHandlers.Add(() => controller.OnExitEvent -= reenter);

            controller.ExitAsync().GetAwaiter().GetResult();

            Before(controller, "component.exit", "controller.exitEvent");
            Assert.That(controller.IsEnter, Is.True);
            Assert.That(controller.T1, Is.EqualTo(9));
            Assert.That(controller.Enters, Is.EqualTo(2));
            Assert.That(controller.Adds, Is.EqualTo(2));
        }

        [Test]
        public void AfterEnterFailure_StopsTheInternalChainAndReleasesTheManagerGuard()
        {
            var controller = Track(First.Instance);
            var failure = new InvalidOperationException("after enter failed");
            int completed = 0;
            Action handler = () => completed++;
            controller.OnEnterEvent += handler;
            _removeEventHandlers.Add(() => controller.OnEnterEvent -= handler);
            controller.AfterAction = () => throw failure;

            Assert.That(Capture(() => controller.EnterAsync().GetAwaiter().GetResult()), Is.SameAs(failure));
            Assert.That(controller.IsEnter, Is.True);
            Assert.That(controller.Events, Has.No.Member("component.after"));
            Assert.That(completed, Is.Zero);
            controller.AfterAction = null;

            controller.EnterAsync().GetAwaiter().GetResult();

            Assert.That(completed, Is.EqualTo(1));
            Assert.That(controller.Adds, Is.EqualTo(1));
        }

        [Test]
        public void InstanceCompletionEventErrors_PropagateAfterManagerCompletionWithoutRollback()
        {
            var controller = Track(First.Instance);
            var enterError = new InvalidOperationException("enter event failed");
            var exitError = new InvalidOperationException("exit event failed");
            Action entered = () => throw enterError;
            Action exited = () => throw exitError;
            controller.OnEnterEvent += entered;
            _removeEventHandlers.Add(() => controller.OnEnterEvent -= entered);

            Assert.That(Capture(() => controller.EnterAsync().GetAwaiter().GetResult()), Is.SameAs(enterError));
            Assert.That(controller.IsEnter, Is.True);
            controller.OnEnterEvent -= entered;
            controller.OnExitEvent += exited;
            _removeEventHandlers.Add(() => controller.OnExitEvent -= exited);

            Assert.That(Capture(() => controller.ExitAsync().GetAwaiter().GetResult()), Is.SameAs(exitError));
            Assert.That(controller.IsEnter, Is.False);
            controller.OnExitEvent -= exited;

            controller.EnterAsync().GetAwaiter().GetResult();
            controller.ExitAsync().GetAwaiter().GetResult();

            Assert.That(controller.Enters, Is.EqualTo(2));
            Assert.That(controller.Exits, Is.EqualTo(2));
        }

        [Test]
        public void ManagerLifecycleCallsAndBulkExit_DoNotSendControllerInstanceEvents()
        {
            var controller = Track(ParameterController.Instance);
            int enters = 0, exits = 0;
            Action entered = () => enters++;
            Action exited = () => exits++;
            controller.OnEnterEvent += entered;
            controller.OnExitEvent += exited;
            _removeEventHandlers.Add(() => controller.OnEnterEvent -= entered);
            _removeEventHandlers.Add(() => controller.OnExitEvent -= exited);

            _manager.EnterAsync<ParameterController, int>(3).GetAwaiter().GetResult();
            _manager.ExitAsync<ParameterController>().GetAwaiter().GetResult();
            Assert.That(enters, Is.Zero);
            Assert.That(exits, Is.Zero);

            controller.EnterAsync(9).GetAwaiter().GetResult();
            Assert.That(enters, Is.EqualTo(1));
            _manager.ExitAllAsync().GetAwaiter().GetResult();
            Assert.That(exits, Is.Zero);

            controller.EnterAsync().GetAwaiter().GetResult();
            controller.ExitAsync().GetAwaiter().GetResult();
            controller.ExitAsync().GetAwaiter().GetResult();
            Assert.That(enters, Is.EqualTo(2));
            Assert.That(exits, Is.EqualTo(1), "A settled duplicate exit does not send another completion event.");
        }

        [Test]
        public void MultiParameterInstanceEntries_SendOneEventAfterTheirOwnAfterEnterStages()
        {
            var two = Track(TwoParameters.Instance);
            var three = Track(ThreeParameters.Instance);
            int twoEvents = 0, threeEvents = 0;
            Action twoEntered = () => { twoEvents++; two.Events.Add("instance.event"); };
            Action threeEntered = () => { threeEvents++; three.Events.Add("instance.event"); };
            two.OnEnterEvent += twoEntered;
            three.OnEnterEvent += threeEntered;
            _removeEventHandlers.Add(() => two.OnEnterEvent -= twoEntered);
            _removeEventHandlers.Add(() => three.OnEnterEvent -= threeEntered);

            two.EnterAsync(5, "room").GetAwaiter().GetResult();
            three.EnterAsync(8, "match", true).GetAwaiter().GetResult();

            Assert.That(twoEvents, Is.EqualTo(1));
            Assert.That(threeEvents, Is.EqualTo(1));
            Before(two, "component.after", "instance.event");
            Before(three, "component.after", "instance.event");
        }

        [Test]
        public void UnmanagedInstanceEntry_IsRejectedWithoutEnteringOrSendingEventsOnAnotherInstance()
        {
            var managed = Track(ParameterController.Instance);
            var unowned = new ParameterController();
            int events = 0;
            unowned.OnEnterEvent += () => events++;

            Assert.That(Capture(() => unowned.EnterAsync().GetAwaiter().GetResult()),
                Is.TypeOf<InvalidOperationException>());
            Assert.That(Capture(() => unowned.EnterAsync(9).GetAwaiter().GetResult()),
                Is.TypeOf<InvalidOperationException>());
            Assert.That(_manager.GetController<ParameterController>(), Is.SameAs(managed));
            Assert.That(managed.Enters, Is.Zero);
            Assert.That(events, Is.Zero);
            Assert.That(_manager.EnterList, Is.Empty);
        }

        [Test]
        public void ExitAll_DoesNotExitANewEntryOfTheSameInstance()
        {
            var first = Enter<First>();
            var second = Enter<Second>();
            second.ExitAction = () =>
            {
                _manager.ExitAsync<First>().GetAwaiter().GetResult();
                _manager.EnterAsync<First>().GetAwaiter().GetResult();
            };

            _manager.ExitAllAsync().GetAwaiter().GetResult();

            Assert.That(_manager.EnterList, Is.EqualTo(new IController[] { first }));
            Assert.That(first.Exits, Is.EqualTo(1));
            Assert.That(first.Enters, Is.EqualTo(2));
        }

        [Test]
        public void ExitAll_DoesNotExitARefreshedEntryStillInTheStack()
        {
            var first = Enter<First>();
            var second = Enter<Second>();
            second.ExitAction = () => _manager.EnterAsync<First>().GetAwaiter().GetResult();

            _manager.ExitAllAsync().GetAwaiter().GetResult();

            Assert.That(first.IsEnter, Is.True);
            Assert.That(first.Exits, Is.Zero);
            Assert.That(first.Enters, Is.EqualTo(2));
            Assert.That(first.Adds, Is.EqualTo(1));
        }

        [Test]
        public void ExitAll_ContinuesOtherExitsAndPreservesASingleFailure()
        {
            var first = Enter<First>();
            var second = Enter<Second>();
            var failure = new InvalidOperationException("second exit failed");
            second.ExitAction = () => throw failure;

            Assert.That(Capture(() => _manager.ExitAllAsync().GetAwaiter().GetResult()), Is.SameAs(failure));
            Assert.That(_manager.EnterList, Is.Empty);
            Assert.That(first.Exits, Is.EqualTo(1));
            Assert.That(second.Exits, Is.EqualTo(1));
        }

        [Test]
        public void ExitAll_AggregatesMultipleFailuresAfterTryingEveryCapturedEntry()
        {
            var first = Enter<First>();
            var second = Enter<Second>();
            var firstFailure = new InvalidOperationException("first exit failed");
            var secondFailure = new InvalidOperationException("second exit failed");
            first.ExitAction = () => throw firstFailure;
            second.ExitAction = () => throw secondFailure;

            var observed = Capture(() => _manager.ExitAllAsync().GetAwaiter().GetResult());

            Assert.That(observed, Is.TypeOf<AggregateException>());
            Assert.That(((AggregateException)observed).InnerExceptions,
                Is.EquivalentTo(new Exception[] { firstFailure, secondFailure }));
            Assert.That(_manager.EnterList, Is.Empty);
        }

        [Test]
        public void FrameSnapshot_SkipsComponentsAndLateUpdateAfterTheInstanceRefreshesItsEntry()
        {
            var controller = Enter<First>();
            var tick = (IFrameworkTick)_manager;
            controller.UpdateAction = () =>
            {
                controller.UpdateAction = null;
                _manager.EnterAsync<First>().GetAwaiter().GetResult();
            };

            tick.OnUpdate();
            tick.OnLateUpdate();

            Assert.That(controller.Updates, Is.EqualTo(1));
            Assert.That(controller.Component.Updates, Is.Zero);
            Assert.That(controller.Lates, Is.Zero);
            tick.OnUpdate();
            tick.OnLateUpdate();
            Assert.That(controller.Updates, Is.EqualTo(2));
            Assert.That(controller.Component.Updates, Is.EqualTo(1));
            Assert.That(controller.Lates, Is.EqualTo(1));
        }

        [Test]
        public void FrameSnapshot_RefreshWithAnActivationFailureStopsTheOldComponentCallbacks()
        {
            var controller = Enter<First>();
            var failure = new InvalidOperationException("activation failed");
            Exception observed = null;
            controller.UpdateAction = () =>
            {
                controller.UpdateAction = null;
                var other = Enter<Second>();
                other.ActiveAction = active => { if (!active) throw failure; };
                observed = Capture(() => _manager.EnterAsync<First>().GetAwaiter().GetResult());
            };
            var tick = (IFrameworkTick)_manager;

            tick.OnUpdate();
            tick.OnLateUpdate();

            Assert.That(observed, Is.SameAs(failure));
            Assert.That(_manager.TopController, Is.SameAs(controller));
            Assert.That(controller.Component.Updates, Is.Zero,
                "A refresh that fails after committing must still invalidate old component frame work.");
            Assert.That(controller.Lates, Is.Zero);
        }

        [Test]
        public void Destroy_ExitsAndDestroysTheManagedInstanceBeforeAReplacementIsCreated()
        {
            var controller = Enter<First>();

            _manager.DestroyAsync<First>().GetAwaiter().GetResult();

            Assert.That(controller.IsEnter, Is.False);
            Assert.That(controller.Removes, Is.EqualTo(1));
            Assert.That(controller.Exits, Is.EqualTo(1));
            Assert.That(controller.Destroys, Is.EqualTo(1));
            Assert.That(controller.Component.Destroys, Is.EqualTo(1));
            Assert.That(_manager.GetController<First>(), Is.Null);
            var replacement = Enter<First>();
            Assert.That(replacement, Is.Not.SameAs(controller));
            Assert.That(First.Instance, Is.SameAs(replacement));
        }

        [Test]
        public void ActivationCallback_SwitchingAwayAndBackDoesNotDuplicateTheNewTopNotification()
        {
            var first = Enter<First>();
            first.ActiveAction = active =>
            {
                if (active) return;
                first.ActiveAction = null;
                var temporary = Enter<CreationController>();
                _manager.ExitAsync(temporary).GetAwaiter().GetResult();
            };

            var second = Enter<Second>();

            Assert.That(second.Activations, Is.EqualTo(new[] { false, true }));
            Assert.That(second.Component.Activations, Is.EqualTo(new[] { false, true }));
            Assert.That(first.Component.Activations, Is.EqualTo(new[] { true, false }));
            Assert.That(_manager.TopController, Is.SameAs(second));
        }

        [Test]
        public void ActivationCallback_FailedPreparationDoesNotDropTheInactiveComponentNotification()
        {
            var first = Enter<First>();
            var failure = new InvalidOperationException("preparation failed");
            Exception observed = null;
            first.ActiveAction = active =>
            {
                if (active) return;
                first.ActiveAction = null;
                first.PreAction = () => throw failure;
                observed = Capture(() => _manager.EnterAsync<First>().GetAwaiter().GetResult());
                first.PreAction = null;
            };

            var second = Enter<Second>();

            Assert.That(observed, Is.SameAs(failure));
            Assert.That(first.Component.Activations, Is.EqualTo(new[] { true, false }));
            Assert.That(_manager.HasActive(second), Is.True);
            Assert.That(first.IsEnter, Is.True);
        }

        [Test]
        public void CreationFailure_DoesNotPublishAnUninitializedCachedInstance()
        {
            CreationController.FailCreate = true;
            Assert.That(Capture(() => _manager.EnterAsync<CreationController>().GetAwaiter().GetResult()),
                Is.TypeOf<InvalidOperationException>());
            Assert.That(_manager.GetController<CreationController>(), Is.Null);
            CreationController.FailCreate = false;

            var controller = Enter<CreationController>();

            Assert.That(controller.IsEnter, Is.True);
            Assert.That(CreationController.Creations, Is.EqualTo(2));
        }

        private T Enter<T>() where T : class, IController, new() =>
            Track(_manager.EnterAsync<T>().GetAwaiter().GetResult());

        private T Track<T>(T controller) where T : class, IController
        {
            if (!_controllers.Contains(controller)) _controllers.Add(controller);
            _queries[controller] = () => _manager.GetController<T>();
            return controller;
        }

        private static Exception Capture(Action action)
        {
            try { action(); }
            catch (Exception exception) { return exception; }
            return null;
        }

        private static IEnumerator Wait(Func<bool> predicate)
        {
            for (int frame = 0; frame < 120 && !predicate(); frame++) yield return null;
            Assert.That(predicate(), Is.True, "The lifecycle operation did not complete within 120 frames.");
        }

        private static void Before<T>(ProbeController<T> controller, string earlier, string later)
            where T : ProbeController<T>, new()
        {
            int first = controller.Events.IndexOf(earlier);
            int second = controller.Events.IndexOf(later);
            Assert.That(first, Is.GreaterThanOrEqualTo(0), earlier + " was not dispatched.");
            Assert.That(second, Is.GreaterThan(first), earlier + " must precede " + later);
        }

        public interface IProbe { void Reset(); }

        public abstract class ProbeController<T> : Controller<T>, IProbe where T : ProbeController<T>, new()
        {
            public readonly List<string> Events = new();
            public readonly List<bool> Activations = new();
            public readonly ProbeComponent Component = new();
            public Action PreAction, EnterAction, AfterAction, ExitAction, UpdateAction;
            public Action<bool> ActiveAction;
            public TaskCompletionSource<bool> PreGate, EnterGate, ExitGate;
            public int Creates, Pres, Adds, Enters, Afters, Removes, Exits, Updates, Lates, Destroys;
            public bool OwnerRegisteredAtCreate, EnteredAtEnter, ActiveAtEnter;

            protected override void OnCreate()
            {
                Creates++;
                OwnerRegisteredAtCreate = ReferenceEquals(Component.Controller, this);
                Events.Add("controller.create");
            }
            protected override async ETask OnPreEnterAsync()
            {
                Pres++;
                Events.Add("controller.pre");
                PreAction?.Invoke();
                if (PreGate != null) await PreGate.Task;
            }
            protected override void OnAddListeners() { Adds++; Events.Add("controller.add"); }
            protected override void OnEnter()
            {
                Enters++;
                EnteredAtEnter = IsEnter;
                ActiveAtEnter = IsActive;
                Events.Add("controller.enter");
                EnterAction?.Invoke();
            }
            protected override async ETask OnEnterAsync()
            {
                Events.Add("controller.enterAsync");
                if (EnterGate != null) await EnterGate.Task;
            }
            protected override void OnAfterEnter()
            {
                Afters++;
                Events.Add("controller.after");
                AfterAction?.Invoke();
            }
            protected override void OnRemoveListeners() { Removes++; Events.Add("controller.remove"); }
            protected override void OnExit() { Exits++; Events.Add("controller.exit"); ExitAction?.Invoke(); }
            protected override async ETask OnExitAsync() { if (ExitGate != null) await ExitGate.Task; }
            protected override void OnSetActive(bool active) { Activations.Add(active); ActiveAction?.Invoke(active); }
            protected override void OnUpdate() { Updates++; UpdateAction?.Invoke(); }
            protected override void OnLateUpdate() { Lates++; }
            protected override void OnDestroy() { Destroys++; Events.Add("controller.destroy"); }

            void IProbe.Reset()
            {
                PreAction = EnterAction = AfterAction = ExitAction = UpdateAction = null;
                ActiveAction = null;
                PreGate?.TrySetResult(true);
                EnterGate?.TrySetResult(true);
                ExitGate?.TrySetResult(true);
                PreGate = EnterGate = ExitGate = null;
                if (this is ParameterController parameters) parameters.ParametersAction = null;
            }
        }

        public sealed class First : ProbeController<First> { }
        public sealed class Second : ProbeController<Second> { }

        public sealed class ParameterController : ProbeController<ParameterController>, ITParams<int>
        {
            private int _value;
            public ref readonly int T1 => ref _value;
            public int ParameterSets, Defaults, ValueAtEnter;
            public Action ParametersAction;
            public void SetParams(in int value) { ParameterSets++; _value = value; ParametersAction?.Invoke(); }
            public void SetParamsDefault() { Defaults++; _value = -1; }
            public void SetParams(object[] values) => throw new NotSupportedException();
            protected override void OnEnter() { base.OnEnter(); ValueAtEnter = _value; }
        }

        public sealed class TwoParameters : ProbeController<TwoParameters>, ITParams<int, string>
        {
            private int _number;
            private string _text;
            public ref readonly int T1 => ref _number;
            public ref readonly string T2 => ref _text;
            public string ValuesAtEnter;
            public void SetParams(in int number, in string text) { _number = number; _text = text; }
            public void SetParams(object[] values) => throw new NotSupportedException();
            protected override void OnEnter() { base.OnEnter(); ValuesAtEnter = _number + ":" + _text; }
        }

        public sealed class ThreeParameters : ProbeController<ThreeParameters>, ITParams<int, string, bool>
        {
            private int _number;
            private string _text;
            private bool _enabled;
            public ref readonly int T1 => ref _number;
            public ref readonly string T2 => ref _text;
            public ref readonly bool T3 => ref _enabled;
            public string ValuesAtEnter;
            public void SetParams(in int number, in string text, in bool enabled)
            { _number = number; _text = text; _enabled = enabled; }
            public void SetParams(object[] values) => throw new NotSupportedException();
            protected override void OnEnter() { base.OnEnter(); ValuesAtEnter = _number + ":" + _text + ":" + _enabled; }
        }

        public sealed class CreationController : ProbeController<CreationController>
        {
            public static int Creations;
            public static bool FailCreate;
            protected override void OnCreate()
            {
                Creations++;
                if (FailCreate) throw new InvalidOperationException("creation failed");
                base.OnCreate();
            }
        }

        public sealed class ProbeComponent : ControllerComponent
        {
            public readonly List<bool> Activations = new();
            public int Creates, Adds, Enters, Removes, Exits, Updates, Destroys;
            private List<string> Events
            {
                get
                {
                    if (Controller is First first) return first.Events;
                    if (Controller is Second second) return second.Events;
                    if (Controller is ParameterController parameters) return parameters.Events;
                    if (Controller is TwoParameters two) return two.Events;
                    if (Controller is ThreeParameters three) return three.Events;
                    return ((CreationController)Controller).Events;
                }
            }
            protected override void OnCreate() { Creates++; Events.Add("component.create"); }
            protected override void OnPreEnter() { Events.Add("component.pre"); }
            protected override void OnAddListeners() { Adds++; Events.Add("component.add"); }
            protected override void OnEnter() { Enters++; Events.Add("component.enter"); }
            protected override void OnAfterEnter() { Events.Add("component.after"); }
            protected override void OnRemoveListeners() { Removes++; Events.Add("component.remove"); }
            protected override void OnExit() { Exits++; Events.Add("component.exit"); }
            protected override void OnUpdate() { Updates++; }
            protected override void OnSetActive(bool active) { Activations.Add(active); }
            protected override void OnDestroy() { Destroys++; Events.Add("component.destroy"); }
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EasyFramework.Tests
{
    public class CustomControllerTests
    {
        private IControllerManager _manager;
        private object _previousManager;
        private FieldInfo _instanceField;
        private readonly List<CustomController> _controllers = new();

        [SetUp]
        public void Setup()
        {
            var type = typeof(F).Assembly.GetType("EasyFramework.ControllerManager");
            _instanceField = typeof(Singleton<>).MakeGenericType(type)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            _previousManager = _instanceField.GetValue(null);
            _manager = (IControllerManager)Activator.CreateInstance(type);
            _instanceField.SetValue(null, _manager);
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                foreach (var controller in _controllers)
                    controller.EnterAction = controller.ExitAction = controller.UpdateAction = controller.DestroyAction = null;
                for (int i = _controllers.Count - 1; i >= 0; i--)
                    _manager.DestroyAsync(_controllers[i]).GetAwaiter().GetResult();
            }
            finally
            {
                _controllers.Clear();
                _instanceField.SetValue(null, _previousManager);
            }
        }

        [Test]
        public void DirectInterfaceImplementation_SupportsParametersRefreshExitAndReplacement()
        {
            var first = Track(_manager.EnterAsync<CustomController, int>(7).GetAwaiter().GetResult());
            var refreshed = _manager.EnterAsync<CustomController, int>(19).GetAwaiter().GetResult();
            Assert.That(refreshed, Is.SameAs(first));
            Assert.That(first.ValueAtEnter, Is.EqualTo(19));
            Assert.That(first.Creates, Is.EqualTo(1));
            Assert.That(first.Prepares, Is.EqualTo(2));
            Assert.That(first.Enters, Is.EqualTo(2));
            Assert.That(first.Afters, Is.EqualTo(2));
            Assert.That(first.Adds, Is.EqualTo(1));
            Assert.That(first.EnteredBeforeAwait, Is.True);
            Assert.That(first.ActiveBeforeAwait, Is.True);

            _manager.EnterAsync<CustomController>().GetAwaiter().GetResult();
            Assert.That(first.ValueAtEnter, Is.EqualTo(-1));
            Assert.That(first.Defaults, Is.EqualTo(1));
            _manager.ExitAsync(first).GetAwaiter().GetResult();
            _manager.ExitAsync<CustomController>().GetAwaiter().GetResult();
            Assert.That(first.Removes, Is.EqualTo(1));
            Assert.That(first.Exits, Is.EqualTo(1));
            Assert.That(first.IsEnter, Is.False);
            Assert.That(_manager.GetController<CustomController>(), Is.SameAs(first));

            _manager.DestroyAsync(first).GetAwaiter().GetResult();
            Assert.That(first.Destroys, Is.EqualTo(1));
            Assert.That(_manager.GetController<CustomController>(), Is.Null);
            var replacement = Track(_manager.EnterAsync<CustomController>().GetAwaiter().GetResult());
            Assert.That(replacement, Is.Not.SameAs(first));
        }

        [UnityTest]
        public IEnumerator PendingEnter_RejectsConflictingOperationsWithoutMutatingParameters()
        {
            var controller = Track(_manager.EnterAsync<CustomController, int>(3).GetAwaiter().GetResult());
            controller.EnterGate = new TaskCompletionSource<bool>();
            var pending = _manager.EnterAsync<CustomController, int>(8);
            var errors = new[]
            {
                Capture(() => _manager.EnterAsync<CustomController, int>(99).GetAwaiter().GetResult()),
                Capture(() => _manager.ExitAsync(controller).GetAwaiter().GetResult()),
                Capture(() => _manager.DestroyAsync(controller).GetAwaiter().GetResult())
            };
            int valueWhilePending = controller.T1;
            controller.EnterGate.SetResult(true);
            for (int frame = 0; frame < 120 && !pending.GetAwaiter().IsCompleted; frame++) yield return null;
            Assert.That(pending.GetAwaiter().IsCompleted, Is.True);
            pending.GetAwaiter().GetResult();
            controller.EnterGate = null;

            foreach (var error in errors) Assert.That(error, Is.TypeOf<InvalidOperationException>());
            Assert.That(valueWhilePending, Is.EqualTo(8));
            Assert.That(controller.T1, Is.EqualTo(8));
            Assert.That(controller.Adds, Is.EqualTo(1));
        }

        [Test]
        public void CallbackFailures_PreserveManagerMembershipAndReleaseTheOperationGuard()
        {
            var controller = Track(_manager.EnterAsync<CustomController>().GetAwaiter().GetResult());
            var enterError = new InvalidOperationException("Custom enter failed.");
            controller.EnterAction = () => throw enterError;
            Assert.That(Capture(() => _manager.EnterAsync<CustomController>().GetAwaiter().GetResult()),
                Is.SameAs(enterError));
            Assert.That(controller.IsEnter, Is.True);
            Assert.That(controller.Exits, Is.Zero);
            controller.EnterAction = null;
            _manager.EnterAsync<CustomController>().GetAwaiter().GetResult();

            var exitError = new InvalidOperationException("Custom exit failed.");
            controller.ExitAction = () => throw exitError;
            Assert.That(Capture(() => _manager.ExitAsync(controller).GetAwaiter().GetResult()),
                Is.SameAs(exitError));
            Assert.That(controller.IsEnter, Is.False);
            controller.ExitAction = null;
            _manager.EnterAsync<CustomController, int>(42).GetAwaiter().GetResult();
            Assert.That(controller.ValueAtEnter, Is.EqualTo(42));
            Assert.That(controller.Adds, Is.EqualTo(2));
        }

        [Test]
        public void ExitAll_SkipsTheNewEntryOfADirectInterfaceImplementation()
        {
            var first = Track(_manager.EnterAsync<CustomController>().GetAwaiter().GetResult());
            var second = Track(_manager.EnterAsync<AnotherController>().GetAwaiter().GetResult());
            second.ExitAction = () =>
            {
                _manager.ExitAsync(first).GetAwaiter().GetResult();
                _manager.EnterAsync<CustomController, int>(21).GetAwaiter().GetResult();
            };

            _manager.ExitAllAsync().GetAwaiter().GetResult();

            Assert.That(first.IsEnter, Is.True);
            Assert.That(first.ValueAtEnter, Is.EqualTo(21));
            Assert.That(first.Exits, Is.EqualTo(1));
            Assert.That(_manager.EnterList, Is.EqualTo(new IController[] { first }));
        }

        [Test]
        public void FrameSnapshot_DoesNotDispatchLateUpdateToARefreshedRegistration()
        {
            var controller = Track(_manager.EnterAsync<CustomController>().GetAwaiter().GetResult());
            controller.UpdateAction = () =>
            {
                controller.UpdateAction = null;
                _manager.EnterAsync<CustomController, int>(17).GetAwaiter().GetResult();
            };
            var tick = (IFrameworkTick)_manager;

            tick.OnUpdate();
            tick.OnLateUpdate();

            Assert.That(controller.Updates, Is.EqualTo(1));
            Assert.That(controller.Lates, Is.Zero);
            tick.OnUpdate();
            tick.OnLateUpdate();
            Assert.That(controller.Updates, Is.EqualTo(2));
            Assert.That(controller.Lates, Is.EqualTo(1));
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void FrameEligibilityFailure_DoesNotSkipTheNextController(bool late)
        {
            var first = Track(_manager.EnterAsync<CustomController>().GetAwaiter().GetResult());
            var second = Track(_manager.EnterAsync<AnotherController>().GetAwaiter().GetResult());
            var tick = (IFrameworkTick)_manager;
            tick.OnUpdate();
            second.Updates = 0;
            var failure = new InvalidOperationException("UpdateEnabled failed.");
            first.UpdateEnabledFailure = failure;
            var debugger = new RecordingDebugger();
            var previousDebugger = FDebug.Debugger;
            FDebug.Debugger = debugger;
            try
            {
                var escaped = Capture(() =>
                {
                    if (late) tick.OnLateUpdate();
                    else tick.OnUpdate();
                });

                Assert.That(late ? second.Lates : second.Updates, Is.EqualTo(1),
                    "An eligibility getter failure must not prevent a later Controller from receiving its frame callback.");
                Assert.That(escaped, Is.Null);
                Assert.That(debugger.LastException, Is.SameAs(failure));
            }
            finally
            {
                first.UpdateEnabledFailure = null;
                FDebug.Debugger = previousDebugger;
            }
        }

        [Test]
        public void DestroyCallback_CannotStartAnotherOperationOnTheSameManagedInstance()
        {
            var controller = Track(_manager.EnterAsync<CustomController>().GetAwaiter().GetResult());
            Exception entering = null, exiting = null, destroying = null;
            controller.DestroyAction = () =>
            {
                entering = Capture(() => _manager.EnterAsync<CustomController>().GetAwaiter().GetResult());
                exiting = Capture(() => _manager.ExitAsync(controller).GetAwaiter().GetResult());
                destroying = Capture(() => _manager.DestroyAsync(controller).GetAwaiter().GetResult());
            };

            _manager.DestroyAsync(controller).GetAwaiter().GetResult();

            Assert.That(entering, Is.TypeOf<InvalidOperationException>());
            Assert.That(exiting, Is.TypeOf<InvalidOperationException>());
            Assert.That(destroying, Is.TypeOf<InvalidOperationException>());
            Assert.That(controller.Exits, Is.EqualTo(1));
            Assert.That(controller.Destroys, Is.EqualTo(1));
            Assert.That(_manager.GetController<CustomController>(), Is.Null);
        }

        private T Track<T>(T controller) where T : CustomController
        {
            _controllers.Add(controller);
            return controller;
        }

        private static Exception Capture(Action action)
        {
            try { action(); }
            catch (Exception error) { return error; }
            return null;
        }

        public class CustomController : IController, ITParams<int>
        {
            private int _value;
            private bool _listenersStarted;
            public ref readonly int T1 => ref _value;
            public bool IsEnter => F.ControllerManager.HasEnter(this);
            public bool IsActive => F.ControllerManager.HasActive(this);
            public bool UpdateEnabled => UpdateEnabledFailure == null ? IsEnter && IsActive : throw UpdateEnabledFailure;
            public Exception UpdateEnabledFailure;
            public int Creates, Defaults, Prepares, Adds, Enters, Afters, Removes, Exits, Destroys, Updates, Lates;
            public int ValueAtEnter;
            public bool EnteredBeforeAwait, ActiveBeforeAwait;
            public Action EnterAction, ExitAction, UpdateAction, DestroyAction;
            public TaskCompletionSource<bool> EnterGate;

            public void SetParams(in int value) => _value = value;
            public void SetParamsDefault() { Defaults++; _value = -1; }
            public void SetParams(object[] values) => throw new NotSupportedException();
            public void OnCreate() { Creates++; }
            public ETask OnPreEnterAsync() { Prepares++; return ETask.CompletedTask; }
            public async ETask OnEnterAsync()
            {
                if (!_listenersStarted)
                {
                    _listenersStarted = true;
                    Adds++;
                }
                Enters++;
                ValueAtEnter = _value;
                EnteredBeforeAwait = IsEnter;
                ActiveBeforeAwait = IsActive;
                EnterAction?.Invoke();
                if (EnterGate != null) await EnterGate.Task;
                Afters++;
            }
            public ETask OnExitAsync()
            {
                if (_listenersStarted)
                {
                    _listenersStarted = false;
                    Removes++;
                }
                Exits++;
                ExitAction?.Invoke();
                return ETask.CompletedTask;
            }
            public void OnSetActive(bool active) { }
            public void OnUpdate() { Updates++; UpdateAction?.Invoke(); }
            public void OnLateUpdate() { Lates++; }
            public void OnDestroy() { Destroys++; DestroyAction?.Invoke(); }
        }

        public sealed class AnotherController : CustomController { }

        private sealed class RecordingDebugger : IDebugger
        {
            public Exception LastException;
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
            public void LogException(Exception exception) { LastException = exception; }
        }
    }
}

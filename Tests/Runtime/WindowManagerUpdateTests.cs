using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace EasyFramework.Tests
{
    public class WindowManagerUpdateTests
    {
        private readonly List<TestWindow> _windows = new();
        private readonly List<GameObject> _objects = new();
        private IWindowManager _manager;
        private IFrameworkTick _tick;
        private FBehaviour _host;
        private FieldInfo _updateList;
        private FieldInfo _closedList;
        private FieldInfo _needRefresh;
        private IDebugger _debugger;
        private TickProbe _hostProbe;

        [SetUp]
        public void Setup()
        {
            _debugger = FDebug.Debugger;
            FDebug.Debugger = new QuietDebugger();
            _manager = F.WindowManager;
            _tick = (IFrameworkTick)_manager;
            _host = F.Behaviour;
            var type = _manager.GetType();
            _updateList = type.GetField("_updateList", BindingFlags.Instance | BindingFlags.NonPublic);
            _closedList = type.GetField("_closedList", BindingFlags.Instance | BindingFlags.NonPublic);
            _needRefresh = type.GetField("_needRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
            // The coroutine yields once before stepping, allowing the host to apply this removal.
            _host.Remove(_tick);
        }

        [TearDown]
        public void Cleanup()
        {
            if (_hostProbe != null)
            {
                _host.Remove(_hostProbe);
                _hostProbe = null;
            }
            foreach (var window in _windows)
            {
                window.UpdateAction = null;
                window.DestroyAction = null;
            }
            foreach (var window in _windows)
            {
                if (ReferenceEquals(_manager.GetWindow(window.GetType()), window)) _manager.Destroy(window);
            }
            foreach (var obj in _objects) if (obj != null) Object.Destroy(obj);
            _windows.Clear();
            _objects.Clear();
            _host.Add(_tick);
            FDebug.Debugger = _debugger;
        }

        [UnityTest]
        public IEnumerator OpenWindow_ReceivesOneUpdatePerManagerTick()
        {
            yield return null;
            var window = Open<WindowA>();

            Tick();

            Assert.That(window.Updates, Is.EqualTo(1), "An open window must run once per manager tick.");
        }

        [UnityTest]
        public IEnumerator LoadingWindow_IsNotUpdatedBeforeItsObjectIsReady()
        {
            yield return null;
            var opening = _manager.OpenAsync<LoadingWindow>();
            var window = _manager.GetWindow<LoadingWindow>();
            _windows.Add(window);
            Tick();
            int updatesWhileLoading = window.Updates;
            var obj = new GameObject("PendingUpdateWindow", typeof(RectTransform));
            _objects.Add(obj);
            window.Gate.SetResult(obj);
            for (int frame = 0; frame < 120 && !opening.GetAwaiter().IsCompleted; frame++) yield return null;
            Assert.That(opening.GetAwaiter().IsCompleted, Is.True);
            Assert.That(opening.GetAwaiter().GetResult(), Is.SameAs(window));

            Assert.That(updatesWhileLoading, Is.Zero,
                "Registration for an in-progress open must not dispatch a window's OnUpdate.");
        }

        [UnityTest]
        public IEnumerator LoadingWindow_IsNotRefreshedBeforeItsObjectIsReady()
        {
            yield return null;
            var opening = _manager.OpenAsync<LoadingWindow>();
            var window = _manager.GetWindow<LoadingWindow>();
            _windows.Add(window);
            Tick();
            _manager.RefreshAllWindow();
            int refreshesWhileLoading = window.Refreshes;
            var obj = new GameObject("PendingRefreshWindow", typeof(RectTransform));
            _objects.Add(obj);
            window.Gate.SetResult(obj);
            for (int frame = 0; frame < 120 && !opening.GetAwaiter().IsCompleted; frame++) yield return null;
            Assert.That(opening.GetAwaiter().IsCompleted, Is.True);
            Assert.That(opening.GetAwaiter().GetResult(), Is.SameAs(window));

            Assert.That(refreshesWhileLoading, Is.Zero,
                "RefreshAllWindow must not run a window's OnRefresh before its object has been initialized.");
        }

        [UnityTest]
        public IEnumerator TimeoutDestroyCallback_ReopenedTypeSurvivesTheOldClosedSnapshot()
        {
            yield return null;
            var a = Open<WindowA>();
            var b = Open<WindowB>();
            _manager.Close(a);
            _manager.Close(b);
            Tick();
            var first = ClosedSnapshot.IndexOf(a) > ClosedSnapshot.IndexOf(b) ? (TestWindow)a : b;
            var second = ReferenceEquals(first, a) ? (TestWindow)b : a;
            a.KeepAliveSeconds = b.KeepAliveSeconds = -1;
            TestWindow reopened = null;
            first.DestroyAction = () =>
            {
                _manager.Destroy(second);
                reopened = second is WindowA ? Open<WindowA>() : Open<WindowB>();
            };

            Tick();
            var current = reopened == null ? null : _manager.GetWindow(reopened.GetType());
            if (reopened != null && !ReferenceEquals(current, reopened)) _manager.Close(reopened);

            Assert.That(reopened, Is.Not.Null);
            Assert.That(current, Is.SameAs(reopened),
                "The old closed snapshot must not remove the replacement instance registered by a destroy callback.");
            Assert.That(second.Destroys, Is.EqualTo(1));
            Assert.That(reopened.IsOpen, Is.True);
        }

        [UnityTest]
        public IEnumerator ExplicitDestroyOfCachedClosedWindow_DoesNotDestroyAgainOnTimeout()
        {
            yield return null;
            var window = Open<WindowA>();
            _manager.Close(window);
            Tick();
            Assert.That(ClosedSnapshot, Has.Member(window));
            _manager.Destroy(window);
            Assert.That(window.Destroys, Is.EqualTo(1));
            Assert.That(_manager.GetWindow<WindowA>(), Is.Null);
            window.KeepAliveSeconds = -1;

            Tick();

            Assert.That(window.Destroys, Is.EqualTo(1),
                "A cached closed reference must not trigger another manager-owned destroy after explicit destruction.");
        }

        [UnityTest]
        public IEnumerator TimeoutDestroyCallback_CanDestroyAnotherClosedWindowOnce()
        {
            yield return null;
            var a = Open<WindowA>();
            var b = Open<WindowB>();
            Tick();
            _manager.Close(a);
            _manager.Close(b);
            Tick();
            var first = ClosedSnapshot.IndexOf(a) > ClosedSnapshot.IndexOf(b) ? (TestWindow)a : b;
            var second = ReferenceEquals(first, a) ? (TestWindow)b : a;
            a.KeepAliveSeconds = b.KeepAliveSeconds = -1;
            first.DestroyAction = () => _manager.Destroy(second);

            Assert.DoesNotThrow(Tick);
            Assert.That(a.Destroys, Is.EqualTo(1));
            Assert.That(b.Destroys, Is.EqualTo(1));
            Assert.That(_manager.GetWindow<WindowA>(), Is.Null);
            Assert.That(_manager.GetWindow<WindowB>(), Is.Null);
            Tick();
            Assert.That(ClosedSnapshot, Has.No.Member(a));
            Assert.That(ClosedSnapshot, Has.No.Member(b));
            Assert.That(a.Destroys, Is.EqualTo(1));
            Assert.That(b.Destroys, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator UpdateDestroyCallback_SkipsRemovedWindowUntilNextSnapshot()
        {
            yield return null;
            var a = Open<WindowA>();
            var b = Open<WindowB>();
            Tick();
            var first = Snapshot.IndexOf(a) < Snapshot.IndexOf(b) ? (TestWindow)a : b;
            var second = ReferenceEquals(first, a) ? (TestWindow)b : a;
            a.Updates = b.Updates = 0;
            first.UpdateAction = () =>
            {
                first.UpdateAction = null;
                _manager.Destroy(second);
            };

            Tick();
            Assert.That(second.Updates, Is.Zero);
            Assert.That(first.Updates, Is.EqualTo(1));
            Assert.That(second.Destroys, Is.EqualTo(1));
            Assert.That(Snapshot, Has.Member(second));
            Assert.That(NeedsRefresh, Is.True);

            first.UpdateAction = null;
            Tick();
            Assert.That(Snapshot, Has.No.Member(second));
            Assert.That(second.Updates, Is.Zero);
            Assert.That(second.Destroys, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ClosedWindow_RemainsTrackedCanReopenAndExpiresAfterClosingAgain()
        {
            yield return null;
            var window = Open<WindowA>();
            _manager.Close(window);
            _manager.Close(window);
            Assert.That(_manager.GetTopWindow(UILayer.HUD), Is.Not.SameAs(window));
            Assert.That(_manager.GetWindows(UILayer.HUD), Has.No.Member(window));
            Assert.That(_manager.GetWindow<WindowA>(), Is.SameAs(window));

            Tick();
            Assert.That(Snapshot, Has.No.Member(window));
            Assert.That(ClosedSnapshot, Has.Member(window));
            Assert.That(window.Updates, Is.Zero);
            Assert.That(window.Destroys, Is.Zero);
            window.KeepAliveSeconds = -1;
            Assert.That(_manager.Open<WindowA>(), Is.SameAs(window));
            Tick();
            Assert.That(window.Updates, Is.EqualTo(1));
            Assert.That(window.Destroys, Is.Zero);

            _manager.Close(window);
            Tick();
            Assert.That(window.Destroys, Is.EqualTo(1));
            Assert.That(_manager.GetWindow<WindowA>(), Is.Null);
            Tick();
            Assert.That(Snapshot, Has.No.Member(window));
            Assert.That(window.Destroys, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DestroyCallback_NewWindowStartsUpdatingOnNextFrame()
        {
            yield return null;
            var a = Open<WindowA>();
            a.KeepAliveSeconds = -1;
            _manager.Close(a);
            WindowC created = null;
            a.DestroyAction = () => created = Open<WindowC>();

            Tick();
            Assert.That(created, Is.Not.Null);
            Assert.That(created.Updates, Is.Zero);
            Assert.That(Snapshot, Has.No.Member(created));
            Assert.That(NeedsRefresh, Is.True);

            Tick();
            Assert.That(created.Updates, Is.EqualTo(1));
            Assert.That(Snapshot, Has.Member(created));
            Assert.That(Snapshot, Has.No.Member(a));
            Assert.That(a.Destroys, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FrameworkHost_DispatchesRegisteredManagerAndStopsAfterRemoval()
        {
            yield return null;
            _host.Add(_tick);
            var window = Open<WindowA>();
            yield return null;
            yield return null;
            Assert.That(window.Updates, Is.GreaterThan(0));

            _host.Remove(_tick);
            int stoppedUpdates = window.Updates;
            yield return null;
            yield return null;
            Assert.That(window.Updates, Is.EqualTo(stoppedUpdates));
        }

        [UnityTest]
        public IEnumerator FrameworkHost_WindowUpdateFailureDoesNotSkipLaterSubscriber()
        {
            yield return null;
            _host.Add(_tick);
            _hostProbe = new TickProbe();
            _host.Add(_hostProbe);
            var window = Open<WindowA>();
            var failure = new InvalidOperationException("window update failed");
            window.UpdateAction = () => throw failure;

            yield return null;
            yield return null;
            Assert.That(window.Updates, Is.GreaterThan(0));
            Assert.That(_hostProbe.Updates, Is.GreaterThan(0));
            Assert.That(((QuietDebugger)FDebug.Debugger).LastException, Is.SameAs(failure));
        }

        private T Open<T>() where T : TestWindow, new()
        {
            var window = _manager.Open<T>();
            if (!_windows.Contains(window))
            {
                _windows.Add(window);
                _objects.Add(window.WindowObject);
            }
            return window;
        }

        private IList<IWindow> Snapshot => (IList<IWindow>)_updateList.GetValue(_manager);
        private IList<IWindow> ClosedSnapshot => (IList<IWindow>)_closedList.GetValue(_manager);
        private bool NeedsRefresh => (bool)_needRefresh.GetValue(_manager);
        private void Tick() => _tick.OnUpdate();

        public abstract class TestWindow : Window
        {
            public float KeepAliveSeconds = 60;
            public int Updates, Destroys;
            public Action UpdateAction, DestroyAction;
            protected override float KeepAliveTime => KeepAliveSeconds;
            protected override GameObject CreateWindowObject(Transform parent) => new GameObject("UpdateListWindow", typeof(RectTransform));
            protected override void OnUpdate() { Updates++; UpdateAction?.Invoke(); }
            protected override void OnDestroy() { Destroys++; DestroyAction?.Invoke(); }
        }
        public sealed class WindowA : TestWindow { }
        public sealed class WindowB : TestWindow { }
        public sealed class WindowC : TestWindow { }
        public sealed class LoadingWindow : TestWindow
        {
            public readonly TaskCompletionSource<GameObject> Gate = new();
            public int Refreshes;
            protected override void OnRefresh() { Refreshes++; }
            protected override async ETask<GameObject> CreateWindowObjectAsync(Transform parent)
            {
                var obj = await Gate.Task;
                obj.transform.SetParent(parent, false);
                return obj;
            }
        }
        private sealed class TickProbe : IFrameworkTick
        {
            public int Updates;
            public void OnUpdate() { Updates++; }
            public void OnLateUpdate() { }
            public void OnDestroy() { }
        }
        private sealed class QuietDebugger : IDebugger
        {
            public Exception LastException;
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
            public void LogException(Exception exception) { LastException = exception; }
        }
    }
}

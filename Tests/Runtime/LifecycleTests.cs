using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace EasyFramework.Tests
{
    public class LifecycleTests
    {
        private readonly List<GameObject> _objects = new();
        private UILayerBehaviour _layer;
        private object _oldManager;
        private FieldInfo _managerField;
        private IDebugger _debugger;

        [SetUp]
        public void Setup()
        {
            _debugger = FDebug.Debugger;
            FDebug.Debugger = new QuietDebugger();
            var type = typeof(F).Assembly.GetType("EasyFramework.ControllerManager");
            _managerField = typeof(Singleton<>).MakeGenericType(type).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            _oldManager = _managerField.GetValue(null);
            _managerField.SetValue(null, Activator.CreateInstance(type));
            var root = Track(new GameObject("TestLayer", typeof(RectTransform), typeof(Canvas)));
            _layer = root.AddComponent<UILayerBehaviour>();
        }

        [TearDown]
        public void Cleanup()
        {
            var first = F.ControllerManager.GetController<First>();
            var second = F.ControllerManager.GetController<Second>();
            if (first != null) { first.ExitAction = null; first.EnterAction = null; first.ExitGate = null; }
            if (second != null) { second.ExitAction = null; second.EnterAction = null; second.ExitGate = null; }
            F.ControllerManager.DestroyAsync<First>().GetAwaiter().GetResult();
            F.ControllerManager.DestroyAsync<Second>().GetAwaiter().GetResult();
            _managerField.SetValue(null, _oldManager);
            foreach (var obj in _objects) if (obj != null) Object.Destroy(obj);
            _objects.Clear();
            FDebug.Debugger = _debugger;
        }

        [Test]
        public void ExitAll_ExitsAndUnbindsEachControllerOnce()
        {
            var a = First.Instance;
            var b = Second.Instance;
            a.EnterAsync().GetAwaiter().GetResult();
            b.EnterAsync().GetAwaiter().GetResult();
            F.ControllerManager.ExitAllAsync().GetAwaiter().GetResult();
            F.ControllerManager.ExitAllAsync().GetAwaiter().GetResult();
            Assert.That(a.ExitCount, Is.EqualTo(1));
            Assert.That(b.ExitCount, Is.EqualTo(1));
            Assert.That(a.RemoveCount, Is.EqualTo(1));
            Assert.That(b.RemoveCount, Is.EqualTo(1));
            Assert.That(F.ControllerManager.EnterList, Is.Empty);
        }

        [Test]
        public void ExitAll_ContinuesCleanupAfterOneControllerFails()
        {
            var a = First.Instance;
            var b = Second.Instance;
            a.EnterAsync().GetAwaiter().GetResult();
            b.EnterAsync().GetAwaiter().GetResult();
            var failure = new InvalidOperationException("exit failed");
            b.ExitAction = () => throw failure;
            Assert.That(Assert.Throws<InvalidOperationException>(
                () => F.ControllerManager.ExitAllAsync().GetAwaiter().GetResult()), Is.SameAs(failure));
            Assert.That(a.ExitCount, Is.EqualTo(1));
            Assert.That(b.ExitCount, Is.EqualTo(1));
            Assert.That(a.RemoveCount, Is.EqualTo(1));
            Assert.That(b.RemoveCount, Is.EqualTo(1));
            Assert.That(F.ControllerManager.EnterList, Is.Empty);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExitAsync_StopsAfterFailureAndResetsTransition(bool asynchronous)
        {
            var controller = First.Instance;
            controller.EnterAsync().GetAwaiter().GetResult();
            int completed = 0;
            controller.OnExitEvent += () => completed++;
            var failure = new InvalidOperationException("exit failed");
            if (asynchronous)
            {
                controller.ExitGate = new TaskCompletionSource<bool>();
                controller.ExitGate.SetException(failure);
            }
            else controller.ExitAction = () => throw failure;

            Assert.That(Assert.Throws<InvalidOperationException>(() => controller.ExitAsync().GetAwaiter().GetResult()), Is.SameAs(failure));
            Assert.That(controller.IsEnter, Is.False);
            Assert.That(controller.RemoveCount, Is.EqualTo(1));
            Assert.That(controller.AsyncExits, Is.EqualTo(asynchronous ? 1 : 0));
            Assert.That(completed, Is.Zero);
            controller.ExitAction = null;
            controller.ExitGate = null;
            controller.EnterAsync().GetAwaiter().GetResult();
            controller.ExitAsync().GetAwaiter().GetResult();
            Assert.That(completed, Is.EqualTo(1));
        }

        [Test]
        public void FailedEnter_PreservesCommittedStackAndCanRefresh()
        {
            var a = First.Instance;
            a.EnterAction = () => throw new InvalidOperationException("enter failed");
            Assert.Throws<InvalidOperationException>(() => a.EnterAsync().GetAwaiter().GetResult());
            Assert.That(a.IsEnter, Is.True);
            Assert.That(a.RemoveCount, Is.Zero);
            a.EnterAction = null;
            a.EnterAsync().GetAwaiter().GetResult();
            Assert.That(a.IsEnter, Is.True);
            a.ExitAsync().GetAwaiter().GetResult();
            Assert.That(a.RemoveCount, Is.EqualTo(1));
        }

        [Test]
        public void FrameCallbacks_UseStableSnapshotAndStopAfterExit()
        {
            var a = First.Instance;
            a.EnterAsync().GetAwaiter().GetResult();
            var ticks = (IFrameworkTick)F.ControllerManager;
            a.UpdateAction = () => Second.Instance.EnterAsync().GetAwaiter().GetResult();
            ticks.OnUpdate();
            ticks.OnLateUpdate();
            Assert.That(a.Updates, Is.EqualTo(1));
            Assert.That(Second.Instance.Updates, Is.Zero);
            ticks.OnUpdate();
            ticks.OnLateUpdate();
            Assert.That(Second.Instance.Updates, Is.EqualTo(1));
            Assert.That(Second.Instance.Lates, Is.EqualTo(1));
            F.ControllerManager.ExitAllAsync().GetAwaiter().GetResult();
            ticks.OnUpdate();
            ticks.OnLateUpdate();
            Assert.That(Second.Instance.Updates, Is.EqualTo(1));
        }

        [Test]
        public void ExitAll_PreservesControllersEnteredDuringExit()
        {
            var a = First.Instance;
            a.EnterAsync().GetAwaiter().GetResult();
            a.ExitAction = () => Second.Instance.EnterAsync().GetAwaiter().GetResult();
            F.ControllerManager.ExitAllAsync().GetAwaiter().GetResult();
            Assert.That(a.ExitCount, Is.EqualTo(1));
            Assert.That(Second.Instance.IsEnter, Is.True);
        }

        [UnityTest]
        public IEnumerator ExitAll_WaitsForAsyncExitAndRejectsReentry()
        {
            var a = First.Instance;
            a.ExitGate = new TaskCompletionSource<bool>();
            a.EnterAsync().GetAwaiter().GetResult();
            var operation = F.ControllerManager.ExitAllAsync();
            Assert.That(operation.GetAwaiter().IsCompleted, Is.False);
            Assert.Throws<InvalidOperationException>(() => a.EnterAsync().GetAwaiter().GetResult());
            a.ExitGate.SetResult(true);
            yield return Wait(() => operation.GetAwaiter().IsCompleted);
            operation.GetAwaiter().GetResult();
            Assert.That(a.ExitCount, Is.EqualTo(1));
            Assert.That(a.RemoveCount, Is.EqualTo(1));
        }

        [Test]
        public void ExitAll_PreservesSameControllerReenteredDuringAnotherExit()
        {
            var a = First.Instance;
            var b = Second.Instance;
            a.EnterAsync().GetAwaiter().GetResult();
            b.EnterAsync().GetAwaiter().GetResult();
            b.ExitAction = () =>
            {
                a.ExitAsync().GetAwaiter().GetResult();
                a.EnterAsync().GetAwaiter().GetResult();
            };

            F.ControllerManager.ExitAllAsync().GetAwaiter().GetResult();

            Assert.That(a.IsEnter, Is.True,
                "The snapshot must not exit a new entry of the same Controller instance.");
            Assert.That(F.ControllerManager.EnterList, Is.EqualTo(new IController[] { a }));
            Assert.That(a.ExitCount, Is.EqualTo(1));
            Assert.That(a.RemoveCount, Is.EqualTo(1));
            Assert.That(b.ExitCount, Is.EqualTo(1));
            Assert.That(b.RemoveCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator AsyncCreation_RejectsOverlappingSyncAndAsyncCreation()
        {
            var window = new ProbeWindow();
            var windowI = window as IWindow;
            var first = windowI.OpenAsync(_layer);
            Assert.Throws<InvalidOperationException>(() => windowI.OpenAsync(_layer).GetAwaiter().GetResult());
            Assert.Throws<InvalidOperationException>(() => windowI.Open(_layer));
            // Rejected calls must not clear the first creation's flag.
            Assert.Throws<InvalidOperationException>(() => windowI.OpenAsync(_layer).GetAwaiter().GetResult());
            Assert.That(window.Requests, Is.EqualTo(1));
            var obj = Track(new GameObject("Window", typeof(RectTransform)));
            window.Gate.SetResult(obj);
            yield return Wait(() => first.GetAwaiter().IsCompleted);
            first.GetAwaiter().GetResult();
            windowI.Open(_layer);
            windowI.OpenAsync(_layer).GetAwaiter().GetResult();
            Assert.That(window.Creates, Is.EqualTo(1));
            Assert.That(window.Requests, Is.EqualTo(1));
            Assert.That(window.WindowObject, Is.SameAs(obj));
            ((IWindow)window).Destroy();
        }

        [UnityTest]
        public IEnumerator AsyncCreation_FailureResetsStateAndAllowsRetry()
        {
            var window = new ProbeWindow();
            var windowI = window as IWindow;
            var first = windowI.OpenAsync(_layer);
            var failure = new InvalidOperationException("create failed");
            window.Gate.SetException(failure);
            yield return Wait(() => first.GetAwaiter().IsCompleted);
            Assert.Throws<ArgumentNullException>(() => first.GetAwaiter().GetResult());
            Assert.That(((QuietDebugger)FDebug.Debugger).LastException, Is.SameAs(failure));
            Assert.That(window.IsLoading, Is.False);
            window.Gate = new TaskCompletionSource<GameObject>();
            var retry = windowI.OpenAsync(_layer);
            var obj = Track(new GameObject("RetryWindow", typeof(RectTransform)));
            window.Gate.SetResult(obj);
            yield return Wait(() => retry.GetAwaiter().IsCompleted);
            retry.GetAwaiter().GetResult();
            Assert.That(window.Requests, Is.EqualTo(2));
            Assert.That(window.Creates, Is.EqualTo(1));
            ((IWindow)window).Destroy();
        }

        [UnityTest]
        public IEnumerator AsyncCreation_NullResultResetsStateAndAllowsSyncCreation()
        {
            var window = new ProbeWindow();
            var windowI = window as IWindow;
            var first = windowI.OpenAsync(_layer);
            window.Gate.SetResult(null);
            yield return Wait(() => first.GetAwaiter().IsCompleted);
            Assert.Throws<ArgumentNullException>(() => first.GetAwaiter().GetResult());
            Assert.That(window.IsLoading, Is.False);
            windowI.Open(_layer);
            Track(window.WindowObject);
            Assert.That(window.WindowObject != null, Is.True);
            Assert.That(window.Creates, Is.EqualTo(1));
            ((IWindow)window).Destroy();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Destroy_ClosesAndUnbindsBeforeDestroy(bool closeFirst)
        {
            var window = new ProbeWindow();
            IWindow api = window;
            api.Open(_layer);
            Track(window.WindowObject);
            api.Open(_layer);
            if (closeFirst) api.Close();
            api.Destroy();
            Assert.That(window.Removes, Is.EqualTo(1));
            Assert.That(window.Closes, Is.EqualTo(1));
            Assert.That(window.Destroys, Is.EqualTo(1));
            Assert.That(window.IsOpen, Is.False);
            Assert.That(window.WindowObject, Is.Null);
            Assert.That(window.CleanupOrder, Is.EqualTo(new[] { "RemoveListeners", "Close", "Destroy" }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DestroyedWindow_RejectsReopenWithoutLoading(bool asynchronous)
        {
            var window = new ProbeWindow();
            IWindow api = window;
            api.Open(_layer);
            Track(window.WindowObject);
            api.Destroy();

            var failure = asynchronous
                ? Assert.Throws<InvalidOperationException>(() => api.OpenAsync(_layer).GetAwaiter().GetResult())
                : Assert.Throws<InvalidOperationException>(() => api.Open(_layer));

            Assert.That(failure.Message, Does.Contain(nameof(ProbeWindow)).And.Contain("destroyed"));
            Assert.That(failure.Message, Does.StartWith(asynchronous ? "Window.OpenAsync(" : "Window.Open("));
            Assert.That(api.Alive, Is.False);
            Assert.That(window.Requests, Is.EqualTo(1));
            Assert.That(window.Creates, Is.EqualTo(1));
            Assert.That(window.Destroys, Is.EqualTo(1));
            Assert.That(window.WindowObject, Is.Null);
            Assert.That(window.IsOpen, Is.False);
            Assert.That(window.IsLoading, Is.False);
        }

        [Test]
        public void Destroy_RecursiveAndRepeatedCallsReleaseTheWindowOnce()
        {
            var window = new ProbeWindow();
            IWindow api = window;
            Assert.That(api.Alive, Is.True);
            api.Open(_layer);
            Track(window.WindowObject);
            api.Close();
            Assert.That(api.Alive, Is.True, "Closing must retain the cached window's resource ownership.");
            window.DestroyAction = () => api.Destroy();

            api.Destroy();
            api.Destroy();
            api.Create();

            Assert.That(api.Alive, Is.False, "Create must not revive a destroyed instance.");
            Assert.That(((IResRequest)window).Alive, Is.False);
            Assert.That(window.Removes, Is.EqualTo(1));
            Assert.That(window.Closes, Is.EqualTo(1));
            Assert.That(window.Destroys, Is.EqualTo(1));
            Assert.That(window.WindowObject, Is.Null);
            Assert.That(window.CleanupOrder, Is.EqualTo(new[] { "RemoveListeners", "Close", "Destroy" }));
        }

        [Test]
        public void DestroyedWindow_StopsFrameRefreshAndActiveCallbacks()
        {
            var window = new ProbeWindow();
            IWindow api = window;
            api.Open(_layer);
            Track(window.WindowObject);
            api.Destroy();
            int refreshes = window.Refreshes;

            api.Update();
            api.Refresh();
            api.SetActive(true);

            Assert.That(window.Updates, Is.Zero);
            Assert.That(window.Refreshes, Is.EqualTo(refreshes));
            Assert.That(window.ActiveChanges, Is.Zero);
            Assert.That(window.WindowObject, Is.Null);
        }

        [UnityTest]
        public IEnumerator Destroy_CallbackFailuresStillReleaseNativeObjectAndAlive()
        {
            foreach (var stage in new[] { "RemoveListeners", "Close", "Destroy", "ComponentDestroy" })
            {
                var window = new DestroyFailureWindow { FailureStage = stage };
                IWindow api = window;
                api.Open(_layer);
                var obj = Track(window.WindowObject);

                Assert.DoesNotThrow(() => api.Destroy(), stage);
                Assert.That(((QuietDebugger)FDebug.Debugger).LastException, Is.SameAs(window.Failure), stage);
                Assert.That(api.Alive, Is.False, stage);
                Assert.That(((IResRequest)window).Alive, Is.False, stage);
                Assert.That(window.IsOpen, Is.False, stage);
                Assert.That(window.WindowObject, Is.Null, stage);
                int calls = window.Calls.Count;
                api.Destroy();
                Assert.That(window.Calls.Count, Is.EqualTo(calls), "Repeated Destroy must not retry callbacks.");

                yield return null;
                Assert.That(obj == null, Is.True, stage + " left the native GameObject alive.");
            }
        }

        [Test]
        public void Destroy_EndsResourceOwnershipAndLoaderReclaimsAfterCacheAndOtherOwners()
        {
            var assembly = typeof(F).Assembly;
            var requestType = assembly.GetType("EasyFramework.AssetBundleRequest", true);
            var loaderType = assembly.GetType("EasyFramework.AssetBundleLoader", true);
            var poolType = typeof(ObjectPool<>).MakeGenericType(requestType);
            var pool = poolType.GetProperty("Shared").GetValue(null);
            var request = poolType.GetMethod("Rent").Invoke(pool, null);
            var loader = (ITickerNode)Activator.CreateInstance(loaderType);
            var requests = (IDictionary)loaderType.GetField("_requestDict", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(loader);
            var owners = (HashSet<IResRequest>)requestType.GetField("ReferList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(request);
            var aliveTime = requestType.GetProperty("AliveTime");
            var first = new ProbeWindow();
            var second = new ProbeWindow();
            var info = new AssetBundleInfo("WindowDestroyOwnership", Array.Empty<string>(), null);
            const string resourceName = "WindowDestroyOwnership";
            bool returned = false;

            try
            {
                requestType.GetField("AbName").SetValue(request, resourceName);
                requestType.GetMethod("InitInfo").Invoke(request, new object[] { resourceName, info });
                owners.Add(first);
                requests.Add(resourceName, request);
                aliveTime.SetValue(request, Time.time - 1);
                loader.OnTick();
                Assert.That(requests.Contains(resourceName), Is.True, "The live window must retain its resource.");

                ((IWindow)first).Destroy();
                Assert.That(first.Alive, Is.False);
                Assert.That(requests.Contains(resourceName), Is.True, "Window.Destroy must not actively unload the resource.");
                Assert.That(info.RefCount, Is.EqualTo(1));

                aliveTime.SetValue(request, Time.time + 60);
                loader.OnTick();
                Assert.That(requests.Contains(resourceName), Is.True, "The loader must honor the cache lifetime.");

                owners.Add(second);
                aliveTime.SetValue(request, Time.time - 1);
                loader.OnTick();
                Assert.That(requests.Contains(resourceName), Is.True, "Another live owner must still retain the resource.");

                ((IWindow)second).Destroy();
                loader.OnTick();
                returned = !requests.Contains(resourceName);
                Assert.That(returned, Is.True, "Dead windows must not indefinitely retain the resource after cache expiry.");
                Assert.That(info.RefCount, Is.Zero);
                Assert.That(owners, Is.Empty, "Returning the request must clear its owner references.");
            }
            finally
            {
                ETask.RemoveTick(loader);
                ((IWindow)first).Destroy();
                ((IWindow)second).Destroy();
                if (!returned)
                {
                    requests.Remove(resourceName);
                    requestType.GetMethod("UnloadForce").Invoke(request, new object[] { false });
                    poolType.GetMethod("Return").Invoke(pool, new[] { request });
                }
            }
        }

        [UnityTest]
        public IEnumerator Initialization_WaitersObserveFailureAndLaterCallRetries()
        {
            var stateField = typeof(F).GetField("_state", BindingFlags.Static | BindingFlags.NonPublic);
            var oldState = stateField.GetValue(null);
            var settings = F.Settings;
            var editorMode = settings.resLoaderEditorMode;
            var loaderMode = settings.resLoaderMode;
            var pathProperty = typeof(EasyFrameworkSettings).GetProperty(nameof(EasyFrameworkSettings.StreamingAssetsDLCPath));
            var oldPath = settings.StreamingAssetsDLCPath;
            var debugLevel = FDebug.DebugLevel;
            var directory = Path.Combine(Application.temporaryCachePath, "Initialization-" + Guid.NewGuid().ToString("N"));
            var manifest = Path.Combine(directory, DLCVersionInfo.FileName);
            Directory.CreateDirectory(directory);
            File.WriteAllText(manifest, "{}");
            try
            {
                stateField.SetValue(null, Enum.Parse(stateField.FieldType, "None"));
                settings.resLoaderEditorMode = false;
                settings.resLoaderMode = EResLoaderMode.DLC_StreamingAssets;
                pathProperty.SetValue(settings, new Uri(directory + Path.DirectorySeparatorChar).AbsoluteUri.TrimEnd('/'));
                LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: DLC manifest initialization failed\\."));
                LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: DLC manifest initialization failed\\."));

                var operation = F.InitializeAsync();
                var a = F.InitializeAsync();
                var b = F.InitializeAsync();
                Assert.That(operation.GetAwaiter().IsCompleted, Is.False);
                Assert.That(a.GetAwaiter().IsCompleted, Is.False);
                Assert.That(b.GetAwaiter().IsCompleted, Is.False);
                yield return Wait(() => operation.GetAwaiter().IsCompleted && a.GetAwaiter().IsCompleted && b.GetAwaiter().IsCompleted);
                Assert.That(Assert.Throws<InvalidOperationException>(() => operation.GetAwaiter().GetResult()).Message,
                    Is.EqualTo("DLC manifest initialization failed."));
                Assert.That(Assert.Throws<Exception>(() => a.GetAwaiter().GetResult()).Message, Is.EqualTo("F.InitializeAsync Error!"));
                Assert.That(Assert.Throws<Exception>(() => b.GetAwaiter().GetResult()).Message, Is.EqualTo("F.InitializeAsync Error!"));
                Assert.That(stateField.GetValue(null).ToString(), Is.EqualTo("Exception"));

                settings.resLoaderEditorMode = true;
                F.InitializeAsync().GetAwaiter().GetResult();
                Assert.That(stateField.GetValue(null).ToString(), Is.EqualTo("Initialized"));
                settings.resLoaderEditorMode = false;
                F.InitializeAsync().GetAwaiter().GetResult();
            }
            finally
            {
                stateField.SetValue(null, oldState);
                settings.resLoaderEditorMode = editorMode;
                settings.resLoaderMode = loaderMode;
                pathProperty.SetValue(settings, oldPath);
                FDebug.DebugLevel = debugLevel;
                File.Delete(manifest);
                Directory.Delete(directory);
            }
        }

        [UnityTest]
        public IEnumerator Initialization_ConcurrentWaitersCompleteIndependently()
        {
            var stateField = typeof(F).GetField("_state", BindingFlags.Static | BindingFlags.NonPublic);
            var oldState = stateField.GetValue(null);
            stateField.SetValue(null, Enum.Parse(stateField.FieldType, "Initializing"));
            try
            {
                var a = F.InitializeAsync();
                var b = F.InitializeAsync();
                Assert.That(a.GetAwaiter().IsCompleted, Is.False);
                Assert.That(b.GetAwaiter().IsCompleted, Is.False);
                stateField.SetValue(null, Enum.Parse(stateField.FieldType, "Initialized"));
                yield return Wait(() => a.GetAwaiter().IsCompleted && b.GetAwaiter().IsCompleted);
                a.GetAwaiter().GetResult();
                b.GetAwaiter().GetResult();
                F.InitializeAsync().GetAwaiter().GetResult();
            }
            finally { stateField.SetValue(null, oldState); }
        }

        [UnityTest]
        public IEnumerator Manager_OverlappingOpenLogsFailure_AndCompletedWindowCanReopen()
        {
            var manager = F.WindowManager;
            var debugger = (QuietDebugger)FDebug.Debugger;
            var first = manager.OpenAsync<ProbeWindow>(UILayer.HUD);
            var window = manager.GetWindow<ProbeWindow>();
            var second = manager.OpenAsync<ProbeWindow>(UILayer.Popup);
            Assert.That(second.GetAwaiter().GetResult(), Is.Null);
            Assert.That(debugger.LastError, Is.EqualTo("F.UISystem.OpenWindow(ProbeWindow): window is loading."));
            Assert.That(debugger.LastException, Is.Null);
            Assert.That(window.Requests, Is.EqualTo(1));
            var obj = Track(new GameObject("ConcurrentWindow", typeof(RectTransform)));
            window.Gate.SetResult(obj);
            yield return Wait(() => first.GetAwaiter().IsCompleted);
            Assert.That(first.GetAwaiter().GetResult(), Is.SameAs(window));
            Assert.That(manager.GetWindow<ProbeWindow>(), Is.SameAs(window),
                "Rejecting a second open must not remove the first operation's window.");
            Assert.That(manager.OpenAsync<ProbeWindow>(UILayer.Popup).GetAwaiter().GetResult(), Is.SameAs(window));
            Assert.That(window.Creates, Is.EqualTo(1));
            Assert.That(window.Requests, Is.EqualTo(1));
            Assert.That(window.LayerBehaviour, Is.SameAs(manager.GetLayerData(UILayer.Popup)));
            manager.Destroy(window);
        }

        [UnityTest]
        public IEnumerator Manager_DestroyAndTimeout_InvalidateContainerAndTracking()
        {
            var manager = F.WindowManager;
            var first = manager.Open<ProbeWindow>();
            manager.Close(first);
            manager.Close(first);
            manager.Destroy(first);
            Assert.That(manager.GetWindow<ProbeWindow>(), Is.Null);
            var second = manager.Open<ProbeWindow>();
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(first.Destroys, Is.EqualTo(1));
            yield return null;
            manager.Close(second);
            yield return null;
            yield return null;
            Assert.That(first.Destroys, Is.EqualTo(1));
            Assert.That(second.Destroys, Is.EqualTo(1));
            Assert.That(manager.GetWindow<ProbeWindow>(), Is.Null);
        }

        [UnityTest]
        public IEnumerator BulkInstantiation_WaitsForAllNativeObjects()
        {
            var template = Track(new GameObject("BulkTemplate"));
            var loader = new TemplateLoader { Template = template };
            var operation = loader.CreateObjAsync("template", 5, _layer.transform);
            yield return Wait(() => operation.GetAwaiter().IsCompleted);
            var objects = operation.GetAwaiter().GetResult();
            Assert.That(objects.Length, Is.EqualTo(5));
            foreach (var obj in objects)
            {
                Track(obj);
                Assert.That(obj != null, Is.True);
                Assert.That(obj.transform.parent, Is.SameAs(_layer.transform));
            }
        }

        [Test]
        public void BulkInstantiation_ValidatesCountBeforeLoading()
        {
            var loader = new TemplateLoader();
            Assert.That(loader.CreateObjAsync("template", 0).GetAwaiter().GetResult(), Is.Empty);
            Assert.Throws<ArgumentOutOfRangeException>(() => loader.CreateObjAsync("template", -1).GetAwaiter().GetResult());
            Assert.That(loader.Loads, Is.Zero);
        }

        private GameObject Track(GameObject obj) { _objects.Add(obj); return obj; }
        private static IEnumerator Wait(Func<bool> predicate)
        {
            for (int frame = 0; frame < 120 && !predicate(); frame++) yield return null;
            Assert.That(predicate(), Is.True, "Operation did not converge in 120 frames.");
        }

        public abstract class ProbeController<T> : Controller<T> where T : ProbeController<T>, new()
        {
            public int ExitCount, RemoveCount, Updates, Lates, AsyncExits;
            public Action UpdateAction, ExitAction, EnterAction;
            protected override void OnEnter() { EnterAction?.Invoke(); }
            public TaskCompletionSource<bool> ExitGate;
            protected override void OnUpdate() { Updates++; UpdateAction?.Invoke(); }
            protected override void OnLateUpdate() { Lates++; }
            protected override void OnRemoveListeners() { RemoveCount++; }
            protected override void OnExit() { ExitCount++; ExitAction?.Invoke(); }
            protected override async ETask OnExitAsync() { AsyncExits++; if (ExitGate != null) await ExitGate.Task; }
        }
        public sealed class First : ProbeController<First> { }
        public sealed class Second : ProbeController<Second> { }
        public sealed class DestroyFailureWindow : Window
        {
            public string FailureStage;
            public readonly Exception Failure = new InvalidOperationException("window cleanup failed");
            public readonly List<string> Calls = new();
            public readonly DestroyFailureComponent Component = new();
            protected override GameObject CreateWindowObject(Transform parent) => new GameObject("DestroyFailureWindow", typeof(RectTransform));
            public void Record(string stage)
            {
                Calls.Add(stage);
                if (stage == FailureStage) throw Failure;
            }
            protected override void OnRemoveListeners() => Record("RemoveListeners");
            protected override void OnClose() => Record("Close");
            protected override void OnDestroy() => Record("Destroy");
        }
        public sealed class DestroyFailureComponent : WindowComponent<DestroyFailureWindow>
        {
            protected override void OnDestroy() => Window.Record("ComponentDestroy");
        }
        public sealed class ProbeWindow : Window
        {
            public TaskCompletionSource<GameObject> Gate = new();
            public int Requests, Creates, Removes, Closes, Destroys;
            public int Updates, Refreshes, ActiveChanges;
            public Action DestroyAction;
            public readonly List<string> CleanupOrder = new();
            protected override float KeepAliveTime => -1;
            protected override GameObject CreateWindowObject(Transform parent)
            {
                Requests++;
                return new GameObject("Window", typeof(RectTransform));
            }
            protected override async ETask<GameObject> CreateWindowObjectAsync(Transform parent)
            {
                Requests++;
                return await Gate.Task;
            }
            protected override void OnWindowLoaded() { Creates++; }
            protected override void OnRemoveListeners() { Removes++; CleanupOrder.Add("RemoveListeners"); }
            protected override void OnClose() { Closes++; CleanupOrder.Add("Close"); }
            protected override void OnDestroy() { Destroys++; CleanupOrder.Add("Destroy"); DestroyAction?.Invoke(); }
            protected override void OnUpdate() { Updates++; }
            protected override void OnRefresh() { Refreshes++; }
            protected override void OnSetActive(bool value) { ActiveChanges++; }
        }
        private sealed class TemplateLoader : IResLoader
        {
            public GameObject Template;
            public int Loads;
            public async ETask<T> LoadAssetAsync<T>(string name, IResRequest request = null) where T : Object
            {
                Loads++;
                return Template as T;
            }
            public T LoadAsset<T>(string name, IResRequest request = null) where T : Object => throw new NotSupportedException();
            public T[] LoadAllAssets<T>(string name, IResRequest request = null) where T : Object => throw new NotSupportedException();
            public ETask<T[]> LoadAllAssetsAsync<T>(string name, IResRequest request = null) where T : Object => throw new NotSupportedException();
            public AssetBundle Load(string name, IResRequest request = null) => throw new NotSupportedException();
            public ETask<AssetBundle> LoadAsync(string name, IResRequest request = null) => throw new NotSupportedException();
            public void Unload(string name, IResRequest request = null) => throw new NotSupportedException();
            public void UnloadForce(string name, bool all = false) => throw new NotSupportedException();
            public void UnloadAllForce(bool all = false) => throw new NotSupportedException();
            public bool IsLoading(string name) => false;
            public bool IsUnloading(string name) => false;
            public float GetLoadingProgress(string name) => 0;
            public string[] GetAllDependencies(string name) => Array.Empty<string>();
            public void LoadScene(string name, UnityEngine.SceneManagement.LoadSceneMode mode = UnityEngine.SceneManagement.LoadSceneMode.Single, IResRequest request = null) => throw new NotSupportedException();
            public ETask<AsyncOperation> LoadSceneAsync(string name, UnityEngine.SceneManagement.LoadSceneMode mode = UnityEngine.SceneManagement.LoadSceneMode.Single, IResRequest request = null) => throw new NotSupportedException();
            public ETask<AsyncOperation> UnloadSceneAsync(string name, IResRequest request = null) => throw new NotSupportedException();
        }
        private sealed class QuietDebugger : IDebugger
        {
            public Exception LastException;
            public string LastError;
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { LastError = message; }
            public void LogException(Exception exception) { LastException = exception; }
        }
    }
}

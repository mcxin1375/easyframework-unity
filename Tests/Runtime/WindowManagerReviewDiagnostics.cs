using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace EasyFramework.Tests
{
    [Category("WindowManagerReview")]
    public class WindowManagerReviewDiagnostics
    {
        private IDebugger _debugger;
        private readonly List<Window> _windows = new();
        private readonly List<GameObject> _objects = new();

        [SetUp]
        public void Setup()
        {
            _debugger = FDebug.Debugger;
            FDebug.Debugger = new QuietDebugger();
        }
        [TearDown]
        public void Cleanup()
        {
            foreach (var window in _windows)
                if (window is OrderWindow ordered) ordered.OrderAction = null;
            for (int i = _windows.Count - 1; i >= 0; i--)
            {
                var window = _windows[i];
                if (ReferenceEquals(F.WindowManager.GetWindow(window.GetType()), window))
                    F.WindowManager.Destroy(window);
                else if (window.WindowObject != null)
                {
                    F.WindowManager.Close(window);
                    ((IWindow)window).Destroy();
                }
            }
            foreach (var obj in _objects) if (obj != null) Object.Destroy(obj);
            _windows.Clear();
            _objects.Clear();
            FDebug.Debugger = _debugger;
        }

        [UnityTest]
        public IEnumerator CrossLayerOverlappingOpen_KeepsAcceptedOpenInItsLayer()
            => RejectedOpenPreservesPendingWindow(true, UILayer.Popup);

        [UnityTest]
        public IEnumerator SameLayerOverlappingOpen_KeepsAcceptedOpenRegistered()
            => RejectedOpenPreservesPendingWindow(true, UILayer.HUD);

        [UnityTest]
        public IEnumerator RejectedSyncOpen_KeepsPendingAsyncOpenRegistered()
            => RejectedOpenPreservesPendingWindow(false, UILayer.HUD);

        private IEnumerator RejectedOpenPreservesPendingWindow(bool asynchronous, UILayer targetLayer)
        {
            var first = F.WindowManager.OpenAsync<GateWindow>(UILayer.HUD);
            var window = F.WindowManager.GetWindow<GateWindow>();
            _windows.Add(window);
            var rejected = asynchronous
                ? F.WindowManager.OpenAsync<GateWindow>(targetLayer).GetAwaiter().GetResult()
                : F.WindowManager.Open<GateWindow>(targetLayer);
            window.Gate.SetResult(Track(new GameObject("ReviewWindow", typeof(RectTransform))));
            yield return Wait(() => first.GetAwaiter().IsCompleted);
            Assert.That(first.GetAwaiter().GetResult(), Is.SameAs(window));
            Assert.That(rejected, Is.Null);
            Assert.That(F.WindowManager.GetWindow<GateWindow>(), Is.SameAs(window),
                "Rejecting a second open must preserve the first operation's managed instance.");
            Assert.That(window.Destroys, Is.Zero,
                "A rejected call must not destroy the window owned by the pending open.");
            Assert.That(window.LayerBehaviour, Is.SameAs(F.WindowManager.GetLayerData(UILayer.HUD)));
            Assert.That(F.WindowManager.GetWindows(UILayer.HUD), Has.Member(window),
                "Accepted HUD open completed, but the failed Popup call moved its registration to Popup.");
        }

        [UnityTest]
        public IEnumerator FailedResourceRequest_ManagerLogsFailureAndReturnsNull()
        {
            var operation = F.WindowManager.OpenAsync<GateWindow>();
            var window = F.WindowManager.GetWindow<GateWindow>();
            _windows.Add(window);
            var failure = new InvalidOperationException("resource failed");
            window.Gate.SetException(failure);
            yield return Wait(() => operation.GetAwaiter().IsCompleted);
            Assert.That(operation.GetAwaiter().GetResult(), Is.Null);
            Assert.That(((QuietDebugger)FDebug.Debugger).Exceptions, Has.Member(failure),
                "The original resource error must remain available in the logged failures.");
            Assert.That(((QuietDebugger)FDebug.Debugger).LastException, Is.TypeOf<ArgumentNullException>());
            Assert.That(window.IsOpen, Is.False);
            Assert.That(window.WindowObject, Is.Null);
            Assert.That(F.WindowManager.GetWindow<GateWindow>(), Is.Null);
        }

        [UnityTest]
        public IEnumerator FailedResourceRequest_DoesNotRemainInOpenedLayerQueries()
        {
            var operation = F.WindowManager.OpenAsync<GateWindow>();
            var window = F.WindowManager.GetWindow<GateWindow>();
            _windows.Add(window);
            window.Gate.SetException(new InvalidOperationException("resource failed"));
            yield return Wait(() => operation.GetAwaiter().IsCompleted);
            operation.GetAwaiter().GetResult();
            Assert.That(window.IsOpen, Is.False);
            Assert.That(F.WindowManager.GetWindows(UILayer.HUD), Has.No.Member(window),
                "Failed, closed window is still returned as an opened layer member and top window.");
        }

        [UnityTest]
        public IEnumerator FailedLoadHook_DoesNotOpenUninitializedComponents()
        {
            var result = F.WindowManager.Open<BrokenLoadWindow>();
            var window = BrokenLoadWindow.LastCreated;
            _windows.Add(window);
            Track(window.WindowObject);
            yield return null;
            Assert.That(result, Is.Null);
            Assert.That(window.IsOpen, Is.False);
            Assert.That(window.WindowObject, Is.Null);
            Assert.That(F.WindowManager.GetWindow<BrokenLoadWindow>(), Is.Null);
            Assert.That(window.Component.Loads, Is.Zero);
            Assert.That(window.Component.Opens, Is.Zero,
                "Window OnLoadObject failed before component loading, but the component still received OnOpen.");
        }

        [Test]
        public void NullResourceResult_SyncOpenReturnsNullAndRemovesRegistration()
        {
            var result = F.WindowManager.Open<NullResourceWindow>();
            var window = F.WindowManager.GetWindow<NullResourceWindow>();
            if (window != null) _windows.Add(window);

            Assert.That(result, Is.Null, "A missing resource must not return a successful window.");
            Assert.That(((QuietDebugger)FDebug.Debugger).LastException, Is.TypeOf<ArgumentNullException>());
            Assert.That(window, Is.Null);
            Assert.That(F.WindowManager.GetWindows(UILayer.HUD), Has.None.InstanceOf<NullResourceWindow>());
        }

        [UnityTest]
        public IEnumerator NullResourceResult_AsyncOpenReturnsNullAndRemovesRegistration()
        {
            var operation = F.WindowManager.OpenAsync<NullResourceWindow>();
            var window = F.WindowManager.GetWindow<NullResourceWindow>();
            if (window != null) _windows.Add(window);
            yield return Wait(() => operation.GetAwaiter().IsCompleted);

            Assert.That(operation.GetAwaiter().GetResult(), Is.Null,
                "A missing resource must not return a successful window.");
            Assert.That(((QuietDebugger)FDebug.Debugger).LastException, Is.TypeOf<ArgumentNullException>());
            Assert.That(F.WindowManager.GetWindow<NullResourceWindow>(), Is.Null);
            Assert.That(F.WindowManager.GetWindows(UILayer.HUD), Has.None.InstanceOf<NullResourceWindow>());
        }

        [Test]
        public void FirstSyncOpen_AppliesOrderingToTheCreatedObject()
        {
            var first = F.WindowManager.Open<OrderWindowA>();
            var second = F.WindowManager.Open<OrderWindowB>();
            _windows.Add(first);
            _windows.Add(second);
            Track(first.WindowObject);
            Track(second.WindowObject);
            var layer = F.WindowManager.GetLayerData(UILayer.HUD);

            Assert.That(second.Canvas.overrideSorting, Is.True,
                "Ordering must be applied after the first object has been created.");
            Assert.That(second.Canvas.sortingOrder, Is.EqualTo(layer.Canvas.sortingOrder + layer.OrderInterval));
            Assert.That(second.Orders, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FirstAsyncOpen_AppliesOrderingAfterResourceCompletes()
        {
            var opening = F.WindowManager.OpenAsync<GateWindow>();
            var window = F.WindowManager.GetWindow<GateWindow>();
            _windows.Add(window);
            window.Gate.SetResult(Track(new GameObject("FirstAsyncWindow", typeof(RectTransform))));
            yield return Wait(() => opening.GetAwaiter().IsCompleted);
            Assert.That(opening.GetAwaiter().GetResult(), Is.SameAs(window));

            Assert.That(window.Canvas.overrideSorting, Is.True,
                "An incomplete resource request must be ordered again after its object exists.");
            Assert.That(window.Canvas.sortingOrder,
                Is.EqualTo(F.WindowManager.GetLayerData(UILayer.HUD).Canvas.sortingOrder));
            Assert.That(window.Orders, Is.EqualTo(1), "A single successful async open must not sort its layer twice.");
        }

        [Test]
        public void Destroy_SortingCallbackReopenCreatesNewLifetime()
        {
            var first = F.WindowManager.Open<OrderWindowA>();
            var second = F.WindowManager.Open<OrderWindowB>();
            _windows.Add(first);
            _windows.Add(second);
            Track(first.WindowObject);
            Track(second.WindowObject);
            OrderWindowA reopened = null;
            first.OrderAction = () => F.WindowManager.Destroy(first);
            second.OrderAction = () => reopened = F.WindowManager.Open<OrderWindowA>();

            F.WindowManager.Open<OrderWindowB>();
            if (reopened != null && !ReferenceEquals(first, reopened))
            {
                _windows.Add(reopened);
                Track(reopened.WindowObject);
            }

            Assert.That(reopened, Is.Not.Null);
            Assert.That(reopened, Is.Not.SameAs(first),
                "The type entry must be invalidated before removing a layer entry can invoke sorting callbacks.");
            Assert.That(reopened.IsOpen, Is.True);
            Assert.That(F.WindowManager.GetWindow<OrderWindowA>(), Is.SameAs(reopened));
        }

        [Test]
        public void Destroy_OrderFailureInAnotherWindowStillReleasesTarget()
        {
            var first = F.WindowManager.Open<OrderWindowA>();
            var second = F.WindowManager.Open<OrderWindowB>();
            _windows.Add(first);
            _windows.Add(second);
            Track(first.WindowObject);
            Track(second.WindowObject);
            first.OrderAction = () => F.WindowManager.Destroy(first);
            second.OrderAction = () => throw new InvalidOperationException("Other window ordering failed.");

            F.WindowManager.Open<OrderWindowB>();

            Assert.That(F.WindowManager.GetWindow<OrderWindowA>(), Is.Null,
                "A different window's ordering callback must not abort target cleanup.");
            Assert.That(first.WindowObject, Is.Null);
            Assert.That(first.IsOpen, Is.False);
        }

        [Test]
        public void ImmediateSort_ExceptionClearsActiveState()
        {
            var first = F.WindowManager.Open<OrderWindowA>();
            var second = F.WindowManager.Open<OrderWindowB>();
            _windows.Add(first);
            _windows.Add(second);
            Track(first.WindowObject);
            Track(second.WindowObject);
            var info = new WindowLayerInfo(F.WindowManager.GetLayerData(UILayer.HUD));
            info.Add(first);
            var failure = new InvalidOperationException("Ordering failed.");
            first.OrderAction = () => throw failure;
            Assert.DoesNotThrow(() => info.RefreshOrder());
            Assert.That(((QuietDebugger)FDebug.Debugger).LastException, Is.SameAs(failure));
            first.Orders = 0;

            info.Add(second);

            Assert.That(first.Orders, Is.Zero,
                "After a failed refresh, Add must behave like an ordinary membership change outside sorting.");
        }

        [Test]
        public void ImmediateSort_OneNestedChangeStopsTheOlderLoop()
        {
            var first = OpenOrderProbe<OrderWindowA>();
            var second = OpenOrderProbe<OrderWindowB>();
            var added = OpenOrderProbe<OrderWindowC>();
            var info = new WindowLayerInfo(F.WindowManager.GetLayerData(UILayer.HUD));
            info.Add(first);
            info.Add(second);
            first.Orders = second.Orders = added.Orders = 0;
            first.OrderAction = () => info.Add(added);

            info.RefreshOrder();

            Assert.That(first.Orders, Is.EqualTo(2), "Both the original and nested pass reach the first window.");
            Assert.That(second.Orders, Is.EqualTo(1), "The original pass must not resume at the second window.");
            Assert.That(added.Orders, Is.EqualTo(1));
            Assert.That(added.Canvas.sortingOrder,
                Is.EqualTo(info.LayerBehaviour.Canvas.sortingOrder + 2 * info.LayerBehaviour.OrderInterval));
        }

        [Test]
        public void ImmediateSort_RemoveAfterNestedSortPreservesRelativeOrder()
        {
            var first = OpenOrderProbe<OrderWindowA>();
            var second = OpenOrderProbe<OrderWindowB>();
            var added = OpenOrderProbe<OrderWindowC>();
            var info = new WindowLayerInfo(F.WindowManager.GetLayerData(UILayer.HUD));
            info.Add(first);
            info.Add(second);
            first.OrderAction = () =>
            {
                info.Add(added);
                info.Remove(second);
            };

            info.RefreshOrder();

            Assert.That(info.WindowList, Is.EqualTo(new IWindow[] { first, added }));
            Assert.That(added.Canvas.sortingOrder,
                Is.GreaterThan(first.Canvas.sortingOrder),
                "Removing a window may leave gaps; the remaining windows must retain their relative order.");
            Assert.That(added.RectTransform.GetSiblingIndex(), Is.GreaterThan(first.RectTransform.GetSiblingIndex()));
        }

        [Test]
        public void ImmediateSort_WindowComponentsKeepTheNewestOrderAfterNestedReorder()
        {
            var first = OpenOrderProbe<ComponentOrderWindow>();
            var second = OpenOrderProbe<OrderWindowB>();
            var info = new WindowLayerInfo(F.WindowManager.GetLayerData(UILayer.HUD));
            info.Add(first);
            info.Add(second);
            first.OrderAction = () => info.Add(first);

            info.RefreshOrder();

            int expectedOrder = info.LayerBehaviour.Canvas.sortingOrder + info.LayerBehaviour.OrderInterval;
            Assert.That(first.Canvas.sortingOrder, Is.EqualTo(expectedOrder));
            Assert.That(first.Component.LastOrder, Is.EqualTo(expectedOrder),
                "The old Window.RefreshOrder must not deliver an obsolete component order after the nested pass.");
        }

        private T OpenOrderProbe<T>() where T : OrderWindow, new()
        {
            var window = F.WindowManager.Open<T>();
            _windows.Add(window);
            Track(window.WindowObject);
            return window;
        }

        [Test]
        public void SortingCallback_OpenWindowDoesNotReenterBeforeThatWindowOpens()
        {
            var first = F.WindowManager.Open<OrderWindowA>();
            _windows.Add(first);
            Track(first.WindowObject);
            int requests = 0;
            Action openMissingWindow = null;
            openMissingWindow = () =>
            {
                if (F.WindowManager.IsOpen<OrderWindowB>()) return;
                requests++;
                // Bound the diagnostic so a recursive implementation cannot overflow Unity's stack.
                if (requests >= 4) return;
                first.OrderAction = openMissingWindow;
                F.WindowManager.Open<OrderWindowB>();
            };
            first.OrderAction = openMissingWindow;

            F.WindowManager.Open<OrderWindowA>();
            var second = F.WindowManager.GetWindow<OrderWindowB>();
            if (second != null)
            {
                _windows.Add(second);
                Track(second.WindowObject);
            }

            Assert.That(requests, Is.EqualTo(1),
                "Add must not invoke the same callback again before the requested window reaches IsOpen.");
            Assert.That(second, Is.Not.Null);
            Assert.That(second.IsOpen, Is.True);
        }

        [Test]
        public void SortingCallback_ClosingCurrentWindowDoesNotSkipNextWindow()
        {
            var first = F.WindowManager.Open<OrderWindowA>();
            var second = F.WindowManager.Open<OrderWindowB>();
            _windows.Add(first);
            _windows.Add(second);
            Track(first.WindowObject);
            Track(second.WindowObject);
            first.OrderAction = () => F.WindowManager.Close(first);
            second.Orders = 0;

            F.WindowManager.Open<OrderWindowB>();

            Assert.That(first.IsOpen, Is.False);
            Assert.That(second.Orders, Is.EqualTo(1),
                "Closing the current entry shifts the live list and skips the next window's ordering callback.");
            Assert.That(second.Canvas.sortingOrder,
                Is.EqualTo(F.WindowManager.GetLayerData(UILayer.HUD).Canvas.sortingOrder));
        }

        private GameObject Track(GameObject obj) { _objects.Add(obj); return obj; }
        private static IEnumerator Wait(Func<bool> predicate)
        {
            for (int frame = 0; frame < 120 && !predicate(); frame++) yield return null;
            Assert.That(predicate(), Is.True, "Operation did not converge in 120 frames.");
        }
        public sealed class GateWindow : Window
        {
            public readonly TaskCompletionSource<GameObject> Gate = new();
            public int Destroys;
            public int Orders;
            protected override async ETask<GameObject> CreateWindowObjectAsync(Transform parent)
            {
                var obj = await Gate.Task;
                if (obj != null) obj.transform.SetParent(parent, false);
                return obj;
            }
            protected override void OnDestroy() { Destroys++; }
            protected override void OnRefreshOrderInLayer(int orderInLayer) { Orders++; }
        }
        public sealed class BrokenLoadWindow : Window
        {
            public static BrokenLoadWindow LastCreated;
            public BrokenLoadWindow() { LastCreated = this; }
            public readonly ProbeComponent Component = new();
            protected override GameObject CreateWindowObject(Transform parent) => new GameObject("BrokenLoadWindow", typeof(RectTransform));
            protected override void OnLoadObject() => throw new InvalidOperationException("load hook failed");
        }
        public sealed class NullResourceWindow : Window
        {
            protected override GameObject CreateWindowObject(Transform parent) => null;
            protected override ETask<GameObject> CreateWindowObjectAsync(Transform parent) => ETask.FromResult<GameObject>(null);
        }
        public sealed class ProbeComponent : WindowComponent
        {
            public int Loads, Opens;
            protected override void OnLoadObject() { Loads++; }
            protected override void OnOpen() { Opens++; }
        }
        public abstract class OrderWindow : Window
        {
            public int Orders;
            public Action OrderAction;
            protected override GameObject CreateWindowObject(Transform parent)
            {
                var obj = new GameObject("OrderWindow", typeof(RectTransform));
                obj.transform.SetParent(parent, false);
                return obj;
            }
            protected override void OnRefreshOrderInLayer(int orderInLayer)
            {
                Orders++;
                var action = OrderAction;
                OrderAction = null;
                action?.Invoke();
            }
        }
        public sealed class OrderWindowA : OrderWindow { }
        public sealed class OrderWindowB : OrderWindow { }
        public sealed class OrderWindowC : OrderWindow { }
        public sealed class ComponentOrderWindow : OrderWindow
        {
            public readonly OrderProbeComponent Component = new();
        }
        public sealed class OrderProbeComponent : WindowComponent
        {
            public int LastOrder;
            protected override void OnRefreshOrderInLayer(int orderInLayer) { LastOrder = orderInLayer; }
        }
        private sealed class QuietDebugger : IDebugger
        {
            public readonly List<Exception> Exceptions = new();
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
            public Exception LastException;
            public void LogException(Exception exception) { LastException = exception; Exceptions.Add(exception); }
        }
    }
}

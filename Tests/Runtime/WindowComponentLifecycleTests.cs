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
    public class WindowComponentLifecycleTests
    {
        private GameObject _layerObject;
        private UILayerBehaviour _layer;
        private LifecycleWindow _window;

        [SetUp]
        public void Setup()
        {
            _layerObject = new GameObject("ComponentLifecycleLayer", typeof(RectTransform), typeof(Canvas));
            _layer = _layerObject.AddComponent<UILayerBehaviour>();
            _window = new LifecycleWindow();
        }

        [TearDown]
        public void Cleanup()
        {
            if (_window.WindowObject != null) ((IWindow)_window).Destroy();
            if (_layerObject != null) Object.Destroy(_layerObject);
        }

        [Test]
        public void SynchronousOpen_RegistersComponentsBeforeLoadingAndReusesThemAfterClose()
        {
            IWindow api = _window;
            api.Create();
            Assert.That(_window.WindowObject, Is.Null, "Create should leave loading to Open.");
            api.Open(_layer);
            AssertLoadedAndOpenedOnce();
            VerifyCloseReopenAndDestroy();
        }

        [UnityTest]
        public IEnumerator AsynchronousOpen_RegistersComponentsAfterLoadingAndPreservesLifecycleOnReopen()
        {
            IWindow api = _window;
            api.Create();
            var opening = api.OpenAsync(_layer);
            Assert.That(opening.GetAwaiter().IsCompleted, Is.False);
            Assert.That(_window.WindowObject, Is.Null);
            Assert.That(_window.Events, Is.Empty, "Callbacks must wait for the loaded object.");
            _window.Gate.SetResult(new GameObject("AsyncComponentWindow", typeof(RectTransform)));
            for (int frame = 0; frame < 120 && !opening.GetAwaiter().IsCompleted; frame++) yield return null;
            Assert.That(opening.GetAwaiter().IsCompleted, Is.True);
            opening.GetAwaiter().GetResult();
            AssertLoadedAndOpenedOnce();
            VerifyCloseReopenAndDestroy();
        }

        private void AssertLoadedAndOpenedOnce()
        {
            Assert.That(_window.IsOpen, Is.True);
            Assert.That(_window.ComponentsRegisteredAtLoad, Is.True,
                "Both field components must have their owner before Window.OnLoadObject.");
            Assert.That(_window.Plain.Owner, Is.SameAs(_window));
            Assert.That(_window.Typed.Window, Is.SameAs(_window));
            Assert.That(_window.Plain.LoadOwner, Is.SameAs(_window));
            Assert.That(_window.Typed.LoadOwner, Is.SameAs(_window));
            Assert.That(_window.Loads, Is.EqualTo(1));
            Assert.That(_window.Plain.Loads, Is.EqualTo(1));
            Assert.That(_window.Typed.Loads, Is.EqualTo(1));
            foreach (var component in new[] { "plain", "typed" })
            {
                Before("window.load", component + ".load");
                Before(component + ".load", "window.open");
                Before("window.open", component + ".open");
                Before(component + ".open", "window.refresh");
                Before("window.refresh", component + ".refresh");
            }
        }

        private void VerifyCloseReopenAndDestroy()
        {
            IWindow api = _window;
            var loadedObject = _window.WindowObject;
            api.Close();
            Assert.That(_window.IsOpen, Is.False);
            api.Open(_layer);
            Assert.That(_window.WindowObject, Is.SameAs(loadedObject));
            Assert.That(_window.Loads, Is.EqualTo(1));
            Assert.That(_window.Plain.Loads, Is.EqualTo(1));
            Assert.That(_window.Typed.Loads, Is.EqualTo(1));
            Assert.That(_window.Adds, Is.EqualTo(2));
            Assert.That(_window.Plain.Adds, Is.EqualTo(2));
            Assert.That(_window.Typed.Adds, Is.EqualTo(2));
            Assert.That(_window.Typed.Window, Is.SameAs(_window));

            _window.Events.Clear();
            api.Destroy();
            Assert.That(_window.IsOpen, Is.False);
            Assert.That(_window.WindowObject, Is.Null);
            Assert.That(_window.Closes, Is.EqualTo(2));
            Assert.That(_window.Plain.Closes, Is.EqualTo(2));
            Assert.That(_window.Typed.Closes, Is.EqualTo(2));
            Assert.That(_window.Destroys, Is.EqualTo(1));
            Assert.That(_window.Plain.Destroys, Is.EqualTo(1));
            Assert.That(_window.Typed.Destroys, Is.EqualTo(1));
            foreach (var component in new[] { "plain", "typed" })
            {
                Before("window.close", component + ".close");
                Before(component + ".close", "window.destroy");
                Before("window.destroy", component + ".destroy");
            }
        }

        private void Before(string earlier, string later)
        {
            int first = _window.Events.IndexOf(earlier);
            int second = _window.Events.IndexOf(later);
            Assert.That(first, Is.GreaterThanOrEqualTo(0), earlier + " was not called.");
            Assert.That(second, Is.GreaterThan(first), earlier + " should run before " + later);
        }

        public sealed class LifecycleWindow : Window
        {
            public readonly List<string> Events = new();
            public readonly PlainComponent Plain = new();
            public readonly TypedComponent Typed = new();
            public readonly TaskCompletionSource<GameObject> Gate = new();
            public bool ComponentsRegisteredAtLoad;
            public int Loads, Adds, Closes, Destroys;

            protected override GameObject CreateWindowObject(Transform parent)
            {
                var obj = new GameObject("SyncComponentWindow", typeof(RectTransform));
                obj.transform.SetParent(parent, false);
                return obj;
            }
            protected override async ETask<GameObject> CreateWindowObjectAsync(Transform parent)
            {
                var obj = await Gate.Task;
                obj.transform.SetParent(parent, false);
                return obj;
            }
            protected override void OnLoadObject()
            {
                Loads++;
                ComponentsRegisteredAtLoad = ReferenceEquals(Plain.Owner, this) && ReferenceEquals(Typed.Window, this);
                Events.Add("window.load");
            }
            protected override void OnAddListeners() { Adds++; Events.Add("window.add"); }
            protected override void OnOpen() { Events.Add("window.open"); }
            protected override void OnRefresh() { Events.Add("window.refresh"); }
            protected override void OnClose() { Closes++; Events.Add("window.close"); }
            protected override void OnDestroy() { Destroys++; Events.Add("window.destroy"); }
        }

        public sealed class PlainComponent : WindowComponent
        {
            public IWindow Owner => Window;
            public IWindow LoadOwner;
            public int Loads, Adds, Closes, Destroys;
            private List<string> Events => ((LifecycleWindow)Window).Events;
            protected override void OnLoadObject() { Loads++; LoadOwner = Window; Events.Add("plain.load"); }
            protected override void OnAddListeners() { Adds++; Events.Add("plain.add"); }
            protected override void OnOpen() { Events.Add("plain.open"); }
            protected override void OnRefresh() { Events.Add("plain.refresh"); }
            protected override void OnClose() { Closes++; Events.Add("plain.close"); }
            protected override void OnDestroy() { Destroys++; Events.Add("plain.destroy"); }
        }

        public sealed class TypedComponent : WindowComponent<LifecycleWindow>
        {
            public LifecycleWindow LoadOwner;
            public int Loads, Adds, Closes, Destroys;
            protected override void OnLoadObject() { Loads++; LoadOwner = Window; Window.Events.Add("typed.load"); }
            protected override void OnAddListeners() { Adds++; Window.Events.Add("typed.add"); }
            protected override void OnOpen() { Window.Events.Add("typed.open"); }
            protected override void OnRefresh() { Window.Events.Add("typed.refresh"); }
            protected override void OnClose() { Closes++; Window.Events.Add("typed.close"); }
            protected override void OnDestroy() { Destroys++; Window.Events.Add("typed.destroy"); }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EasyFramework.Profiler;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EasyFramework.Tests
{
    public class ProfilerInputTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly List<Behaviour> _existingPanels = new();
        private readonly List<RaycastResult> _hits = new();
        private EventSystem _eventSystem;
        private ProfilerManager _manager;
        private Component _inputBlocker;
        private GameObject _cachedCanvas;
        private GameObject _cachedRaycastTarget;
        private Button _button;
        private ProfilerSettings _settings;
        private bool _autoInitialize, _profilerSetting, _errorSetting, _multipleCameraSetting;
        private bool _oldStateCaptured, _createdManager;
        private int _clicks;
        private bool _wasEnabled, _blockUIInput;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            _settings = ProfilerSettings.Instance;
            _autoInitialize = _settings.autoInitialize;
            _profilerSetting = _settings.profilerGUIBehaviour;
            _errorSetting = _settings.errorGUIBehaviour;
            _multipleCameraSetting = _settings.multipleCameraDebugBehaviour;
            _blockUIInput = _settings.blockUIInput;
            _oldStateCaptured = true;

            _settings.autoInitialize = false;
            _createdManager = !ProfilerManager.HasInstance();
            if (_createdManager)
            {
                _settings.profilerGUIBehaviour = false;
                _settings.errorGUIBehaviour = false;
                _settings.multipleCameraDebugBehaviour = false;
            }
            _settings.blockUIInput = true;
            _manager = ProfilerManager.CreateInstance();
            Pause(Object.FindObjectsOfType<ProfilerGUIBehaviour>());
            Pause(Object.FindObjectsOfType<ErrorGUIBehaviour>());
            yield return null;

            _eventSystem = F.WindowManager.EventSystem;
            _wasEnabled = _eventSystem.enabled;
            var canvas = Create<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            var obj = new GameObject("UnderlyingButton", typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            _button = obj.GetComponent<Button>();
            _button.onClick.AddListener(() => _clicks++);
            for (int frame = 0; frame < 60; frame++)
            {
                CollectRaycasts(-1);
                if (_hits.Count > 0 && _hits[0].gameObject == _button.gameObject) break;
                yield return null;
            }
            AssertBlocked(false);

            // Prove both native canvases can be hit before testing cached activation changes.
            var probe = Create<ProfilerGUIBehaviour>();
            SetDetails(probe, true);
            for (int frame = 0; frame < 60; frame++)
            {
                CollectRaycasts(-1);
                if (_hits.Count > 0 && _hits[0].gameObject != _button.gameObject) break;
                yield return null;
            }
            var blockerType = typeof(ProfilerManager).Assembly.GetType("EasyFramework.Profiler.ProfilerInputBlocker");
            _inputBlocker = _manager.GetComponent(blockerType);
            Assert.That(_inputBlocker, Is.Not.Null);
            _cachedCanvas = _inputBlocker.transform.Find("ProfilerInputBlockerCanvas").gameObject;
            Assert.That(_cachedCanvas, Is.Not.Null);
            _cachedRaycastTarget = _cachedCanvas.transform.Find("RaycastTarget").gameObject;
            Assert.That(_cachedRaycastTarget, Is.Not.Null);
            SetDetails(probe, false);
            Object.DestroyImmediate(probe);
            AssertBlocked(false);
        }

        [TearDown]
        public void Cleanup()
        {
            for (int index = _objects.Count - 1; index >= 0; index--)
                if (_objects[index] != null) Object.DestroyImmediate(_objects[index]);
            _objects.Clear();
            _eventSystem.enabled = _wasEnabled;
            foreach (var panel in _existingPanels)
                if (panel != null) panel.enabled = true;
            _existingPanels.Clear();
            if (_oldStateCaptured)
            {
                _settings.blockUIInput = _blockUIInput;
                _settings.autoInitialize = _autoInitialize;
                if (_createdManager)
                {
                    _settings.profilerGUIBehaviour = _profilerSetting;
                    _settings.errorGUIBehaviour = _errorSetting;
                    _settings.multipleCameraDebugBehaviour = _multipleCameraSetting;
                }
                _oldStateCaptured = false;
            }
            _clicks = 0;
        }

        [Test]
        public void BlockingDisabled_VisiblePanelsDoNotInterceptPointer()
        {
            _settings.blockUIInput = false;
            var profiler = Create<ProfilerGUIBehaviour>();
            var errors = Create<ErrorGUIBehaviour>();
            SetDetails(profiler, true);
            errors.AddLog("unblocked error", "stack", LogType.Error);

            AssertBlocked(false);
            Assert.That(_cachedCanvas.activeSelf, Is.True);
            Assert.That(_cachedRaycastTarget.GetComponent<Image>().raycastTarget, Is.False);
            Click(-1);
            Assert.That(_clicks, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator BlockingSetting_ChangesVisiblePanelsOnNextUpdate()
        {
            _settings.blockUIInput = false;
            var profiler = Create<ProfilerGUIBehaviour>();
            var errors = Create<ErrorGUIBehaviour>();
            SetDetails(profiler, true);
            errors.AddLog("setting error", "stack", LogType.Error);
            AssertBlocked(false);

            _settings.blockUIInput = true;
            yield return null;
            Assert.That(AssertBlocked(true), Is.SameAs(_cachedRaycastTarget));
            _settings.blockUIInput = false;
            yield return null;
            AssertBlocked(false);
            Assert.That(_cachedCanvas.activeSelf, Is.True);
            Assert.That(_cachedRaycastTarget.GetComponent<Image>().raycastTarget, Is.False);
        }

        [Test]
        public void RepeatedVisibilityChanges_ReuseCachedBlocker()
        {
            var profiler = Create<ProfilerGUIBehaviour>();
            for (int iteration = 0; iteration < 4; iteration++)
            {
                SetDetails(profiler, true);
                Assert.That(AssertBlocked(true), Is.SameAs(_cachedRaycastTarget));
                SetDetails(profiler, false);
                AssertBlocked(false);
                Assert.That(_cachedCanvas != null, Is.True);
                Assert.That(_cachedCanvas.activeSelf, Is.True);
                Assert.That(_cachedRaycastTarget.GetComponent<Image>().raycastTarget, Is.False);
            }
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void VisibleProfiler_RaycastInterceptsPointerClick_AndLastReleaseRestoresButton(int pointerId)
        {
            var profiler = Create<ProfilerGUIBehaviour>();
            SetDetails(profiler, true);
            var hit = AssertBlocked(true);
            var image = hit.GetComponent<Image>();
            Assert.That(image, Is.Not.Null);
            Assert.That(image.color.a, Is.Zero, "The blocker must be invisible.");
            Assert.That(image.raycastTarget, Is.True);
            Assert.That(image.canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
            Click(pointerId);
            Assert.That(_clicks, Is.Zero);

            SetDetails(profiler, false);
            AssertBlocked(false);
            Click(pointerId);
            Assert.That(_clicks, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ProfilerLifecycle_ControlsOnlyItsBlocker_AndPreservesEventSystem(bool inputEnabled)
        {
            _eventSystem.enabled = inputEnabled;
            var profiler = Create<ProfilerGUIBehaviour>();
            AssertBlocked(false);
            SetDetails(profiler, true);
            AssertBlocked(true);
            AssertInput(inputEnabled);
            profiler.enabled = false;
            AssertBlocked(false);
            profiler.enabled = true;
            AssertBlocked(true);
            SetDetails(profiler, false);
            AssertBlocked(false);
            AssertInput(inputEnabled);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ErrorCloseAndClear_KeepBlockerUntilLastVisibleLog(bool inputEnabled)
        {
            _eventSystem.enabled = inputEnabled;
            var errors = Create<ErrorGUIBehaviour>();
            AssertBlocked(false);
            errors.AddLog("first error", "stack", LogType.Error);
            errors.AddLog("second error", "stack", LogType.Exception);
            AssertBlocked(true);
            Invoke(errors, "OnCloseLog");
            AssertBlocked(true);
            Invoke(errors, "OnCloseLog");
            AssertBlocked(false);
            errors.AddLog("clear error", "stack", LogType.Error);
            AssertBlocked(true);
            Invoke(errors, "OnClearLog");
            AssertBlocked(false);
            AssertInput(inputEnabled);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ProfilerAndErrors_ReleaseOnlyTheirOwnBlocker(bool inputEnabled)
        {
            _eventSystem.enabled = inputEnabled;
            var profiler = Create<ProfilerGUIBehaviour>();
            var errors = Create<ErrorGUIBehaviour>();
            SetDetails(profiler, true);
            var sharedBlocker = AssertBlocked(true);
            errors.AddLog("visible error", "stack", LogType.Error);
            Assert.That(AssertBlocked(true), Is.SameAs(sharedBlocker));
            SetDetails(profiler, false);
            AssertBlocked(true);
            errors.enabled = false;
            AssertBlocked(false);
            errors.enabled = true;
            AssertBlocked(true);
            Object.DestroyImmediate(errors);
            AssertBlocked(false);
            AssertInput(inputEnabled);
        }

        [Test]
        public void TwoProfilers_RepeatedVisibilityChangesDoNotReleaseOtherOwner()
        {
            var first = Create<ProfilerGUIBehaviour>();
            var second = Create<ProfilerGUIBehaviour>();
            SetDetails(first, true);
            SetDetails(first, true);
            SetDetails(second, true);
            SetDetails(first, false);
            SetDetails(first, false);
            AssertBlocked(true);
            Object.DestroyImmediate(second);
            AssertBlocked(false);
        }

        [UnityTest]
        public IEnumerator ExpandedProfiler_DestroyReleasesBlocker_WithoutChangingEventSystem()
        {
            foreach (bool inputEnabled in new[] { false, true })
            {
                _eventSystem.enabled = inputEnabled;
                var profiler = Create<ProfilerGUIBehaviour>();
                SetDetails(profiler, true);
                AssertBlocked(true);
                Object.Destroy(profiler);
                yield return null;
                AssertBlocked(false);
                AssertInput(inputEnabled);
            }
        }

        [UnityTest]
        public IEnumerator VisibleErrors_DestroyReleasesBlocker_WithoutChangingEventSystem()
        {
            foreach (bool inputEnabled in new[] { false, true })
            {
                _eventSystem.enabled = inputEnabled;
                var errors = Create<ErrorGUIBehaviour>();
                errors.AddLog("destroy error", "stack", LogType.Error);
                AssertBlocked(true);
                Object.Destroy(errors);
                yield return null;
                AssertBlocked(false);
                AssertInput(inputEnabled);
            }
        }

        [UnityTest]
        public IEnumerator VisiblePanels_DoNotOverrideBusinessEventSystemChanges()
        {
            var profiler = Create<ProfilerGUIBehaviour>();
            var errors = Create<ErrorGUIBehaviour>();
            SetDetails(profiler, true);
            errors.AddLog("business input error", "stack", LogType.Error);
            foreach (bool inputEnabled in new[] { false, true })
            {
                _eventSystem.enabled = inputEnabled;
                yield return null;
                AssertBlocked(true);
                AssertInput(inputEnabled);
            }
            SetDetails(profiler, false);
            Invoke(errors, "OnClearLog");
            AssertBlocked(false);
            AssertInput(true);
        }

        private GameObject AssertBlocked(bool expected)
        {
            var target = Raycast(-1).gameObject;
            Assert.That(target == _button.gameObject, Is.EqualTo(!expected),
                (expected ? "The private overlay must receive the first raycast. " : "The last release must restore the underlying button raycast. ") + RaycastDiagnostics());
            return target;
        }

        private RaycastResult Raycast(int pointerId)
        {
            CollectRaycasts(pointerId);
            Assert.That(_hits, Is.Not.Empty, "The test canvas must be raycastable. " + RaycastDiagnostics());
            return _hits[0];
        }

        private void CollectRaycasts(int pointerId)
        {
            Canvas.ForceUpdateCanvases();
            _hits.Clear();
            _eventSystem.RaycastAll(Pointer(pointerId), _hits);
        }

        private string RaycastDiagnostics()
        {
            var image = _button.GetComponent<Image>();
            var canvas = image.canvas;
            var rect = image.rectTransform;
            var blocker = _manager.GetComponent("EasyFramework.Profiler.ProfilerInputBlocker");
            var mask = blocker == null ? null : blocker.transform.Find("ProfilerInputBlockerCanvas");
            var maskCanvas = mask == null ? null : mask.GetComponent<Canvas>();
            var maskImage = mask == null ? null : mask.GetComponentInChildren<Image>();
            var maskRaycaster = mask == null ? null : mask.GetComponent<GraphicRaycaster>();
            var directHits = new List<RaycastResult>();
            maskRaycaster?.Raycast(Pointer(-1), directHits);
            int registeredGraphics = maskCanvas == null ? 0 : GraphicRegistry.GetRaycastableGraphicsForCanvas(maskCanvas).Count;
            return $"Screen={Screen.width}x{Screen.height}; graphics={SystemInfo.graphicsDeviceType}; " +
                $"canvasActive={canvas.isActiveAndEnabled}; pixelRect={canvas.pixelRect}; rootRect={((RectTransform)canvas.transform).rect}; " +
                $"imageActive={image.isActiveAndEnabled}; rect={rect.rect}; position={rect.position}; scale={rect.lossyScale}; " +
                $"depth={image.depth}; cull={image.canvasRenderer.cull}; rendererCount={image.canvasRenderer.materialCount}; " +
                $"raycastTarget={image.raycastTarget}; rectHit={RectTransformUtility.RectangleContainsScreenPoint(rect, Pointer(-1).position, null)}; " +
                $"hits={_hits.Count}; first={(_hits.Count == 0 ? "none" : _hits[0].gameObject.name)}; " +
                $"maskActive={mask != null && mask.gameObject.activeInHierarchy}; maskRect={(mask == null ? default : ((RectTransform)mask).rect)}; " +
                $"maskCanvasEnabled={(maskCanvas != null && maskCanvas.isActiveAndEnabled)}; maskCanvasRoot={(maskCanvas == null ? "none" : maskCanvas.rootCanvas.name)}; " +
                $"maskImageEnabled={(maskImage != null && maskImage.isActiveAndEnabled)}; maskImageCanvas={(maskImage == null || maskImage.canvas == null ? "none" : maskImage.canvas.name)}; " +
                $"maskDepth={(maskImage == null ? -1 : maskImage.depth)}; maskCull={(maskImage == null || maskImage.canvasRenderer.cull)}; " +
                $"maskSorting={(maskCanvas == null ? -1 : maskCanvas.sortingOrder)}; maskOverride={(maskCanvas != null && maskCanvas.overrideSorting)}; " +
                $"registered={registeredGraphics}; directHits={directHits.Count}; raycasterEnabled={(maskRaycaster != null && maskRaycaster.isActiveAndEnabled)}";
        }

        private PointerEventData Pointer(int pointerId) => new(_eventSystem)
        {
            pointerId = pointerId,
            position = new Vector2(Screen.width / 2f, Screen.height / 2f),
            button = PointerEventData.InputButton.Left,
        };

        private void Click(int pointerId)
        {
            var hit = Raycast(pointerId);
            ExecuteEvents.ExecuteHierarchy(hit.gameObject, Pointer(pointerId), ExecuteEvents.pointerClickHandler);
        }

        private void AssertInput(bool expected) => Assert.That(_eventSystem.enabled, Is.EqualTo(expected),
            "Diagnostic overlays must not change the business EventSystem switch.");

        private T Create<T>() where T : Component
        {
            var obj = new GameObject(typeof(T).Name + "-input-test", typeof(RectTransform));
            _objects.Add(obj);
            return obj.AddComponent<T>();
        }

        private void Pause<T>(T[] panels) where T : Behaviour
        {
            foreach (var panel in panels)
            {
                if (!panel.enabled) continue;
                _existingPanels.Add(panel);
                panel.enabled = false;
            }
        }

        private static void SetDetails(ProfilerGUIBehaviour panel, bool visible) => Invoke(panel, "SetDetailsVisible", visible);

        private static void Invoke(object target, string method, params object[] arguments)
        {
            var member = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(member, Is.Not.Null, "Missing diagnostic panel operation: " + method);
            member.Invoke(target, arguments);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework.Profiler
{
    internal sealed class ProfilerInputBlocker : MonoBehaviour
    {
        private readonly HashSet<MonoBehaviour> _owners = new();
        private GameObject _raycastCanvas;
        private Image _raycastTarget;

        private void OnEnable() => Refresh();

        private void OnDisable() { if (_raycastTarget != null) _raycastTarget.raycastTarget = false; }

        private void Awake()
        {
            _raycastCanvas = new GameObject("ProfilerInputBlockerCanvas", typeof(RectTransform),
                typeof(Canvas), typeof(GraphicRaycaster));
            _raycastCanvas.transform.SetParent(transform, false);

            var canvas = _raycastCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = short.MaxValue;
            var raycastTarget = new GameObject("RaycastTarget", typeof(RectTransform), typeof(Image));
            raycastTarget.transform.SetParent(_raycastCanvas.transform, false);
            var rectTransform = (RectTransform)raycastTarget.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            _raycastTarget = raycastTarget.GetComponent<Image>();
            _raycastTarget.color = Color.clear;
            _raycastTarget.raycastTarget = false;
            _raycastTarget.canvasRenderer.cullTransparentMesh = false;
        }

        internal void SetOwner(MonoBehaviour owner, bool visible)
        {
            if (visible) _owners.Add(owner);
            else _owners.Remove(owner);
            Refresh();
        }

        internal void Refresh()
        {
            if (_raycastCanvas == null) return;
            bool active = isActiveAndEnabled && _owners.Count > 0 && ProfilerSettings.Instance.blockUIInput;
            if (_raycastTarget.raycastTarget == active) return;
            _raycastTarget.raycastTarget = active;
            if (active) Canvas.ForceUpdateCanvases();
        }
    }
}

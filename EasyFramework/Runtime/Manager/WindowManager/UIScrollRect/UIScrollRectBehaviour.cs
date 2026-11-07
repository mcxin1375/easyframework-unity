/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public class UIScrollRectBehaviour : WindowBehaviour
    {
        public Func<int, Vector2> GetItemSizeDelegate;
        public Func<int, Vector2> GetItemPositionDelegate;

        public bool HasSizeDelegate => GetItemSizeDelegate != null;
        public bool HasPositionDelegate => GetItemPositionDelegate != null;
        
        public GridLayoutGroup.Axis direction = GridLayoutGroup.Axis.Vertical;

        [Header("Grid Behaviour")]
        [SerializeField] private UIGridBehaviour grid;
        public UIGridBehaviour Grid
        {
            get
            {
                if (grid == null) grid = gameObject.GetComponentInChildren<UIGridBehaviour>();
                return grid;
            }
        }
        
        [SerializeField] private float scrollTime = 0.25f;

        private ScrollRect _scrollRect;
        private Vector2 _scrollStart;
        private Vector2 _scrollTarget;
        private float _scrollTimeAdd;
        private float _scrollDuration;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        private void Update()
        {
            if (_scrollDuration <= 0f) return;

            _scrollTimeAdd += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(_scrollTimeAdd / _scrollDuration);
            var easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            _scrollRect.normalizedPosition = Vector2.LerpUnclamped(_scrollStart, _scrollTarget, easedProgress);
            if (progress < 1f) return;

            _scrollDuration = 0f;
        }

        public void ScrollTo(Vector2 normalizedPosition) => ScrollTo(normalizedPosition, scrollTime);
        public void ScrollTo(Vector2 normalizedPosition, float duration)
        {
            StopScroll();

            var targetPosition = new Vector2(
                Mathf.Clamp01(normalizedPosition.x),
                Mathf.Clamp01(normalizedPosition.y));
            var startPosition = _scrollRect.normalizedPosition;
            if (duration <= 0f || Vector2.SqrMagnitude(startPosition - targetPosition) <= 0.000001f)
            {
                _scrollRect.normalizedPosition = targetPosition;
                return;
            }

            _scrollStart = startPosition;
            _scrollTarget = targetPosition;
            _scrollTimeAdd = 0f;
            _scrollDuration = duration;
        }

        public void ScrollTo(int index) => ScrollTo(index, scrollTime);
        public void ScrollTo(int index, float duration)
        {
            StopScroll();

            var grid = Grid;
            var list = grid == null ? null : grid.UIScrollRectList;
            var scrollRect = _scrollRect;
            var content = scrollRect?.content;
            var viewport = scrollRect?.viewport != null ? scrollRect.viewport : GetComponent<RectTransform>();
            if (list == null || index < 0 || index >= list.ItemNumber || content == null || viewport == null) return;

            grid.RefreshItemPos(list.ItemNumber);
            var itemPosition = grid.GetItemPos(index);
            var itemSize = grid.GetItemSize(index);
            var targetPosition = scrollRect.normalizedPosition;
            var needScroll = false;

            var hiddenWidth = content.rect.width - viewport.rect.width;
            if (scrollRect.horizontal && hiddenWidth > 0f)
            {
                var offset = Mathf.Clamp(-content.anchoredPosition.x, 0f, hiddenWidth);
                var itemLeft = itemPosition.x - itemSize.x * 0.5f;
                var itemRight = itemPosition.x + itemSize.x * 0.5f;
                var targetOffset = itemLeft < offset
                    ? itemLeft
                    : itemRight > offset + viewport.rect.width ? itemRight - viewport.rect.width : offset;
                targetOffset = Mathf.Clamp(targetOffset, 0f, hiddenWidth);
                needScroll = !Mathf.Approximately(targetOffset, offset);
                targetPosition.x = targetOffset / hiddenWidth;
            }

            var hiddenHeight = content.rect.height - viewport.rect.height;
            if (scrollRect.vertical && hiddenHeight > 0f)
            {
                var offset = Mathf.Clamp(content.anchoredPosition.y, 0f, hiddenHeight);
                var itemTop = -itemPosition.y - itemSize.y * 0.5f;
                var itemBottom = -itemPosition.y + itemSize.y * 0.5f;
                var targetOffset = itemTop < offset
                    ? itemTop
                    : itemBottom > offset + viewport.rect.height ? itemBottom - viewport.rect.height : offset;
                targetOffset = Mathf.Clamp(targetOffset, 0f, hiddenHeight);
                needScroll |= !Mathf.Approximately(targetOffset, offset);
                targetPosition.y = 1f - targetOffset / hiddenHeight;
            }

            if (needScroll) ScrollTo(targetPosition, duration);
        }

        public void StopScroll()
        {
            _scrollDuration = 0f;
            _scrollRect?.StopMovement();
        }
        
        internal void RefreshOnEditorMode()
        {
            InitBaseProperties();
            RefreshBaseProperties();
        }
        private void InitBaseProperties()
        {
            if (grid == null) grid = GetComponentInChildren<UIGridBehaviour>();
            if (grid == null)
            {
                var gridRect = transform.childCount > 0 ? transform.GetChild(0).GetComponent<RectTransform>() : null;
                if (gridRect == null)
                {
                    var go = new GameObject("Grid");
                    gridRect = go.AddComponentEx<RectTransform>();
                    gridRect.SetParent(transform);
                    gridRect.gameObject.layer = transform.gameObject.layer;
                    gridRect.localScale = Vector3.one;
                    gridRect.rotation = Quaternion.identity;
                    gridRect.anchoredPosition3D = Vector3.zero;
                }
                grid = gridRect.gameObject.AddComponentEx<UIGridBehaviour>();
                grid.enabled = true;
            }
        }
        private void RefreshBaseProperties()
        {
            var scrollRect = GetComponent<ScrollRect>();
            scrollRect.horizontal = direction == GridLayoutGroup.Axis.Horizontal;
            scrollRect.vertical = direction == GridLayoutGroup.Axis.Vertical;
            
            if (grid == null) grid = GetComponentInChildren<UIGridBehaviour>();
            if (grid == null) return;

            scrollRect.content = grid.rectTransform;
            
            grid.RefreshOnEditorMode();
        }
    }
}

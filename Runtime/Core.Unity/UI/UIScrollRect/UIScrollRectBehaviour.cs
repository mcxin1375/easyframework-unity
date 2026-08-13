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
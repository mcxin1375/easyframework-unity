/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIGridBehaviour : UIBaseBehaviour
    {
        public RectOffset padding = new RectOffset();
        public Vector2 cellSize = new Vector2(100, 100);
        public Vector2 spacing = Vector2.zero;

        public Vector2 CellSize
        {
            get
            {
                var v = cellSize;
                if (v.x == 0) v.x = SRBehaviour.rectTransform.rect.width;
                if (v.y == 0) v.y = SRBehaviour.rectTransform.rect.height;
                return v;
            }
        }

        public GridLayoutGroup.Corner startCorner = GridLayoutGroup.Corner.UpperLeft;
        public GridLayoutGroup.Axis startAxis = GridLayoutGroup.Axis.Vertical;
        public TextAnchor childAlignment = TextAnchor.UpperLeft;
        
        [Header("SRBehaviour & BaseItem")]
        [SerializeField] private UIScrollRectBehaviour srBehaviour;
        public UIScrollRectBehaviour SRBehaviour
        {
            get
            {
                if (srBehaviour == null) srBehaviour = gameObject.GetComponentInParent<UIScrollRectBehaviour>(true);
                return srBehaviour;
            }
        }
        public RectTransform baseItem;
        public int rows;
        public int columns;
        public int startIndex;
        public int endIndex;

        [Header("Dynamic Item Size")]
        public bool dynamicMode;
        
        [Header("Editor Debug")]
        [Range(0, 100)]
        public int childCount = 0;
        public bool displayAll;

        [SerializeField] private List<Vector2> itemSizeList = new();
        [SerializeField] private List<Vector2> itemPosList = new();

        internal IUIScrollRectList UIScrollRectList { get; set; }

        private Vector2 _gridPos;
        
        private void Awake()
        {
            baseItem.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (UIScrollRectList == null) return;

            if (TryUpdateItemIndex(UIScrollRectList.ItemNumber))
            {
                UIScrollRectList.Refresh(false);
            }
        }

        public void RefreshItemPos(int itemNumber, bool forceUpdate = true)
        {
            if (itemNumber == 0)
            {
                startIndex = 0;
                endIndex = -1;
                var parent = transform.parent as RectTransform;
                rectTransform.sizeDelta = parent.rect.size;
                return;
            }

            if (forceUpdate)
            {
                itemSizeList.Clear();
                itemPosList.Clear();
            }

            if (dynamicMode)
            {
                this.CalculateDynamicItemSize(itemSizeList, itemNumber);
                this.CalculateDynamicItemPosition(itemPosList, itemNumber, out var rect);
                this.CalculateDynamicItemDisplayRange(itemNumber, out startIndex, out endIndex);
                rectTransform.sizeDelta = rect;
            }
            else
            {
                this.CalculateFixedItemPosition(itemPosList, itemNumber, out var rect, out rows, out columns);
                this.CalculateFixedItemDisplayRange(itemNumber, out startIndex, out endIndex);
                rectTransform.sizeDelta = rect;
                // Debug.Log($"StartIndex: {startIndex}, EndIndex: {endIndex},  itemNumber: {itemNumber}, rows: {rows}, columns: {columns}, {SRBehaviour.rectTransform.rect.size}");
            }
        }
        public bool TryUpdateItemIndex(int itemNumber)
        {
            if (itemNumber == 0) return false;
            if (_gridPos == rectTransform.anchoredPosition) return false;
            _gridPos = rectTransform.anchoredPosition;
            
            var preStartIndex = startIndex;
            var preEndIndex = endIndex;

            if (dynamicMode)
            {
                this.CalculateDynamicItemDisplayRange(itemNumber, out startIndex, out endIndex);
            }
            else
            {
                this.CalculateFixedItemDisplayRange(itemNumber, out startIndex, out endIndex);
            }
            
            // Debug.Log($"preStartIndex: {preStartIndex}, preEndIndex: {preEndIndex}, startIndex: {startIndex}, endIndex: {endIndex}");
            return preStartIndex != startIndex || preEndIndex != endIndex;
        }

        public Vector2 GetItemPos(int index)
        {
            if (index < 0 || index >= itemPosList.Count) return Vector2.zero;
            return itemPosList[index];
        }
        public Vector2 GetItemSize(int index)
        {
            if (!dynamicMode) return CellSize;
            if (index < 0 || index >= itemSizeList.Count) return CellSize;
            return itemSizeList[index];
        }
        
        
        internal void RefreshOnEditorMode()
        {
            if (Application.isPlaying) return;

            InitBaseProperties();
            RefreshOnEditor(childCount);
            SyncChildren();
        }
        private void InitBaseProperties()
        {
            baseItem = transform.childCount > 0 ? transform.GetChild(0).GetComponent<RectTransform>() : null;
            if (baseItem == null)
            {
                var go = new GameObject("Item");
                baseItem = go.AddComponentEx<RectTransform>();
                baseItem.SetParent(transform);
                baseItem.gameObject.AddComponent<Image>();
                baseItem.gameObject.layer = transform.gameObject.layer;
            }
            baseItem.localScale = Vector3.one;
            baseItem.rotation = Quaternion.identity;
            baseItem.anchoredPosition3D = Vector3.zero;
            baseItem.anchorMin = new Vector2(0, 1);
            baseItem.anchorMax = new Vector2(0, 1);
            baseItem.pivot = new Vector2(0.5f, 0.5f);

            srBehaviour = gameObject.GetComponentInParent<UIScrollRectBehaviour>(true);
            if (srBehaviour != null)
            {
                switch (srBehaviour.direction)
                {
                    case GridLayoutGroup.Axis.Horizontal:
                        startAxis = GridLayoutGroup.Axis.Vertical;
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                        rectTransform.anchorMin = new Vector2(0, 0.5f);
                        rectTransform.anchorMax = new Vector2(0, 0.5f);
                        rectTransform.pivot = new Vector2(0, 0.5f);
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0);
                        break;
                    case GridLayoutGroup.Axis.Vertical:
                        startAxis = GridLayoutGroup.Axis.Horizontal;
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                        rectTransform.anchorMin = new Vector2(0.5f, 1);
                        rectTransform.anchorMax = new Vector2(0.5f, 1);
                        rectTransform.pivot = new Vector2(0.5f, 1);
                        rectTransform.anchoredPosition = new Vector2(0, rectTransform.anchoredPosition.y);
                        break;
                }
            }
        }
        private void SyncChildren()
        {
            for (int i = childCount + 1; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).gameObject;
                DestroyImmediate(child);
            }
        }
        private void RefreshOnEditor(int num)
        {
            if (baseItem == null) return;
            baseItem.gameObject.SetActive(num == 0);
            baseItem.sizeDelta = CellSize;
            
            itemPosList.Clear();
            var posNum = Mathf.Max(1, num);
            RefreshItemPos(posNum, false);
            
            baseItem.anchoredPosition = GetItemPos(0);
            for (int i = 0; i < num; i++)
            {
                var index = i + 1;
                var item = index < transform.childCount ? transform.GetChild(index) : Instantiate(baseItem.gameObject, transform).transform;
                var itemRect = item as RectTransform;
                itemRect.name = $"Item: {i}";
                itemRect.anchoredPosition = GetItemPos(i);
                itemRect.sizeDelta = GetItemSize(i);

                var active = displayAll || (i >= startIndex && i <= endIndex);
                item.gameObject.SetActive(active);
            }
        }
    }
}
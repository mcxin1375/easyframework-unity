/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EasyFramework
{
    public interface IUIScrollRectList
    {
        int ItemNumber { get; }

        void Refresh(bool forceUpdate = true);
        void SelectAt(int index);
        void OnItemButtonClick(Button button, int index);
    }

    public class UIScrollRectList<TItem, TData> : WindowComponent, IUIScrollRectList where TItem : UIGridItemBehaviour<TData>
    {
        public Action<TItem> OnItemSelected;
        public Action<TItem> OnItemPress;
        public Action<Button, int> OnItemButtonAction;
        
        public Func<int, Vector2> GetItemSizeDelegate;
        public Func<int, Vector2> GetItemPositionDelegate;

        public int SelectIndex { get; private set; } = -1;
        public int ItemNumber => _dataList?.Count ?? 0;
        public IReadOnlyList<TData> DataList => _dataList;
        private List<TData> _dataList;

        private readonly Dictionary<int, TItem> _itemDict = new();
        private readonly Queue<TItem> _itemPool = new();
        private readonly Queue<int> _recycleList = new();

        private UIScrollRectBehaviour _behaviour;
        private UIGridBehaviour _gridBehaviour;

        protected override void OnDestroy()
        {
            foreach (var item in _itemDict.Values) item.Destroy();
            foreach (var item in _itemPool) item.Destroy();
            
            _itemDict.Clear();
            _itemPool.Clear();
            _recycleList.Clear();
            _dataList.Clear();
            _dataList = null;
        }
        
        public void Initialize(UIScrollRectBehaviour behaviour)
        {
            _behaviour = behaviour;
            _gridBehaviour = _behaviour.Grid;
            _gridBehaviour.UIScrollRectList = this;
        }

        public void Refresh(List<TData> list)
        {
            _dataList = list;
            Refresh();
        }
        public void Refresh(List<TData> list, int selectIndex)
        {
            Refresh(list);
            SelectAt(selectIndex);
        }
        public void Refresh(TData[] array)
        {
            _dataList ??= new();
            _dataList.Clear();
            _dataList.AddRange(array);
            Refresh();
        }
        public void Refresh(TData[] array, int selectIndex)
        {
            Refresh(array);
            SelectAt(selectIndex);
        }

        public void UnSelectAll() => SelectAt(-1);
        public void SelectAt(int index)
        {
            if (_itemDict.TryGetValue(SelectIndex, out var preItem)) preItem.SetSelect(false);
            SelectIndex = index;
            if (_itemDict.TryGetValue(SelectIndex, out var item)) item.SetSelect(true);
            
            OnItemSelected?.Invoke(item);
        }

        public void OnItemButtonClick(Button button, int index)
        {
            SelectAt(index);
            OnItemButtonAction?.Invoke(button, index);
        }

        public TData GetData(int index)
        {
            if (_dataList == null) return default;
            if (index < 0 || index >= _dataList.Count) return default;
            return _dataList[index];
        }

        public void Refresh(bool forceRefresh = true)
        {
            _gridBehaviour.RefreshItemPos(ItemNumber);
            
            foreach (var key in _itemDict.Keys)
            {
                if (key < _gridBehaviour.startIndex || key > _gridBehaviour.endIndex) _recycleList.Enqueue(key);
            }
            while (_recycleList.Count > 0)
            {
                var key = _recycleList.Dequeue();
                if (_itemDict.Remove(key, out var item))
                {
                    _itemPool.Enqueue(item);
                    item.SetActive(false);
                }
            }
            
            // Debug.Log($"{rectTransform.name} - {Grid.startIndex}, {Grid.endIndex}, ItemNumber: {ItemNumber}");
            
            for (int i = _gridBehaviour.startIndex; i <= _gridBehaviour.endIndex; i++)
            {
                if (i >= ItemNumber) break;

                if (!_itemDict.TryGetValue(i, out var item))
                {
                    item = Take();
                    _itemDict.Add(i, item);
                    item.name = $"Item: {i}";
                    item.rectTransform.anchoredPosition = _gridBehaviour.GetItemPos(i);
                    item.rectTransform.sizeDelta = _gridBehaviour.GetItemSize(i);
                    item.SetActive(true);
                    item.Refresh(GetData(i), i, SelectIndex == i);
                }
                else if (forceRefresh)
                {
                    item.rectTransform.anchoredPosition = _gridBehaviour.GetItemPos(i);
                    item.rectTransform.sizeDelta = _gridBehaviour.GetItemSize(i);
                    item.Refresh(GetData(i), i, SelectIndex == i);
                }
            }
        }
        
        private TItem Take()
        {
            if (_itemPool.Count > 0) return _itemPool.Dequeue();
            var go = Object.Instantiate(_gridBehaviour.baseItem, _gridBehaviour.rectTransform).gameObject;
            var behaviour = go.AddComponentEx<TItem>();
            behaviour.Create(this);
            return behaviour;
        }
    }
}
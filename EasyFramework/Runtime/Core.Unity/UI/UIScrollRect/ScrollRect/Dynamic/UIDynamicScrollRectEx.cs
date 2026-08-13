// /*----------------------------------------------------------------
// // author:Cookie mcx
// // date:2023/4/14
// // describe:无线滚动列表（动态尺寸格子，只支持一行一个格子）
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.UI;
// using Object = UnityEngine.Object;
//
// namespace EasyFramework
// {
//     /// <summary>
//     /// 动态无线滚动列表，Grid只支持GridLayoutGroup，其它的懒得支持
//     /// 需要实现格子尺寸计算
//     /// </summary>
//     /// <typeparam name="K">Item class</typeparam>
//     /// <typeparam name="T">Data class</typeparam>
//     public class UIDynamicScrollRectEx<K,T> where K: UIDynamicScrollRectItemEx<T>, new()
//     {
//
//         public delegate Vector2 CalculateItemSizeDelegate(int index, T t);
//         
//         public RectTransform RectTrScrollRect { get; private set; }
//         public RectTransform Grid { get; private set; }
//         public RectTransform Item { get; private set; }
//
//         public ScrollRectEx ScrollRectEx { get; private set; }
//         public GridLayoutGroup GridLayoutGroup { get; private set; }
//
//         
//         public int SelectIndex { get; private set; }
//         public K SelectItem => GetItem(SelectIndex);
//         public T[] Datas { get; private set; }
//         
//         private bool IsVertical => GridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount;
//
//         private readonly Dictionary<int, K> _itemDict = new Dictionary<int, K>();
//         private readonly Action<K, object[]> _itemAction;
//
//
//         private Vector3 _gridAnchoredPosition;
//         
//         private Vector2[] _itemPosArr;
//         private Vector2[] _itemSizeArr;
//         private Vector2 _gridSizeDelta = new Vector2();
//         
//         private Queue<K> _itemCache;
//         private CalculateItemSizeDelegate _calculateItemSizeDelegate;
//         private object[] _args;
//
//         private int _minIndex = 0;
//         private int _maxIndex = 0;
//
//         public UIDynamicScrollRectEx(RectTransform rectTr, CalculateItemSizeDelegate calculateItemSizeDelegate, Action<K, object[]> action = null)
//         {
//             RectTrScrollRect = rectTr;
//             _calculateItemSizeDelegate = calculateItemSizeDelegate;
//             _itemAction = action;
//
//             Init();
//         }
//
//         private void Init()
//         {
//             Grid = RectTrScrollRect.GetChild(0) as RectTransform;
//             Item = Grid.GetChild(0) as RectTransform;;
//             
//             Item.gameObject.SetActive(false);
//             GridLayoutGroup = Grid.GetComponent<GridLayoutGroup>();
//             
//             GridLayoutGroup.enabled = false;
//             Item.anchorMin = new Vector2(0, 1);
//             Item.anchorMax = new Vector2(0, 1);
//             Item.pivot = new Vector2(0.5f, 0.5f);
//             Item.sizeDelta = GridLayoutGroup.cellSize;
//
//             _gridAnchoredPosition = Grid.anchoredPosition;
//
//             ScrollRectEx = RectTrScrollRect.GetComponent<ScrollRectEx>();
//             if (ScrollRectEx == null)
//             {
//                 ScrollRect scrollRect = RectTrScrollRect.GetComponent<ScrollRect>();
//                 scrollRect.enabled = false;
//                 
//                 ScrollRectEx = RectTrScrollRect.gameObject.AddComponentEx<ScrollRectEx>();
//                 ScrollRectEx.content = scrollRect.content;
//                 ScrollRectEx.horizontal = scrollRect.horizontal;
//                 ScrollRectEx.vertical = scrollRect.vertical;
//                 ScrollRectEx.velocity = scrollRect.velocity;
//                 ScrollRectEx.scrollSensitivity = scrollRect.scrollSensitivity;
//                 ScrollRectEx.decelerationRate = scrollRect.decelerationRate;
//                 ScrollRectEx.viewport = scrollRect.viewport;
//                 
//                 Object.Destroy(scrollRect);
//             }
//
//             ScrollRectEx.OnUpdateContentPos += OnUpdateContentPos;
//         }
//         
//         public virtual void Dispose()
//         {
//             foreach (K item in _itemDict.Values)
//             {
//                 item.Dispose();
//             }
//             _itemDict.Clear();
//             if (_itemCache?.Count > 0)
//             {
//                 foreach (K item in _itemCache)
//                 {
//                     item.Dispose();
//                 }
//                 _itemCache.Clear();
//             }
//         }
//
//         public virtual K GetItem(int index) => _itemDict.ContainsKey(index) ? _itemDict[index] : null;
//
//         public void Refresh(T[] datas, params object[] args)
//         {
//             Datas = datas;
//             _args = args;
//             OnRefresh();
//         }
//
//         public void RefreshAtIndex(T[] datas, int index, params object[] args)
//         {
//             Refresh(datas, args);
//             SelectAt(index);
//         }
//
//         public void SelectAt(int index, params object[] args)
//         {
//             K k = GetItem(index);
//             k?.SetSelect(false);
//             
//             SelectIndex = index;
//             
//             k = GetItem(index);
//             k?.SetSelect(true);
//             
//             OnItemAction(k, args);
//         }
//
//         protected void Enqueue(int index)
//         {
//             // Log.Info("Enqueue", index);
//             
//             if (_itemDict.ContainsKey(index))
//             {
//                 K k = _itemDict[index];
//                 _itemDict.Remove(index);
//                 Enqueue(k);
//             }
//         }
//
//         protected void Enqueue(K k)
//         {
//             if (k == null) return;
//             k.SetActive(false);
//             
//             _itemCache ??= new Queue<K>();
//             _itemCache.Enqueue(k);
//         }
//         
//         protected K Dequeue()
//         {
//             return _itemCache?.Count > 0 ? _itemCache.Dequeue() : CreateItem();
//         }
//
//         private K CreateItem()
//         {
//             GameObject go = Object.Instantiate(Item.gameObject, Grid);
//             K item = new K();
//             item.Register(go, BindItemAction);
//             return item;
//         }
//         
//         private void OnUpdateContentPos(Vector2 anchoredPosition)
//         {
//             if (Datas?.Length > 0)
//             {
//                 if (IsVertical)
//                 {
//                     if (_gridAnchoredPosition.y < anchoredPosition.y) UpdateIndexUp();
//                     else UpdateIndexDown();
//                 }
//                 else
//                 {
//                     if (_gridAnchoredPosition.x < anchoredPosition.x) UpdateIndexDown();
//                     else UpdateIndexUp();
//                 }
//                 
//                 // UpdateMinIndex();
//                 // UpdateMaxIndex();
//
//                 _minIndex = Mathf.Clamp(_minIndex, 0, Datas?.Length - 1 ?? 0);
//                 _maxIndex = Mathf.Clamp(_maxIndex, 0, Datas?.Length - 1 ?? 0);
//
//                 // Log.Info(_minIndex, _maxIndex);
//                 for (int i = _minIndex; i <= _maxIndex; i++)
//                 {
//                     if (i < 0 || i >= Datas.Length) continue;
//                     OnItemDisplay(i, Datas[i], _args);
//                 }
//             }
//             
//             _gridAnchoredPosition = anchoredPosition;
//         }
//
//         private void UpdateIndexUp()
//         {
//             if (_minIndex == -1) _minIndex = 0;
//             while (true)
//             {
//                 if (_minIndex < 0 || _minIndex >= Datas.Length) break;
//                 bool isDisplay = CheckDisplay(_minIndex, _itemPosArr[_minIndex], _itemSizeArr[_minIndex]);
//                 if (isDisplay) break;
//                 Enqueue(_minIndex);
//                 _minIndex++;
//             }
//
//             _maxIndex = _minIndex + 1;
//             while (true)
//             {
//                 if (_maxIndex < 0 || _maxIndex >= Datas.Length) break;
//                 bool isDisplay = CheckDisplay(_maxIndex, _itemPosArr[_maxIndex], _itemSizeArr[_maxIndex]);
//                 if (!isDisplay)
//                 {
//                     _maxIndex--;
//                     break;
//                 }
//                 _maxIndex++;
//             }
//         }
//
//         private void UpdateIndexDown()
//         {
//             if (_maxIndex == -1) _maxIndex = Datas.Length - 1;
//             while (true)
//             {
//                 if (_maxIndex < 0 || _maxIndex >= Datas.Length) break;
//                 bool isDisplay = CheckDisplay(_maxIndex, _itemPosArr[_maxIndex], _itemSizeArr[_maxIndex]);
//                 if (isDisplay) break;
//                 Enqueue(_maxIndex);
//                 _maxIndex--;
//             }
//
//             _minIndex = _maxIndex - 1;
//             while (true)
//             {
//                 if (_minIndex < 0 || _minIndex >= Datas.Length) break;
//                 bool isDisplay = CheckDisplay(_minIndex, _itemPosArr[_minIndex], _itemSizeArr[_minIndex]);
//                 if (!isDisplay)
//                 {
//                     _minIndex++;
//                     break;
//                 }
//                 _minIndex--;
//             }
//         }
//
//         private void UpdateMinIndex()
//         {
//             if (_minIndex < 0 || _minIndex >= Datas.Length) return;
//             
//             bool isDisplay = CheckDisplay(_minIndex, _itemPosArr[_minIndex], _itemSizeArr[_minIndex]);
//             if (isDisplay)
//             {
//                 while (_minIndex > 0)
//                 {
//                     if (!CheckDisplay(_minIndex - 1, _itemPosArr[_minIndex - 1], _itemSizeArr[_minIndex - 1]))
//                         break;
//                     _minIndex--;
//                     OnItemDisplay(_minIndex, Datas[_minIndex], _args);
//                 }
//             }
//             else
//             {
//                 Enqueue(_minIndex);
//                 _minIndex++;
//                 while (_minIndex < Datas.Length)
//                 {
//                     if (CheckDisplay(_minIndex, _itemPosArr[_minIndex], _itemSizeArr[_minIndex]))
//                     {
//                         OnItemDisplay(_minIndex, Datas[_minIndex], _args);
//                         break;
//                     }
//                     Enqueue(_minIndex);
//                     _minIndex++;
//                 }
//             }
//         }
//
//         private void UpdateMaxIndex()
//         {
//             if (_maxIndex < 0 || _maxIndex >= Datas.Length) return;
//             
//             bool isDisplay = CheckDisplay(_maxIndex, _itemPosArr[_maxIndex], _itemSizeArr[_maxIndex]);
//             if (isDisplay)
//             {
//                 while (_maxIndex < Datas.Length - 1)
//                 {
//                     if (!CheckDisplay(_maxIndex + 1, _itemPosArr[_maxIndex + 1], _itemSizeArr[_maxIndex + 1]))
//                         break;
//                     _maxIndex++;
//                     OnItemDisplay(_maxIndex, Datas[_maxIndex], _args);
//                 }
//             }
//             else
//             {
//                 Enqueue(_maxIndex);
//                 _maxIndex--;
//                 while (_maxIndex >= 0)
//                 {
//                     if (CheckDisplay(_maxIndex, _itemPosArr[_maxIndex], _itemSizeArr[_maxIndex]))
//                     {
//                         OnItemDisplay(_maxIndex, Datas[_maxIndex], _args);
//                         break;
//                     }
//                     Enqueue(_maxIndex);
//                     _maxIndex--;
//                 }
//             }
//         }
//
//         private void BindItemAction(UIDynamicScrollRectItemEx<T> item, params object[] args)
//         {
//             K k = item as K;
//             SelectAt(k?.Index ?? -1, args);
//         }
//         
//         protected virtual void OnRefresh()
//         {
//             _minIndex = -1;
//             _maxIndex = -1;
//             
//             if (Datas?.Length > 0)
//             {
//                 RefreshAllItemPosAndSize();
//                 Grid.sizeDelta = _gridSizeDelta;
//                 
//                 for (int i = 0; i < Datas.Length; i++)
//                 {
//                     bool isDisplay = CheckDisplay(i, _itemPosArr[i], _itemSizeArr[i]);
//                     if (isDisplay)
//                     {
//                         if (_minIndex == -1 || i < _minIndex) _minIndex = i;
//                         if (_maxIndex == -1 || i > _maxIndex) _maxIndex = i;
//                         // OnItemDisplay(i, Datas[i], _args);
//                     }
//                     else
//                     {
//                         if (_maxIndex > 0) break;
//                     }
//                 }
//                 
//                 int[] keys = _itemDict.Keys.ToArray();
//                 foreach (int key in keys)
//                 {
//                     K itemEx = _itemDict[key];
//                     if (itemEx.Index >= Datas.Length || itemEx.Index < _minIndex || itemEx.Index > _maxIndex)
//                     {
//                         Enqueue(itemEx);
//                         _itemDict.Remove(key);
//                     }
//                 }
//
//                 for (int i = _minIndex; i <= _maxIndex; i++)
//                 {
//                     if (i < 0 || i >= Datas.Length) continue;
//                     OnItemDisplay(i, Datas[i], _args);
//                 }
//
//             }
//             else
//             {
//                 foreach (K itemEx in _itemDict.Values)
//                 {
//                     Enqueue(itemEx);
//                 }
//                 _itemDict.Clear();
//             }
//
//             Grid.sizeDelta = _gridSizeDelta;
//         }
//
//         protected virtual void OnItemDisplay(int index, T data, object[] args)
//         {
//             // Log.Info("OnItemDisplay", index);
//             
//             if (!_itemDict.ContainsKey(index))
//             {
//                 _itemDict.Add(index, Dequeue());
//             }
//
//             K item = _itemDict[index];
//             item.SetActive(true);
//             item.SetSelect(SelectIndex == index);
//             item.Refresh(index, data, _itemPosArr[index], _itemSizeArr[index], args);
//
//             // if (_minIndex == -1 || index < _minIndex) _minIndex = index;
//             // if (_maxIndex == -1 || index > _maxIndex) _maxIndex = index;
//         }
//
//         protected virtual void RefreshAllItemPosAndSize()
//         {
//             _itemPosArr = new Vector2[Datas.Length];
//             _itemSizeArr = new Vector2[Datas.Length];
//             _gridSizeDelta = RectTrScrollRect.rect.size;
//
//             float offset = IsVertical ? GridLayoutGroup.padding.top : GridLayoutGroup.padding.left;
//             for (int i = 0; i < Datas.Length; i++)
//             {
//                 Vector2 size = CalculateItemSize(i, Datas[i]);
//                 _itemSizeArr[i] = size;
//
//                 if (IsVertical)
//                 {
//                     float x = size.x / 2;
//                     float y = size.y / 2 + offset;
//
//                     if (i < Datas.Length - 1) offset += GridLayoutGroup.spacing.y + size.y;
//                     else offset += size.y;
//
//                     _itemPosArr[i] = new Vector2(x, -y);
//                 }
//                 else
//                 {
//                     float x = size.x / 2 + offset;
//                     float y = size.y / 2;
//
//                     if (i < Datas.Length - 1) offset += GridLayoutGroup.spacing.x + size.x;
//                     else offset += size.x;
//
//                     _itemPosArr[i] = new Vector2(x, -y);
//                 }
//             }
//
//             if (IsVertical)
//             {
//                 if (offset > _gridSizeDelta.y) _gridSizeDelta.y = offset;
//             }
//             else
//             {
//                 if (offset > _gridSizeDelta.x) _gridSizeDelta.x = offset;
//             }
//         }
//
//         protected virtual bool CheckDisplay(int index, Vector2 pos, Vector2 size)
//         {
//             if (IsVertical)
//             {
//                 float upPos = Mathf.Abs(pos.y) - size.y / 2;
//                 float downPos = Mathf.Abs(pos.y) + size.y / 2;
//
//                 float gridUpPos = Grid.anchoredPosition.y;
//                 float gridDownPos = gridUpPos + RectTrScrollRect.rect.height;
//                 
//                 if (downPos < gridUpPos || upPos > gridDownPos) return false;
//             }
//             else
//             {
//                 float leftPos = Mathf.Abs(pos.x) - size.x / 2;
//                 float rightPos = Mathf.Abs(pos.x) + size.x / 2;
//
//                 float gridMinPos = -Grid.anchoredPosition.x;
//                 float gridMaxPos = gridMinPos + RectTrScrollRect.rect.width;
//                 
//                 if (rightPos < gridMinPos || leftPos > gridMaxPos) return false;
//             }
//             return true;
//         }
//         
//         protected virtual void OnItemAction(K k, params object[] args)
//         {
//             _itemAction?.Invoke(k, args);
//         }
//
//         protected virtual Vector2 CalculateItemSize(int index, T t)
//         {
//             return _calculateItemSizeDelegate?.Invoke(index, t) ?? GridLayoutGroup.cellSize;
//         }
//
//     }
// }

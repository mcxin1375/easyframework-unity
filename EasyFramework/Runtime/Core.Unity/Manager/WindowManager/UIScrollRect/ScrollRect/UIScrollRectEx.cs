// /*----------------------------------------------------------------
// // author:meng cheng xin
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace EasyFramework
// {
//     public class UIScrollRectEx<TK,TD> where TK: UIScrollRectItemEx<TD>, new() 
//     {
//         public RectTransform ScrollRect { get; private set; }
//         public RectTransform Grid { get; private set; }
//         public RectTransform Item { get; private set; }
//         public GridLayoutGroup GridLayoutGroup { get; private set; }
//         
//         public int SelectIndex { get; protected set; }
//         public TD[] DataArray { get; protected set; }
//         
//         public IReadOnlyList<TK> ItemList => itemList;
//         public TK SelectItem => GetItem(SelectIndex);
//         
//         protected Action<TK, Button> itemAction;
//         protected List<TK> itemList = new();
//         private Queue<TK> _itemCache;
//
//         public UIScrollRectEx(RectTransform rectTr, Action<TK, Button> action = null)
//         {
//             ScrollRect = rectTr;
//             itemAction = action;
//             Grid = ScrollRect.GetChild(0) as RectTransform;
//             Item = Grid.GetChild(0) as RectTransform;;
//             Item.gameObject.SetActive(false);
//             GridLayoutGroup = Grid.GetComponent<GridLayoutGroup>();
//         }
//
//         public void Dispose() => OnDispose();
//         
//         protected virtual void OnDispose()
//         {
//             foreach (TK item in itemList)
//             {
//                 item.Dispose();
//             }
//         }
//
//         public void Refresh(object[] arr)
//         {
//             
//         }
//
//         public virtual void Refresh(TD[] datas, params object[] args)
//         {
//             DataArray = datas;
//             if (DataArray != null)
//             {
//                 for (int i = 0; i < DataArray.Length; i++)
//                 {
//                     if (i >= itemList.Count)
//                     {
//                         itemList.Add(Dequeue());
//                     }
//
//                     TK item = itemList[i];
//                     item.SetActive(true);
//                     item.Refresh(i, DataArray[i], args);
//                 }
//             }
//
//             int len = DataArray?.Length ?? 0;
//             if (len < itemList.Count)
//             {
//                 for (int i = len; i < itemList.Count; i++)
//                     itemList[i].SetActive(false);
//             }
//             Grid.sizeDelta = UGUIHelper.CalculateGridSizeDelta(Grid, len);
//
//         }
//         
//         public void RefreshAtIndex(TD[] dataArray, int index, params object[] args)
//         {
//             Refresh(dataArray, args);
//             SelectAt(index);
//         }
//
//         public virtual TK GetItem(int index)
//         {
//             return index >= 0 && index < itemList.Count ? itemList[index] : null;
//         }
//
//         public void SelectAt(int index)
//         {
//             SelectIndex = index;
//             OnItemAction(GetItem(index));
//         }
//         public void SelectEmpty()
//         {
//             foreach (TK item in itemList)
//             {
//                 item.SetSelect(false);
//             }
//             SelectIndex = -1;
//         }
//
//         protected virtual void OnItemAction(TK k, Button btn = null)
//         {
//             if (k != null)
//             {
//                 SelectIndex = k.Index;
//             }
//             foreach (TK item in itemList)
//             {
//                 item.SetSelect(item.Index == k?.Index);
//             }
//             itemAction?.Invoke(k, btn);
//         }
//
//         protected TK CreateItem()
//         {
//             GameObject go = GameObject.Instantiate(Item.gameObject, Grid);
//             TK item = new TK();
//             item.Register(go, OnScrollRectItemAction);
//             return item;
//         }
//
//         protected void Enqueue(TK k)
//         {
//             if (k == null) return;
//             if (_itemCache == null) _itemCache = new Queue<TK>();
//             k.SetActive(false);
//             _itemCache.Enqueue(k);
//         }
//
//         protected TK Dequeue()
//         {
//             return _itemCache?.Count > 0 ? _itemCache.Dequeue() : CreateItem();
//         }
//
//         private void OnScrollRectItemAction(UIScrollRectItemEx<TD> item, Button btn = null)
//         {
//             if (item != null)
//             {
//                 OnItemAction(item as TK, btn);
//             }
//
//         }
//
//     }
// }

// /*----------------------------------------------------------------
// // author:meng cheng xin
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace EasyFramework
// {
//     public class UIVerticalScrollRectEx<K, T> : UIScrollRectEx<K, T> where K : UIScrollRectItemEx<T>, new()
//     {
//         public UIVerticalScrollRectEx(RectTransform rectTr, Action<K, Button> action) : base(rectTr, action)
//         {
//             verticalLayoutGroup = Grid.GetComponent<VerticalLayoutGroup>();
//             verticalLayoutGroup.enabled = false;
//
//             Item.anchorMin = new Vector2(0, 1);
//             Item.anchorMax = new Vector2(0, 1);
//             Item.pivot = new Vector2(0.5f, 1);
//         }
//
//         public VerticalLayoutGroup verticalLayoutGroup { get; private set; }
//
//         // public void Refresh(T[] datas)
//         // {
//         //     if (datas != null)
//         //     {
//         //         for (int i = 0; i < datas.Length; i++)
//         //         {
//         //             K item = itemList.Count > i ? itemList[i] : CreateItem();
//         //             item.gameObject.SetActive(true);
//         //             item.Refresh(i, datas[i]);
//         //         }
//         //
//         //         if (datas.Length < itemList.Count)
//         //         {
//         //             for (int i = datas.Length; i < itemList.Count; i++)
//         //                 itemList[i].gameObject.SetActive(false);
//         //         }
//         //
//         //         K curItem = GetItem(SelectIndex);
//         //         OnItemAction(curItem);
//         //         
//         //         RefreshAllItemPosAndSize(datas);
//         //     }
//         // }
//
//         public override void Refresh(T[] datas, params object[] args)
//         {
//             // base.Refresh(datas, args);
//
//             DataArray = datas;
//             if (datas != null)
//             {
//                 for (int i = 0; i < datas.Length; i++)
//                 {
//                     if (i >= itemList.Count)
//                     {
//                         itemList.Add(Dequeue());
//                     }
//
//                     K item = itemList[i];
//                     item.SetActive(true);
//                     item.Refresh(i, datas[i], args);
//                 }
//             }
//
//             int len = datas?.Length ?? 0;
//             if (len < itemList.Count)
//             {
//                 for (int i = len; i < itemList.Count; i++)
//                     itemList[i].SetActive(false);
//             }
//
//             RefreshAllItemPosAndSize(len);
//         }
//
//
//         private void RefreshAllItemPosAndSize(int dataLen)
//         {
//             float height = verticalLayoutGroup.padding.top;
//
//             for (int i = 0; i < itemList.Count; i++)
//             {
//                 K item = itemList[i];
//
//                 RectTransform rectTr_Item = item.gameObject.GetComponent<RectTransform>();
//                 rectTr_Item.anchoredPosition = new Vector2(rectTr_Item.sizeDelta.x / 2, -height);
//                 if (i < dataLen)
//                     height = height + rectTr_Item.sizeDelta.y + verticalLayoutGroup.spacing;
//             }
//
//             if (itemList.Count > 0)
//                 height = height - verticalLayoutGroup.spacing;
//             height = height + verticalLayoutGroup.padding.bottom;
//             if (height < ScrollRect.rect.height)
//                 height = ScrollRect.rect.height;
//             Grid.sizeDelta = new Vector2(Grid.sizeDelta.x, height);
//         }
//     }
// }
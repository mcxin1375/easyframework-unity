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
//     
//     /// <summary>
//     /// 循环滚动组件
//     /// rectTr 需要用 ScrollRectEx 组件
//     /// grid 需要用 GridLayoutGroup 组件
//     /// </summary>
//     /// <typeparam name="K"></typeparam>
//     /// <typeparam name="T"></typeparam>
//     public class UILoopScrollRectEx<K, T> : UIScrollRectEx<K, T> where K : UIScrollRectItemEx<T>, new()
//     {
//         
//         private enum EScrollRectItemInputType
//         {
//             None,
//             BeginDrag,
//             Drag,
//             EndDrag,
//             Press
//         }
//         
//         public float LongPressTime { get; set; } = 1;
//
//         public event Action<K> OnItemPress; 
//         public event Action<K, int, Vector2> OnItemBeginDrag; 
//         public event Action<K, int, Vector2> OnItemDrag; 
//         public event Action<K, int, Vector2> OnItemEndDrag; 
//
//         private bool IsVertical => GridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount;
//         private object[] _args;
//         private K[] _items;
//         private int _startIndex, _endIndex;
//         
//         // ==== 拖拽Item功能 相关参数
//         // private Action<K, EScrollRectItemInputType, int, Vector2> _inputAction;
//         private Vector2 _downPos;
//         private bool _checkDownType;
//         private float _downTime;
//         private EScrollRectItemInputType _inputType;
//         private K _downItem;
//         
//         // ==== 拖拽Item功能 相关参数
//
//         public UILoopScrollRectEx(RectTransform rectTr, Action<K, Button> action = null) : base(rectTr, action)
//         {
//             GridLayoutGroup.enabled = false;
//             Item.anchorMin = new Vector2(0, 1);
//             Item.anchorMax = new Vector2(0, 1);
//             Item.pivot = new Vector2(0.5f, 0.5f);
//             Item.sizeDelta = GridLayoutGroup.cellSize;
//
//             ScrollRectEx scrollRectEx = rectTr.GetComponent<ScrollRectEx>();
//             if (scrollRectEx == null)
//             {
//                 ScrollRect scrollRect = rectTr.GetComponent<ScrollRect>();
//                 scrollRect.enabled = false;
//
//                 scrollRectEx = rectTr.gameObject.AddComponentEx<ScrollRectEx>();
//                 scrollRectEx.content = scrollRect.content;
//                 scrollRectEx.horizontal = scrollRect.horizontal;
//                 scrollRectEx.vertical = scrollRect.vertical;
//                 scrollRectEx.velocity = scrollRect.velocity;
//                 scrollRectEx.scrollSensitivity = scrollRect.scrollSensitivity;
//                 scrollRectEx.decelerationRate = scrollRect.decelerationRate;
//                 scrollRectEx.viewport = scrollRect.viewport;
//                 scrollRectEx.verticalScrollbar = scrollRect.verticalScrollbar;
//                 scrollRectEx.horizontalScrollbar = scrollRect.horizontalScrollbar;
//             }
//
//             scrollRectEx.OnUpdateContentPos += OnUpdateContentPos;
//             F.InputSystem.InputEvent += OnInputEvent;
//         }
//
//         protected override void OnDispose()
//         {
//             base.OnDispose();
//
//             ScrollRectEx scrollRectEx = ScrollRect.GetComponent<ScrollRectEx>();
//             scrollRectEx.OnUpdateContentPos -= OnUpdateContentPos;
//
//             F.InputSystem.InputEvent -= OnInputEvent;
//             if (_items != null)
//             {
//                 foreach (K item in _items)
//                 {
//                     if (item != null) item.Dispose();
//                 }
//             }
//         }
//
//         public override void Refresh(T[] datas, params object[] args)
//         {
//             DataArray = datas;
//             _args = args;
//
//             if (_items != null)
//             {
//                 foreach (K item in _items)
//                 {
//                     Enqueue(item);
//                 }
//             }
//
//             _startIndex = 0;
//             _endIndex = 0;
//             _items = DataArray != null ? new K[DataArray.Length] : null;
//             RefreshItemArrayPosition(Grid.anchoredPosition, true);
//             
//             Grid.sizeDelta = UGUIHelper.CalculateGridSizeDelta(Grid, DataArray?.Length ?? 0);
//         }
//
//         public override K GetItem(int index)
//         {
//             return _items != null && _items.Length > index ? _items[index] : null;
//         }
//
//         protected override void OnItemAction(K k, Button btn = null)
//         {
//             if (k != null && _items != null)
//             {
//                 SelectIndex = k.Index;
//             
//                 foreach (K item in _items)
//                 {
//                     if (item != null)
//                         item.SetSelect(item.Index == k.Index);
//                 }
//             }
//             itemAction?.Invoke(k, btn);
//         }
//
//         private void OnUpdateContentPos(Vector2 anchoredPosition)
//         {
//             if (DataArray != null)
//             {
//                 RefreshItemArrayPosition(anchoredPosition, false);
//             }
//         }
//
//         private void OnInputEvent(EInputType inputType, int fingerId, Vector2 position)
//         {
//             switch (inputType)
//             {
//                 case EInputType.Down:
//                     _inputType = EScrollRectItemInputType.None;
//                     _checkDownType = false;
//                     _downItem = null;
//                     
//                     var hit = UGUIHelper.RaycastUIObject(position, out var go);
//                     Transform target = hit ? go.transform.parent : null;
//                     if (target != null && _items != null)
//                     {
//                         foreach (K item in _items)
//                         {
//                             if (item == null) continue;
//                             if (item.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
//                             {
//                                 _checkDownType = true;
//                                 _downPos = position;
//                                 _downTime = Time.time;
//                                 _downItem = item;
//                                 break;
//                             }
//                         }
//                     }
//                     break;
//                 case EInputType.Hover:
//                     
//                     if (_checkDownType)
//                     {
//                         Vector2 offset = position - _downPos;
//                         if (Mathf.Abs(offset.x) > Screen.width * 0.01f)
//                         {
//                             // Log.Info("Out X");
//                             _checkDownType = false;
//                             if (IsVertical) _inputType = EScrollRectItemInputType.BeginDrag;
//                             else _inputType = EScrollRectItemInputType.EndDrag;
//                         }
//                         else if (Mathf.Abs(offset.y) > Screen.height * 0.01f)
//                         {
//                             // Log.Info("Out y");
//                             _checkDownType = false;
//                             if (!IsVertical) _inputType = EScrollRectItemInputType.BeginDrag;
//                             else _inputType = EScrollRectItemInputType.EndDrag;
//                         }
//                         else
//                         {
//                             if (Time.time - _downTime > LongPressTime)
//                             {
//                                 _checkDownType = false;
//                                 _inputType = EScrollRectItemInputType.Press;
//                             }
//                         }
//                     }
//
//                     switch (_inputType)
//                     {
//                         case EScrollRectItemInputType.BeginDrag:
//                             _inputType = EScrollRectItemInputType.Drag;
//                             BeginDragItem(fingerId, position);
//                             break;
//                         case EScrollRectItemInputType.Drag:
//                             DragItem(fingerId, position);
//                             break;
//                         case EScrollRectItemInputType.EndDrag:
//                             _inputType = EScrollRectItemInputType.None;
//                             EndDragItem(fingerId, position);
//                             break;
//                         case EScrollRectItemInputType.Press:
//                             _inputType = EScrollRectItemInputType.None;
//                             _downItem.IgnoreClickOnce = true;
//                             
//                             // _inputAction(_downItem, EScrollRectItemInputType.Press, fingerId, position);
//                             OnItemPress?.Invoke(_downItem);
//                             break;
//                     }
//                     
//                     break;
//                 case EInputType.Up:
//                     _inputType = EScrollRectItemInputType.None;
//                     _checkDownType = false;
//                     EndDragItem(fingerId, position);
//                     _downItem = null;
//                     break;
//             }
//         }
//
//         private void BeginDragItem(int fingerId, Vector2 position)
//         {
//             ScrollRectEx scrollRectEx = ScrollRect.GetComponent<ScrollRectEx>();
//             scrollRectEx.SetDragActive(false);
//
//             if (_downItem == null) return;
//             // _inputAction(_downItem, EScrollRectItemInputType.BeginDrag, fingerId, position);
//             OnItemBeginDrag?.Invoke(_downItem, fingerId, position);
//         }
//
//         private void DragItem(int fingerId, Vector2 position)
//         {
//             if (_downItem == null) return;
//             // _inputAction(_downItem, EScrollRectItemInputType.Drag, fingerId, position);
//             OnItemDrag?.Invoke(_downItem, fingerId, position);
//         }
//
//         private void EndDragItem(int fingerId, Vector2 position)
//         {
//             ScrollRectEx scrollRectEx = ScrollRect.GetComponent<ScrollRectEx>();
//             scrollRectEx.SetDragActive(true);
//             
//             if (_downItem == null) return;
//             // _inputAction(_downItem, EScrollRectItemInputType.EndDrag, fingerId, position);
//             OnItemEndDrag?.Invoke(_downItem, fingerId, position);
//         }
//
//         private void RefreshItemArrayPosition(Vector2 anchoredPosition, bool refreshAll)
//         {
//             int startIndex, endIndex;
//
//             int lineCount = GridLayoutGroup.constraintCount;
//             if (IsVertical)
//             {
//                 //屏幕适配行列个数
//                 lineCount = Mathf.FloorToInt(ScrollRect.rect.width  / (GridLayoutGroup.cellSize.x + GridLayoutGroup.spacing.y));
//                 lineCount = ScrollRect.rect.width % (GridLayoutGroup.cellSize.x + GridLayoutGroup.spacing.y) > GridLayoutGroup.cellSize.x ? lineCount + 1 : lineCount;
//                 if (GridLayoutGroup.constraintCount != lineCount) GridLayoutGroup.constraintCount = lineCount;
//
//                 if (lineCount < 1) lineCount = 1;
//                 
//                 float tmpTop = anchoredPosition.y;
//                 startIndex =Mathf.FloorToInt(tmpTop / (GridLayoutGroup.cellSize.y + GridLayoutGroup.spacing.y)) * lineCount;
//                 float tmpBottom = Grid.anchoredPosition.y + ScrollRect.rect.height;
//                 endIndex = Mathf.CeilToInt(tmpBottom / (GridLayoutGroup.cellSize.y + GridLayoutGroup.spacing.y)) * lineCount;
//             }
//             else
//             {
//                 float tmpLeft = -anchoredPosition.x;
//                 startIndex = Mathf.FloorToInt(tmpLeft / (GridLayoutGroup.cellSize.x + GridLayoutGroup.spacing.x)) * lineCount;
//                 float tmpRight = -anchoredPosition.x + ScrollRect.rect.width;
//                 endIndex = Mathf.CeilToInt(tmpRight / (GridLayoutGroup.cellSize.x + GridLayoutGroup.spacing.x)) * lineCount;
//             }
//             
//             startIndex = startIndex < 0 ? 0 : startIndex;
//             endIndex = endIndex > DataArray.Length ? DataArray.Length : endIndex;
//
//             if (_startIndex == startIndex && _endIndex == endIndex) return;
//             
//             _startIndex = startIndex;
//             _endIndex = endIndex;
//             
//             if (_items != null)
//             {
//                 for (int i = 0; i < _items.Length; i++)
//                 {
//                     if (i < startIndex || i > endIndex)
//                     {
//                         if (_items[i] != null)
//                         {
//                             Enqueue(_items[i]);
//                             _items[i] = null;
//                         }
//                     }
//                 }
//             }
//
//             // Log.Info(startIndex, endIndex);
//             if (DataArray?.Length > 0)
//             {
//                 for (int i = startIndex; i < endIndex; i++)
//                 {
//                     K item = GetItem(i);
//                     if (item == null)
//                     {
//                         item = Dequeue();
//                         item.SetActive(true);
//                         item.Refresh(i, DataArray[i], _args);
//                         item.SetSelect(i == SelectIndex);
//
//                         float x, y;
//                         int offsetIndex = i % lineCount;
//                         int lineIndex = i / lineCount;
//                         if (IsVertical)
//                         {
//                             x = GridLayoutGroup.cellSize.x / 2 + offsetIndex * (GridLayoutGroup.cellSize.x + GridLayoutGroup.spacing.x) + GridLayoutGroup.padding.left;
//                             y = -(GridLayoutGroup.cellSize.y + GridLayoutGroup.spacing.y) * lineIndex + -GridLayoutGroup.cellSize.y / 2 - GridLayoutGroup.padding.top;
//                         }
//                         else
//                         {
//                             x = (GridLayoutGroup.cellSize.x + GridLayoutGroup.spacing.x) * lineIndex + GridLayoutGroup.cellSize.x / 2 + GridLayoutGroup.padding.left;
//                             y = -GridLayoutGroup.cellSize.y / 2 + offsetIndex * (GridLayoutGroup.cellSize.y + GridLayoutGroup.spacing.y) - GridLayoutGroup.padding.top;
//                         }
//                         item.rectTransform.sizeDelta = GridLayoutGroup.cellSize; // 屏幕适配时会需要改变cellSize
//                         item.rectTransform.anchoredPosition = new Vector2(x, y);
//                         
//                         _items[i] = item;
//                     }
//                     else if (refreshAll)
//                     {
//                         item.Refresh(i, DataArray[i], _args);
//                         item.SetSelect(i == SelectIndex);
//                     }
//                 }
//             }
//             else
//             {
//                 if (_items != null)
//                 {
//                     foreach (K item in _items)
//                     {
//                         Enqueue(item);
//                     }
//                 }
//
//                 _items = null;
//             }
//         }
//     }
// }
// /*----------------------------------------------------------------
// // author:Cookie mcx
// // date:2023/4/14
// // describe:无线滚动列表（动态尺寸格子，只支持一行一个格子）
// //----------------------------------------------------------------*/
//
// using System;
// using UnityEngine;
//
// namespace EasyFramework
// {
//     public abstract class UIDynamicScrollRectItemEx<T> : ITimerObject
//     {
//         public bool IsTimerAlive => gameObject != null && gameObject.activeSelf;
//
//         public GameObject gameObject { get; private set; }
//         public RectTransform rectTransform { get; private set; }
//
//         
//         public T Data { get; private set; }
//         public int Index { get; private set; }
//         public bool IsSelect { get; private set; }
//
//         private Action<UIDynamicScrollRectItemEx<T>, object[]> _action;
//
//         
//         public void Register(GameObject go, Action<UIDynamicScrollRectItemEx<T>, object[]> action)
//         {
//             gameObject = go;
//             _action = action;
//             rectTransform = go.transform as RectTransform;
//
//             ComponentHelper.AutoSetUIComponents(this, go);
//             OnBindEvent();
//             OnRegister();
//         }
//
//         public void Refresh(int index, T t, Vector2 anchoredPosition, Vector2 sizeDelta, object[] args = null)
//         {
//             Index = index;
//             Data = t;
//             rectTransform.anchoredPosition = anchoredPosition;
//             rectTransform.sizeDelta = sizeDelta;
//             
//             OnRefresh(args);
//         }
//
//         public void SetSelect(bool value) 
//         { 
//             IsSelect = value;
//             OnSetSelect();
//         }
//
//         public void SetActive(bool value)
//         {
//             gameObject.SetActive(value);
//         }
//
//         public void Dispose()
//         {
//             OnDispose();
//         }
//
//         protected virtual void OnBindEvent()
//         {
//             gameObject.AddComponent<UIButtonBinding>().BindAction(btn => { _action?.Invoke(this, new object[] { btn }); });
//         }
//         protected virtual void OnRegister() { }
//         protected virtual void OnDispose() { }
//         protected virtual void OnSetSelect() { }
//
//         protected abstract void OnRefresh(object[] args = null);
//
//
//     }
// }

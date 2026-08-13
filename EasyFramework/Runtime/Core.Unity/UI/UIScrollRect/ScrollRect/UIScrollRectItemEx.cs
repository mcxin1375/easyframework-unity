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
//     public class UIScrollRectItemEx<T> : ITimerObject
//     {
//         // public bool IsTimerAlive { get; private set; }
//         public bool IsTimerAlive => gameObject != null && gameObject.activeSelf;
//
//         public GameObject gameObject { get; private set; }
//         public RectTransform rectTransform { get; private set; }
//
//         public bool IgnoreClickOnce { get; set; }
//         public bool IsSelect { get; private set; }
//         public int Index { get; private set; }
//
//         public T Data { get; private set; }
//
//         internal void Register(GameObject go, Action<UIScrollRectItemEx<T>, Button> action)
//         {
//             gameObject = go;
//             rectTransform = go.transform as RectTransform;
//             // IsTimerAlive = true;
//
//             var uiButtonBinding = gameObject.AddComponent<UIButtonBinding>();
//             uiButtonBinding.BindAction(btn =>
//             {
//                 if (IgnoreClickOnce)
//                 {
//                     IgnoreClickOnce = false;
//                     return;
//                 }
//
//                 action?.Invoke(this, btn);
//             });
//             
//             ComponentHelper.AutoSetUIComponents(this, go);
//             OnCreate();
//         }
//         
//         public void Refresh()
//         {
//             OnRefresh();
//         }
//
//         public void Refresh(int index, T t, params object[] args)
//         {
//             Index = index;
//             Data = t;
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
//             // Log.Info(value);
//         }
//
//         internal void Dispose()
//         {
//             OnDispose();
//             // IsTimerAlive = false;
//         }
//
//         protected virtual void OnCreate() { }
//         protected virtual void OnDispose() { }
//         // protected virtual void OnRefresh(params object[] args) { }
//         protected virtual void OnRefresh(params object[] args) { }
//         protected virtual void OnSetSelect() { }
//         
//     }
// }

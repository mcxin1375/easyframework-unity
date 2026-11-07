/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EasyFramework
{
    public abstract class Window : IWindow, ITimerObject, IResRequest
    {
        public bool Alive { get; private set; } = true;
        public bool KeepAlive => IsOpen || _closedTime <= 0 || Time.time <= _closedTime + KeepAliveTime;
        public bool IsTimerAlive => IsOpen;
        public bool IsOpen { get; private set; }
        public bool IsLoading { get; private set; }
        public bool IsActive { get; private set; }
        public Type Type { get; }
        public UILayerBehaviour LayerBehaviour { get; private set; }
        public GameObject WindowObject { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public Canvas Canvas { get; private set; }
        public GraphicRaycaster GraphicRaycaster { get; private set; }

        protected virtual float KeepAliveTime => 60;
        private float _closedTime;
        private readonly List<IWindowComponent> _componentList = new();
        
        protected Window()
        {
            Type = GetType();
        }
        protected void RegisterComponent(IWindowComponent component)
        {
            if (component == null || _componentList.Contains(component)) return;
            _componentList.Add(component);
            component.Register(this);
        }
        private void InitWindowObject()
        {
            WindowObject.name = GetType().Name;
            RectTransform = WindowObject.transform as RectTransform;
            Canvas = WindowObject.AddComponentEx<Canvas>();
            GraphicRaycaster = WindowObject.AddComponentEx<GraphicRaycaster>();
            WindowObject.AddComponentEx<UIButtonBinding>().BindAction(ButtonClick);
            
            if (this is IWindowUI windowUI)
            {
                windowUI.InitializeUI(WindowObject);
            }
            
            _componentList.Clear();
            var arr = ReflectionUtility.FindFieldsAndProperties<IWindowComponent>(this);
            foreach (var ex in arr) RegisterComponent(ex);
            var comps = WindowObject.GetComponentsInChildren<IWindowComponent>();
            foreach (var comp in comps) RegisterComponent(comp);

            OnWindowLoaded();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnWindowLoaded();
        }
        void IWindow.Create()
        {
            OnCreate();
        }
        void IWindow.Destroy()
        {
            if (!Alive) return;
            Alive = false;

            try
            {
                ((IWindow)this).Close();
            
                OnDestroy();
                if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnDestroy();
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }

            _componentList.Clear();
            if (WindowObject != null)
            {
                Object.Destroy(WindowObject);
                WindowObject = null;
            }
        }
        void IWindow.Open(UILayerBehaviour uiLayer)
        {
            if (!Alive) throw new InvalidOperationException($"Window.Open({Type.Name}): window has been destroyed.");
            if (IsLoading) throw new InvalidOperationException("Window creation is already in loading.");
            
            LayerBehaviour = uiLayer;
            if (WindowObject == null)
            {
                WindowObject = CreateWindowObject(uiLayer.transform);
                if (WindowObject == null)
                    throw new ArgumentNullException($"Window object is null. Type: {Type.Name}");
                
                InitWindowObject();
            }
            
            WindowObject.SetActive(true);
        
            if (!IsOpen)
            {
                IsOpen = true;
                _closedTime = 0;
                OnAddListeners();
                if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnAddListeners();
            }
            
            OnOpen();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnOpen();
            
            Refresh();
        }
        async ETask IWindow.OpenAsync(UILayerBehaviour uiLayer)
        {
            if (!Alive) throw new InvalidOperationException($"Window.OpenAsync({Type.Name}): window has been destroyed.");
            if (IsLoading) throw new InvalidOperationException("Window creation is already in loading.");
            
            LayerBehaviour = uiLayer;
            if (WindowObject == null)
            {
                IsLoading = true;

                try
                {
                    WindowObject = await CreateWindowObjectAsync(uiLayer.transform);
                }
                catch (Exception e)
                {
                    FDebug.LogException(e);
                }
                finally { IsLoading = false; }
                
                if (WindowObject == null)
                    throw new ArgumentNullException($"Window object is null. Type: {Type.Name}");
                
                InitWindowObject();
            }
            
            (this as IWindow).Open(uiLayer);
        }

        void IWindow.Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            _closedTime = Time.time;

            OnRemoveListeners();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnRemoveListeners();

            OnClose();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnClose();
            
            if (WindowObject) WindowObject.SetActive(false);
        }
        public void SetActive(bool value)
        {
            if (!Alive) return;
            if (WindowObject == null) return;

            IsActive = value;
            WindowObject.SetActive(value);
            OnSetActive(value);
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnSetActive(value);
        }
        public void Refresh()
        {
            if (!Alive) return;

            OnRefresh();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnRefresh();
        }
        void IWindow.RefreshOrder(UILayerBehaviour uiLayer, int orderInLayer, int orderIndex)
        {
            if (!Alive) return;

            LayerBehaviour = uiLayer;
            if (WindowObject == null) return;
            
            Canvas.overrideSorting = true;
            Canvas.sortingLayerName = LayerBehaviour.SortingLayerName;
            Canvas.sortingOrder = orderInLayer;
            
            RectTransform.SetParent(LayerBehaviour.transform);
            RectTransform.ResetLocalPropertyEx();
            // RectTransform.SetAsLastSibling();
            RectTransform.SetSiblingIndex(orderIndex);
            WindowObject.SetLayerEx(LayerBehaviour.gameObject.layer);

            OnRefreshOrderInLayer(Canvas.sortingOrder);
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnRefreshOrderInLayer(Canvas.sortingOrder);
        }
        void IWindow.Update()
        {
            if (!Alive) return;
            
            OnUpdate();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnUpdate();
        }
        void ButtonClick(Button button)
        {
            OnButtonClick(button);
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.OnButtonClick(button);
        }

        protected void Close() => F.WindowManager.Close(this);

        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnWindowLoaded() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnSetActive(bool value) { }
        protected virtual void OnRefresh() { }
        protected virtual void OnRefreshOrderInLayer(int orderInLayer) { }
        protected virtual void OnButtonClick(Button btn) { }
        protected virtual void OnUpdate() { }

        protected virtual GameObject CreateWindowObject(Transform parent) => F.ResLoader.CreateObj(Type.Name, parent);
        protected virtual ETask<GameObject> CreateWindowObjectAsync(Transform parent) => F.ResLoader.CreateObjAsync(Type.Name, parent, this);
    }
}

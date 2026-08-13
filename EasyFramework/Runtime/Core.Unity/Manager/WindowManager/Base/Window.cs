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
        public bool Alive => IsTimerAlive;
        public bool IsTimerAlive => IsOpen;
        public virtual bool NeedDestroy => !IsOpen && _destroyTime > 0 && Time.time > _destroyTime;
        public bool IsOpen { get; private set; }
        public bool IsActive { get; private set; }
        public Type Type { get; }
        public UILayerBehaviour LayerBehaviour { get; private set; }
        public GameObject WindowObject { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public Canvas Canvas { get; private set; }
        public GraphicRaycaster GraphicRaycaster { get; private set; }

        protected virtual float KeepAliveTime => 60;
        private float _destroyTime;
        private readonly List<IWindowComponent> _componentList = new();
        
        protected Window()
        {
            Type = GetType();
        }
        private void RegisterComponent(IWindowComponent component)
        {
            if (component == null) return;
            _componentList.Add(component);
        }
        private void InitWindowObject()
        {
            WindowObject.name = GetType().Name;
            RectTransform = WindowObject.transform as RectTransform;
            Canvas = WindowObject.AddComponentEx<Canvas>();
            GraphicRaycaster = WindowObject.AddComponentEx<GraphicRaycaster>();
            WindowObject.AddComponentEx<UIButtonBinding>().BindAction(ButtonClick);
            
            _componentList.Clear();
            var arr = EasyFrameworkReflection.FindFieldsAndProperties<IWindowComponent>(this);
            foreach (var ex in arr) RegisterComponent(ex);
            var comps = WindowObject.GetComponentsInChildren<IWindowComponent>();
            foreach (var comp in comps) RegisterComponent(comp);
            
            if (this is IWindowUI windowUI)
            {
                windowUI.InitializeUI(WindowObject);
            }
            
            OnCreate();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.Create(this);
        }
        public async ETask CreateAsync(UILayerBehaviour uiLayer)
        {
            LayerBehaviour = uiLayer;
            if (WindowObject != null) return;
            
            WindowObject = await F.ResLoader.CreateObjAsync(Type.Name, uiLayer.transform, this);
            if (WindowObject == null)
            {
                Debug.LogError($"Failed to create window object of type {Type.Name}");
                return;
            }

            InitWindowObject();
        }
        void IWindow.Create(UILayerBehaviour uiLayer)
        {
            LayerBehaviour = uiLayer;
            if (WindowObject != null) return;
            
            WindowObject = F.ResLoader.CreateObj(Type.Name, uiLayer.transform);
            if (WindowObject == null)
            {
                Debug.LogError($"Failed to create window object of type {Type.Name}");
                return;
            }
            
            InitWindowObject();
        }
        void IWindow.Destroy()
        {
            OnDestroy();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.Destroy();

            _componentList.Clear();
            if (WindowObject != null)
            {
                Object.Destroy(WindowObject);
                WindowObject = null;
            }
        }
        void IWindow.Open()
        {
            if (WindowObject != null) WindowObject.SetActive(true);
        
            if (!IsOpen)
            {
                _destroyTime = 0;
                IsOpen = true;
                OnAddListeners();
                if (_componentList.Count > 0) foreach (var ex in _componentList) ex.AddListeners();
            }
            
            OnOpen();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.Open();
            
            Refresh();
        }
        async ETask IWindow.AfterOpenAsync()
        {
            await OnAfterOpenAsync();
            if (_componentList.Count > 0) foreach (var ex in _componentList) await ex.AfterOpenAsync();
        }
        async ETask IWindow.BeforeCloseAsync()
        {
            await OnBeforeCloseAsync();
            if (_componentList.Count > 0) foreach (var ex in _componentList) await ex.BeforeCloseAsync();
        }
        void IWindow.Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            _destroyTime = Time.time + KeepAliveTime;

            OnRemoveListeners();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.RemoveListeners();

            OnClose();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.Close();
            
            if (WindowObject) WindowObject.SetActive(false);
        }
        public void SetActive(bool value)
        {
            if (WindowObject == null) return;

            IsActive = value;
            WindowObject.SetActive(value);
            OnSetActive(value);
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.SetActive(value);
        }
        public void Refresh()
        {
            OnRefresh();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.Refresh();
        }
        void IWindow.RefreshOrder(UILayerBehaviour uiLayer, int orderInLayer, int orderIndex)
        {
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

            OnRefreshOrderInLayer(orderInLayer);
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.RefreshOrderInLayer(orderInLayer);
        }
        void IWindow.Update()
        {
            OnUpdate();
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.Update();
        }
        void ButtonClick(Button button)
        {
            OnButtonClick(button);
            if (_componentList.Count > 0) foreach (var ex in _componentList) ex.ButtonClick(button);
        }

        protected void Close() => F.WindowManager.Close(this);
        protected ETask CloseAsync() => F.WindowManager.CloseAsync(this);

        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnOpen() { }
        protected virtual ETask OnAfterOpenAsync() => ETask.CompletedTask;
        protected virtual ETask OnBeforeCloseAsync() => ETask.CompletedTask;
        protected virtual void OnClose() { }
        protected virtual void OnSetActive(bool value) { }
        protected virtual void OnRefresh() { }
        protected virtual void OnRefreshOrderInLayer(int orderInLayer) { }
        protected virtual void OnButtonClick(Button btn) { }
        protected virtual void OnUpdate() { }
    }
}
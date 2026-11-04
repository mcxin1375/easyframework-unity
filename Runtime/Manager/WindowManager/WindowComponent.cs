/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using UnityEngine.UI;

namespace EasyFramework
{
    public abstract class WindowComponent<T> : WindowComponent where T : class, IWindow
    {
        private T _t;
        public new T Window {
            get
            {
                _t ??= base.Window as T;
                return _t;
            }
        }
    }
    public abstract class WindowComponent : IWindowComponent
    {
        protected IWindow Window { get; private set; }

        void IWindowComponent.Register(IWindow window)
        {
            Window = window;
        }
        void IWindowComponent.OnLoadObject() => OnLoadObject();
        void IWindowComponent.OnDestroy() => OnDestroy();
        void IWindowComponent.OnAddListeners() => OnAddListeners();
        void IWindowComponent.OnRemoveListeners() => OnRemoveListeners();
        void IWindowComponent.OnOpen() => OnOpen();
        void IWindowComponent.OnClose() => OnClose();
        void IWindowComponent.OnSetActive(bool value) => OnSetActive(value);
        void IWindowComponent.OnRefresh() => OnRefresh();
        void IWindowComponent.OnRefreshOrderInLayer(int orderInLayer) => OnRefreshOrderInLayer(orderInLayer);
        void IWindowComponent.OnButtonClick(Button btn) => OnButtonClick(btn);
        void IWindowComponent.OnUpdate() => OnUpdate();

        protected virtual void OnLoadObject() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnSetActive(bool value) { }
        protected virtual void OnRefresh() { }
        protected virtual void OnRefreshOrderInLayer(int orderInLayer) { }
        protected virtual void OnButtonClick(Button btn) { }
        protected virtual void OnUpdate() { }
    }
}
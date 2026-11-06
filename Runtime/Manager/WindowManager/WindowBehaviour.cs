/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using UnityEngine.UI;

namespace EasyFramework
{
    public abstract class WindowBehaviour<T> : WindowBehaviour where T : class, IWindow
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
    public abstract class WindowBehaviour : UIBaseBehaviour, IWindowComponent
    {
        protected IWindow Window { get; private set; }

        void IWindowComponent.Register(IWindow window)
        {
            Window = window;
        }
        void IWindowComponent.OnWindowLoaded() => OnWindowLoaded();
        void IWindowComponent.OnDestroy() => OnDestroyEx();
        void IWindowComponent.OnAddListeners() => OnAddListeners();
        void IWindowComponent.OnRemoveListeners() => OnRemoveListeners();
        void IWindowComponent.OnOpen() => OnOpen();
        void IWindowComponent.OnClose() => OnClose();
        void IWindowComponent.OnSetActive(bool value) => OnSetActive(value);
        void IWindowComponent.OnRefresh() => OnRefresh();
        void IWindowComponent.OnRefreshOrderInLayer(int orderInLayer) => OnRefreshOrderInLayer(orderInLayer);
        void IWindowComponent.OnButtonClick(Button btn) => OnButtonClick(btn);
        void IWindowComponent.OnUpdate() => OnUpdate();

        protected virtual void OnWindowLoaded() { }
        protected virtual void OnDestroyEx() { }
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
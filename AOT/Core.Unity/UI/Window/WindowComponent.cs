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
        public new T Window { get; private set; }
        public sealed override void Create(IWindow window)
        {
            Window = window as T;
            base.Create(window);
        }
    }
    public abstract class WindowComponent : IWindowComponent
    {
        public IWindow Window { get; private set; }

        public virtual void Create(IWindow window)
        {
            Window = window;
            OnCreate();
        }
        void IWindowComponent.Destroy() => OnDestroy();
        void IWindowComponent.AddListeners() => OnAddListeners();
        void IWindowComponent.RemoveListeners() => OnRemoveListeners();
        void IWindowComponent.Open() => OnOpen();
        ETask IWindowComponent.AfterOpenAsync() => OnAfterOpenAsync();
        ETask IWindowComponent.BeforeCloseAsync() => OnBeforeCloseAsync();
        void IWindowComponent.Close() => OnClose();
        void IWindowComponent.SetActive(bool value) => OnSetActive(value);
        void IWindowComponent.Refresh() => OnRefresh();
        void IWindowComponent.RefreshOrderInLayer(int orderInLayer) => OnRefreshOrderInLayer(orderInLayer);
        void IWindowComponent.ButtonClick(Button btn) => OnButtonClick(btn);
        void IWindowComponent.Update() => OnUpdate();

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
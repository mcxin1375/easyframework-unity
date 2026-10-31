/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public abstract class ControllerComponent : IControllerComponent
    {
        public Controller Controller { get; private set; }
        
        public virtual void Create(IController controller)
        {
            Controller = controller as Controller;
            
            OnCreate();
        }
        ETask IControllerComponent.OnPreEnterAsync()
        {
            OnPreEnter();
            return OnPreEnterAsync();
        }
        ETask IControllerComponent.OnEnterAsync()
        {
            OnEnter();
            return OnEnterAsync();
        }
        ETask IControllerComponent.OnAfterEnterAsync()
        {
            OnAfterEnter();
            return OnAfterEnterAsync();
        }
        ETask IControllerComponent.OnExitAsync()
        {
            OnExit();
            return OnExitAsync();
        }
        void IControllerComponent.OnAddListeners() => OnAddListeners();
        void IControllerComponent.OnRemoveListeners() => OnRemoveListeners();
        void IControllerComponent.OnUpdate() => OnUpdate();
        void IControllerComponent.OnLateUpdate() => OnLateUpdate();
        void IControllerComponent.OnDestroy() => OnDestroy();
        void IControllerComponent.OnSetActive(bool isActive) => OnSetActive(isActive);
        protected virtual void OnCreate() { }
        protected virtual void OnPreEnter() { }
        protected virtual ETask OnPreEnterAsync() => ETask.CompletedTask;
        protected virtual void OnEnter() { }
        protected virtual ETask OnEnterAsync() => ETask.CompletedTask;
        protected virtual void OnAfterEnter() { }
        protected virtual ETask OnAfterEnterAsync() => ETask.CompletedTask;
        protected virtual void OnExit() { }
        protected virtual ETask OnExitAsync() => ETask.CompletedTask;
        protected virtual void OnSetActive(bool isActive) { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnDestroy() { }
    }
}
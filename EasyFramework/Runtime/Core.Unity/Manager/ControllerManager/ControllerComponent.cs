/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public abstract class ControllerComponent<T> : ControllerComponent where T : class, IController
    {
        public new T Controller { get; private set; }
        public sealed override void Create(IController ctr)
        {
            Controller = ctr as T;
            base.OnCreate();
        }
    }
    public abstract class ControllerComponent : IControllerComponent
    {
        public ControllerBase Controller { get; private set; }
        public readonly Type Type;
        
        protected ControllerComponent()
        {
            Type = GetType();
        }
        public virtual void Create(IController controller)
        {
            Controller = controller as ControllerBase;
            
            OnCreate();
        }
        ETask IControllerComponent.OnBeforeEnterAsync(EControllerEnter enter)
        {
            OnBeforeEnter(enter);
            return OnBeforeEnterAsync(enter);
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
        ETask IControllerComponent.OnSetActiveAsync(bool isActive)
        {
            OnSetActive(isActive);
            return OnSetActiveAsync(isActive);
        }
        void IControllerComponent.OnAddListeners() => OnAddListeners();
        void IControllerComponent.OnRemoveListeners() => OnRemoveListeners();
        void IControllerComponent.OnUpdate() => OnUpdate();
        void IControllerComponent.OnLateUpdate() => OnLateUpdate();
        void IControllerComponent.OnDestroy() => OnDestroy();
        protected virtual void OnCreate() { }
        protected virtual void OnBeforeEnter(EControllerEnter enter) { }
        protected virtual ETask OnBeforeEnterAsync(EControllerEnter enter) => ETask.CompletedTask;
        protected virtual void OnEnter() { }
        protected virtual ETask OnEnterAsync() => ETask.CompletedTask;
        protected virtual void OnAfterEnter() { }
        protected virtual ETask OnAfterEnterAsync() => ETask.CompletedTask;
        protected virtual void OnExit() { }
        protected virtual ETask OnExitAsync() => ETask.CompletedTask;
        protected virtual void OnSetActive(bool isActive) { }
        protected virtual ETask OnSetActiveAsync(bool isActive) => ETask.CompletedTask;
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnDestroy() { }
    }
}
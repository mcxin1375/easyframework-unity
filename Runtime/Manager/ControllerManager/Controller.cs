/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public abstract partial class Controller<T> : IController, ITimerObject
    {
        public event Action OnEnterEvent;
        public event Action OnExitEvent;

        public virtual bool IsTimerAlive => IsEnter && IsActive;
        protected virtual bool UpdateEnabled => IsEnter && IsActive;
        protected virtual bool ExitOtherBeforeEnter => false;
        
        public bool IsEnter => ControllerManager.Instance.HasEnter(this);
        public bool IsActive => ControllerManager.Instance.HasActive(this);

        // public readonly ControllerResLoader ResLoader = new();
        // public readonly ControllerLoading Loading = new();
        
        private IControllerComponent[] _components;

        private void Create()
        {
            FDebug.Log($"{GetType().Name} - OnCreate()");
            
            _components = ReflectionUtility.FindFieldsAndProperties<IControllerComponent>(this);
            OnCreate();
            foreach (var ex in _components) ex.Create(this);
        }
        
        private void Destroy()
        {
            FDebug.Log($"{GetType().Name} - Destroy()");
            
            OnDestroy();
            foreach (var ex in _components) ex.OnDestroy();
        }

        void IController.OnUpdate()
        {
            if (!UpdateEnabled) return;
            
            OnUpdate();
            foreach (var ex in _components) ex.OnUpdate();
        }
        void IController.OnLateUpdate()
        {
            if (!UpdateEnabled) return;
            
            OnLateUpdate();
            foreach (var ex in _components) ex.OnLateUpdate();
        }
        
        void IController.OnSetActive(bool isActive)
        {
            FDebug.Log($"{GetType().Name} - SetActive({isActive})");

            OnSetActive(isActive);
            foreach (var ex in _components) ex.OnSetActive(isActive);
        }

        protected virtual ETask OnPreEnterAsync() => ETask.CompletedTask;
        protected virtual void OnEnter() { }
        protected virtual ETask OnEnterAsync() => ETask.CompletedTask;
        protected virtual void OnAfterEnter() { }
        protected virtual ETask OnAfterEnterAsync() => ETask.CompletedTask;
        protected virtual void OnExit() { }
        protected virtual ETask OnExitAsync() => ETask.CompletedTask;
        protected virtual void OnSetActive(bool isActive) { }
        protected virtual void OnCreate() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnDestroy() { }
        
        public async ETask EnterAsync()
        {
            if (IsEnter)
            {
                FDebug.LogWarning($"{GetType().Name} is already enter!");
                return;
            }
            FDebug.Log($"{GetType().Name} - EnterAsync()");
            
            await OnPreEnterAsync();
            foreach (var ex in _components) await ex.OnPreEnterAsync();
            
            if (ExitOtherBeforeEnter)
            {
                await ControllerManager.Instance.ExitAllAsync();
            }
            ControllerManager.Instance.Enter(this);

            OnAddListeners();
            foreach (var ex in _components) ex.OnAddListeners();
            
            OnEnter();
            await OnEnterAsync();
            foreach (var ex in _components) await ex.OnEnterAsync();

            OnAfterEnter();
            await OnAfterEnterAsync();
            foreach (var ex in _components) await ex.OnAfterEnterAsync();
            
            OnEnterEvent?.Invoke();
        }
        
        public async ETask ExitAsync()
        {
            if (!IsEnter)
            {
                FDebug.LogWarning($"{GetType().Name} is already exit!");
                return;
            }
            FDebug.Log($"{GetType().Name} - ExitAsync()");
            
            ControllerManager.Instance.Remove(this);
            
            OnRemoveListeners();
            foreach (var ex in _components) ex.OnRemoveListeners();

            OnExit();
            await OnExitAsync();
            foreach (var ex in _components) await ex.OnExitAsync();
            
            OnExitEvent?.Invoke();
        }
    }
}

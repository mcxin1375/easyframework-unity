/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public abstract class ControllerBase : IController, ITimerObject
    {
        public event Action OnEnterAction;
        public event Action OnExitAction;

        public virtual bool IsTimerAlive => IsEnter && IsActive;
        protected virtual bool UpdateEnabled => IsEnter && IsActive;
        protected virtual bool DebugEnabled => false;
        protected virtual bool AfterEnterEnabled => IsActive;
        
        public Type Type { get; }
        public bool IsEnter { get; private set; }
        public bool IsActive { get; private set; }

        public readonly ControllerResLoader ResLoader = new();
        public readonly ControllerLoading Loading = new();
        
        private IControllerComponent[] _components;

        protected ControllerBase()
        {
            Type = GetType();
        }
        void IController.Create()
        {
            if (DebugEnabled) FDebug.Log($"{Type.Name} - Create()");
            
            _components = EasyFrameworkReflection.FindFieldsAndProperties<IControllerComponent>(this);
            
            OnCreate();
            if (this is IControllerLoading loadingHandler) Loading.Add(loadingHandler);
            
            foreach (var ex in _components)
            {
                ex.Create(this);
                if (ex is IControllerLoading handler) Loading.Add(handler);
            }
        }

        async ETask IController.BeforeEnterAsync(EControllerEnter enter)
        {
            if (DebugEnabled) FDebug.Log($"{Type.Name} - BeforeEnterAsync()");

            if (IsEnter) return;
            
            OnBeforeEnter(enter);
            await OnBeforeEnterAsync(enter);
            foreach (var ex in _components) await ex.OnBeforeEnterAsync(enter);
        }

        async ETask IController.EnterAsync(EControllerEnter enter)
        {
            if (DebugEnabled) FDebug.Log($"{Type.Name} - EnterAsync()");

            var isActive = enter == EControllerEnter.Single || enter == EControllerEnter.Additive;
            if (IsEnter)
            {
                await (this as IController).SetActiveAsync(isActive);
                return;
            }

            IsEnter = true;
            
            OnAddListeners();
            foreach (var ex in _components) ex.OnAddListeners();
            
            OnStartLoading();
            await Loading.StartLoadingAsync();
            
            OnEnter();
            await OnEnterAsync();
            foreach (var ex in _components) await ex.OnEnterAsync();

            await (this as IController).SetActiveAsync(isActive);
            
            OnEnterAction?.Invoke();

            if (AfterEnterEnabled)
            {
                OnAfterEnter();
                await OnAfterEnterAsync();
                foreach (var ex in _components) await ex.OnAfterEnterAsync();
            }
        }
        async ETask IController.ExitAsync()
        {
            if (DebugEnabled) FDebug.Log($"{Type.Name} - ExitAsync()");

            if (!IsEnter) return;
            IsEnter = false;
            
            OnRemoveListeners();
            foreach (var ex in _components) ex.OnRemoveListeners();

            await (this as IController).SetActiveAsync(false);
            
            OnExit();
            await OnExitAsync();
            foreach (var ex in _components) await ex.OnExitAsync();
            
            OnExitAction?.Invoke();
        }

        async ETask IController.SetActiveAsync(bool isActive)
        {
            if (DebugEnabled) FDebug.Log($"{Type.Name} - SetActiveAsync({isActive})");

            // if (IsActive == isActive) return;
            IsActive = isActive;
            
            OnSetActive(isActive);
            await OnSetActiveAsync(isActive);
            foreach (var ex in _components) await ex.OnSetActiveAsync(isActive);
        }
        void IController.Update()
        {
            if (!UpdateEnabled) return;
            
            OnUpdate();
            foreach (var ex in _components) ex.OnUpdate();
        }
        void IController.LateUpdate()
        {
            if (!UpdateEnabled) return;
            
            OnLateUpdate();
            foreach (var ex in _components) ex.OnLateUpdate();
        }
        void IController.Destroy()
        {
            if (DebugEnabled) FDebug.Log($"{Type.Name} - Destroy()");
            
            OnDestroy();
            foreach (var ex in _components) ex.OnDestroy();
        }

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
        protected virtual void OnCreate() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnStartLoading() { }
    }
}
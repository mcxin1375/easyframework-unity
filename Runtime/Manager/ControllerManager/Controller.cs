/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public abstract class Controller : IController, ITimerObject, IBehaviourEvent
    {
        public event Action OnEnterAction;
        public event Action OnExitAction;

        public virtual bool IsTimerAlive => IsEnter && IsActive;
        protected virtual bool UpdateEnabled => IsEnter && IsActive;
        
        public Type Type { get; }
        public bool IsEnter { get; private set; }
        public bool IsActive { get; private set; }

        public readonly ControllerResLoader ResLoader = new();
        public readonly ControllerLoading Loading = new();
        
        private IControllerComponent[] _components;

        protected Controller()
        {
            Type = GetType();
            F.Behaviour.Add(this);
        }
        protected void Initialize()
        {
            // FDebug.Log($"{Type.Name} - Initialize()");
            
            _components = ReflectionUtility.FindFieldsAndProperties<IControllerComponent>(this);
            OnCreate();
            foreach (var ex in _components) ex.Create(this);
        }

        void IBehaviourEvent.OnUpdate()
        {
            if (!UpdateEnabled) return;
            
            OnUpdate();
            foreach (var ex in _components) ex.OnUpdate();
        }
        void IBehaviourEvent.OnLateUpdate()
        {
            if (!UpdateEnabled) return;
            
            OnLateUpdate();
            foreach (var ex in _components) ex.OnLateUpdate();
        }
        void IBehaviourEvent.OnDestroy()
        {
            FDebug.Log($"{Type.Name} - OnDestroy()");
            
            OnDestroy();
            foreach (var ex in _components) ex.OnDestroy();
        }
        
        void IController.SetActive(bool isActive)
        {
            FDebug.Log($"{Type.Name} - SetActive({isActive})");

            IsActive = isActive;
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
            FDebug.Log($"{Type.Name} - EnterAsync()");
            
            if (IsEnter) return;
            
            await OnPreEnterAsync();
            foreach (var ex in _components) await ex.OnPreEnterAsync();
            
            F.ControllerManager.Enter(this);

            OnAddListeners();
            foreach (var ex in _components) ex.OnAddListeners();
            
            OnEnter();
            await OnEnterAsync();
            foreach (var ex in _components) await ex.OnEnterAsync();

            // await (this as IController).SetActiveAsync(isActive);

            OnAfterEnter();
            await OnAfterEnterAsync();
            foreach (var ex in _components) await ex.OnAfterEnterAsync();
            
            OnEnterAction?.Invoke();
        }
        
        public async ETask ExitAsync()
        {
            FDebug.Log($"{Type.Name} - ExitAsync()");

            if (!IsEnter) return;
            
            F.ControllerManager.Remove(this);
            
            OnRemoveListeners();
            foreach (var ex in _components) ex.OnRemoveListeners();

            // await (this as IController).SetActiveAsync(false);
            
            OnExit();
            await OnExitAsync();
            foreach (var ex in _components) await ex.OnExitAsync();
            
            OnExitAction?.Invoke();
        }

        // public ETask EnterAsync(EControllerEnter mode = EControllerEnter.Additive) => F.ControllerManager.EnterAsync<T>(mode);
        // public ETask EnterAsync<TK1>(in TK1 tk1, EControllerEnter mode = EControllerEnter.Additive) => F.ControllerManager.EnterAsync(this, in tk1, mode);
        // public ETask EnterAsync<TK1, TK2>(in TK1 tk1, in TK2 tk2, EControllerEnter mode = EControllerEnter.Additive) => F.ControllerManager.EnterAsync(this, in tk1, in tk2, mode);
        // public ETask EnterAsync<TK1, TK2, TK3>(in TK1 tk1, in TK2 tk2, in TK3 tk3, EControllerEnter mode = EControllerEnter.Additive) => F.ControllerManager.EnterAsync(this, in tk1, in tk2, in tk3, mode);
        // public ETask EnterAsync(Type type, EControllerEnter mode = EControllerEnter.Additive) => F.ControllerManager.EnterAsync(type, mode);
        // public ETask EnterAsync(IController controller, EControllerEnter mode = EControllerEnter.Additive) => F.ControllerManager.EnterAsync(controller, mode);
        // public ETask SetActiveAsync(bool isActive) => F.ControllerManager.SetActiveAsync<T>(isActive);
        //
        // public ETask ExitAsync() => F.ControllerManager.ExitAsync<T>();
        // public ETask ExitAsync(Type type) => F.ControllerManager.ExitAsync(type);
        // public ETask ExitAllAsync() => F.ControllerManager.ExitAllAsync();
    }

    public abstract class Controller<T> : Controller where T : Controller<T>, new()
    {
        private static T _instance;
        public static T Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.Initialize();
                }
                return _instance;
            }
        }
    }
}

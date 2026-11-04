using System;

namespace EasyFramework
{
    public abstract partial class Controller<T> : IController, ITimerObject
    {
        public event Action OnEnterEvent;
        public event Action OnExitEvent;

        private IControllerComponent[] _components = Array.Empty<IControllerComponent>();
        private long _lifecycleVersion;
        private long _activationVersion;
        private bool _listenersStarted;

        public bool IsEnter => F.ControllerManager.HasEnter(this);
        public bool IsActive => F.ControllerManager.HasActive(this);
        public virtual bool IsTimerAlive => IsEnter && IsActive;
        protected virtual bool UpdateEnabled => IsEnter && IsActive;

        // Events belong to these instance entry points, after the manager releases its operation.
        public ETask<T> EnterAsync() => CompleteEnter(ControllerManager.Instance.EnterInstanceAsync((T)this));
        public ETask<T> EnterAsync<T1>(in T1 p1)
            => CompleteEnter(ControllerManager.Instance.EnterWithParamsAsync<T, T1>((T)this, in p1));
        public ETask<T> EnterAsync<T1, T2>(in T1 p1, in T2 p2)
            => CompleteEnter(ControllerManager.Instance.EnterWithParamsAsync<T, T1, T2>((T)this, in p1, in p2));
        public ETask<T> EnterAsync<T1, T2, T3>(in T1 p1, in T2 p2, in T3 p3)
            => CompleteEnter(ControllerManager.Instance.EnterWithParamsAsync<T, T1, T2, T3>((T)this, in p1, in p2, in p3));
        public async ETask ExitAsync()
        {
            bool wasEntered = IsEnter;
            await F.ControllerManager.ExitAsync(this);
            if (wasEntered) OnExitEvent?.Invoke();
        }

        private async ETask<T> CompleteEnter(ETask<T> operation)
        {
            var controller = await operation;
            OnEnterEvent?.Invoke();
            return controller;
        }

        bool IController.UpdateEnabled => UpdateEnabled;
        void IController.OnCreate()
        {
            _components = ReflectionUtility.FindFieldsAndProperties<IControllerComponent>(this);
            foreach (var component in _components) component.Register(this);
            OnCreate();
            foreach (var component in _components) component.OnCreate();
        }
        async ETask IController.OnPreEnterAsync()
        {
            // A new enter request invalidates component work from an older frame callback.
            _lifecycleVersion++;
            await OnPreEnterAsync();
            foreach (var component in _components) await component.OnPreEnterAsync();
        }
        async ETask IController.OnEnterAsync()
        {
            if (!_listenersStarted)
            {
                _listenersStarted = true;
                OnAddListeners();
                foreach (var component in _components) component.OnAddListeners();
            }
            OnEnter();
            await OnEnterAsync();
            foreach (var component in _components) await component.OnEnterAsync();

            OnAfterEnter();
            await OnAfterEnterAsync();
            foreach (var component in _components) await component.OnAfterEnterAsync();
        }
        async ETask IController.OnExitAsync()
        {
            _lifecycleVersion++;
            if (_listenersStarted)
            {
                _listenersStarted = false;
                OnRemoveListeners();
                foreach (var component in _components) component.OnRemoveListeners();
            }
            OnExit();
            await OnExitAsync();
            foreach (var component in _components) await component.OnExitAsync();
        }
        void IController.OnSetActive(bool active)
        {
            long activation = ++_activationVersion;
            OnSetActive(active);
            foreach (var component in _components)
            {
                if (activation != _activationVersion || IsActive != active) break;
                component.OnSetActive(active);
            }
        }
        void IController.OnUpdate()
        {
            long version = _lifecycleVersion;
            OnUpdate();
            foreach (var component in _components)
            {
                if (version != _lifecycleVersion || !UpdateEnabled) break;
                component.OnUpdate();
            }
        }
        void IController.OnLateUpdate()
        {
            long version = _lifecycleVersion;
            OnLateUpdate();
            foreach (var component in _components)
            {
                if (version != _lifecycleVersion || !UpdateEnabled) break;
                component.OnLateUpdate();
            }
        }
        void IController.OnDestroy()
        {
            _lifecycleVersion++;
            OnDestroy();
            foreach (var component in _components) component.OnDestroy();
            _components = Array.Empty<IControllerComponent>();
        }

        protected virtual void OnCreate() { }
        protected virtual ETask OnPreEnterAsync() => ETask.CompletedTask;
        protected virtual void OnAddListeners() { }
        protected virtual void OnEnter() { }
        protected virtual ETask OnEnterAsync() => ETask.CompletedTask;
        protected virtual void OnAfterEnter() { }
        protected virtual ETask OnAfterEnterAsync() => ETask.CompletedTask;
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnExit() { }
        protected virtual ETask OnExitAsync() => ETask.CompletedTask;
        protected virtual void OnSetActive(bool active) { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnDestroy() { }
    }
}

/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IEntityFSMState
    {
        string Name { get; }
        protected internal void Create(FEntity entity);
        protected internal void Destroy();
        protected internal void Enter();
        protected internal void Exit();
        protected internal void Update();
        protected internal void LateUpdate();
    }
    public abstract class EntityFSMState : IEntityFSMState
    {
        public string Name { get; }
        protected FEntity Entity { get; private set; }
        protected bool IsEnter { get; private set; }

        protected EntityFSMState()
        {
            Name = GetType().Name;
        }
        void IEntityFSMState.Create(FEntity entity)
        {
            Entity = entity;
            OnCreate();
        }
        void IEntityFSMState.Destroy() => OnDestroy();
        void IEntityFSMState.Enter()
        {
            if (!IsEnter)
            {
                IsEnter = true;
                OnAddListeners();
            }
            OnEnter();
        }
        void IEntityFSMState.Exit()
        {
            if (IsEnter)
            {
                IsEnter = false;
                OnRemoveListeners();
            }
            OnExit();
        }
        void IEntityFSMState.Update() => OnUpdate();
        void IEntityFSMState.LateUpdate() => OnLateUpdate();
        
        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
    }
}

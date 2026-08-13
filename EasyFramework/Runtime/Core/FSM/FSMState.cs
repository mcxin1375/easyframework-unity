/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public abstract class FSMState : FSM.IState
    {
        public string Name => GetType().Name;
        public bool IsEnter { get; private set; }

        void FSM.IState.Create(object[] args)
        {
            OnCreate(args);
        }
        void FSM.IState.Enter()
        {
            if (!IsEnter)
            {
                IsEnter = true;
                OnAddListeners();
            }
            OnEnter();
        }
        void FSM.IState.Exit()
        {
            if (IsEnter)
            {
                IsEnter = false;
                OnRemoveListeners();
            }
            OnExit();
        }

        void FSM.IState.Destroy()
        {
            OnDestroy();
        }
        protected virtual void OnCreate(object[] args) { }
        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnDestroy() { }
    }
}

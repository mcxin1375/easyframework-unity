/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public abstract class FSMTickState : FSM.IState, ITickerNode
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
            
            ETask.AddTick(this);
        }
        void FSM.IState.Exit()
        {
            if (IsEnter)
            {
                IsEnter = false;
                OnRemoveListeners();
            }
            OnExit();
            
            ETask.RemoveTick(this);
        }
        void FSM.IState.Destroy()
        {
            ETask.RemoveTick(this);
            
            OnDestroy();
        }

        bool ITickerNode.OnTick()
        {
            if (IsEnter)
                OnTick();
            return IsEnter;
        }

        protected virtual void OnCreate(object[] args) { }
        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnTick() { }
        protected virtual void OnDestroy() { }
    }
}

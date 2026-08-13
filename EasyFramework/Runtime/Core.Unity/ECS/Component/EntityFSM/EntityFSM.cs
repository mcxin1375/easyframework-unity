/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System;

namespace EasyFramework
{
    public class EntityFSM<T> : EntityFSM
    {
        protected override Type FSMType => typeof(T);
    }

    public abstract class EntityFSM : FEntityComponent
    {
        protected abstract Type FSMType { get; }
        
        public IEntityFSMState CurrentState { get; protected set; }
        private readonly Dictionary<string, IEntityFSMState> _stateDict = new ();

        protected override void OnCreate()
        {
            base.OnCreate();

            if (FSMType == null) throw new NullReferenceException("FSMType");
            
            var arr = EasyFrameworkReflection.CreateInstancesByAttribute<IEntityFSMState>(FSMType, FSMType.Assembly);
            foreach (var fsmState in arr)
            {
                fsmState.Create(FEntity);
                _stateDict.Add(fsmState.Name, fsmState);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            foreach (var fsmState in _stateDict.Values) fsmState.Destroy();
            _stateDict.Clear();
            CurrentState = null;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            CurrentState?.Update();
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            
            CurrentState?.LateUpdate();
        }

        public IEntityFSMState GetState(string name) => _stateDict.ContainsKey(name) ? _stateDict[name] : null;
        public T GetState<T>(string name) where T : class, IEntityFSMState => _stateDict.ContainsKey(name) ? _stateDict[name] as T : null;
        public bool IsEnter(string name) => CurrentState?.Name == name;
        public bool IsEnter<T>() where T : class, IEntityFSMState => CurrentState?.Name == typeof(T).Name;

        public void Enter<T>() where T : class, IEntityFSMState => Enter(typeof(T).Name);
        public void Enter(string name)
        {
            var t = GetState(name);
            if (t == null) throw new Exception($"FSM.Enter({name}) state is empty!");
            
            Enter(t);
        }
        protected virtual void Enter(IEntityFSMState enterState)
        {
            if (CurrentState == enterState)
            {
                enterState.Enter();
                return;
            }
            // fsmState.PreEnter();
            CurrentState?.Exit();
            CurrentState = enterState;
            CurrentState.Enter();
        }
    }
}
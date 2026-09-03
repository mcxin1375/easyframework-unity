/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System;
using System.Reflection;

namespace EasyFramework
{

    public class FSM<T> : FSM where T : Attribute, IReflection
    {
        public FSM(params object[] args) : base(typeof(T).GetCustomAttribute<T>().InstanceTypes, args)
        {
            
        }
    }

    public class FSM
    {
        public interface IState
        {
            string Name { get; }
            void Create(object[] parameters);
            void Destroy();
            void Enter();
            void Exit();
        }
        
        public IState CurrentState { get; protected set; }

        private readonly Dictionary<string, IState> _stateDict = new ();

        public FSM(IReflection reflection, params object[] args) : this(reflection.InstanceTypes, args)
        {
        }
        public FSM(Type[] types, params object[] args)
        {
            var arr = ReflectionUtility.CreateInstancesByTypes<IState>(types);
            foreach (var fsmState in arr)
            {
                fsmState.Create(args);
                _stateDict.Add(fsmState.Name, fsmState);
            }
        }

        public void Enter<T, TK1>(TK1 t1) where T : class, IState, ITParams<TK1>, new()
        {
            var t = GetState<T>();
            if (t == null) 
            {
                FDebug.LogError($"FSM.Enter({typeof(T).Name}) state is empty!");
                return;
            }
            t.SetParams(t1);
            Enter(t);
        }
        public void Enter<T, TK1, TK2>(TK1 t1, TK2 t2) where T : class, IState, ITParams<TK1, TK2>, new()
        {
            var t = GetState<T>();
            if (t == null) 
            {
                FDebug.LogError($"FSM.Enter({typeof(T).Name}) state is empty!");
                return;
            }
            t.SetParams(t1, t2);
            Enter(t);
        }
        public void Enter<T, TK1, TK2, TK3>(TK1 t1, TK2 t2, TK3 t3) where T : class, IState, ITParams<TK1, TK2, TK3>, new()
        {
            var t = GetState<T>();
            if (t == null) 
            {
                FDebug.LogError($"FSM.Enter({typeof(T).Name}) state is empty!");
                return;
            }
            t.SetParams(t1, t2, t3);
            Enter(t);
        }
        public void Enter<T>() where T : class, IState, new() => Enter(typeof(T).Name);
        public void Enter(string name)
        {
            var t = GetState(name);
            if (t == null)
            {
                FDebug.LogError($"FSM.Enter({name}) state is empty!");
                return;
            }
            if (t is ITParams tParams) tParams.SetParamsDefault();
            Enter(t);
        }
        
        public IState GetState(string name) => _stateDict.ContainsKey(name) ? _stateDict[name] : null;
        public T GetState<T>() where T : class, IState
        {
            var name = typeof(T).Name;
            return _stateDict.TryGetValue(name, out var value) ? value as T : null;
        }
        
        public bool IsEnter(string name) => CurrentState?.Name == name;
        public bool IsEnter<T>() => CurrentState?.Name == typeof(T).Name;
        
        public void Destroy()
        {
            foreach (var fsmState in _stateDict.Values) fsmState.Destroy();
            _stateDict.Clear();
            CurrentState = null;
        }

        protected virtual void Enter(IState enterState)
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

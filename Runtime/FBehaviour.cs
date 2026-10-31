/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework
{
    public class FBehaviour : SingletonBehaviour<FBehaviour>
    {
        private readonly Ticker _ticker = new();
        
        void Awake()
        {
            transform.name = "[EasyFramework]";
            FDebug.Log($"{transform.name} OnAwake");
            
            Object.DontDestroyOnLoad(gameObject);
        }
        void Update()
        {
            _ticker.Update();
            WorldManager.Instance.Update();
        }
        void LateUpdate()
        {
            _ticker.LateUpdate();
            WorldManager.Instance.LateUpdate();
        }
        void OnDestroy()
        {
            FDebug.Log($"[{transform.name}] OnDestroy");
            
            _ticker.Destroy();
            WorldManager.Instance.Destroy();
        }
        
        public void Add(IBehaviourEvent v) => _ticker.Add(v);
        public void Remove(IBehaviourEvent v) => _ticker.Remove(v);

        class Ticker
        {
            private readonly List<IBehaviourEvent> _updateList = new();
            private readonly List<IBehaviourEvent> _addList = new();
            private bool _needRefresh;

            public void Update()
            {
                if (_needRefresh)
                {
                    _needRefresh = false;
                    _updateList.Clear();
                    foreach (var controller in _addList) _updateList.Add(controller);
                }
                
                foreach (var v in _updateList) v.OnUpdate();
            }
            public void LateUpdate()
            {
                foreach (var v in _updateList) v.OnLateUpdate();
            }
            public void Destroy()
            {
                foreach (var v in _updateList) v.OnDestroy();
            }
            
            public void Add(IBehaviourEvent v)
            {
                if (_addList.Contains(v)) _addList.Remove(v);
                _addList.Add(v);
                _needRefresh = false;
            }

            public void Remove(IBehaviourEvent v)
            {
                if (_addList.Contains(v)) _addList.Remove(v);
                _needRefresh = false;
            }
        }
    }

    public interface IBehaviourEvent
    {
        void OnUpdate();
        void OnLateUpdate();
        void OnDestroy();
    }
}
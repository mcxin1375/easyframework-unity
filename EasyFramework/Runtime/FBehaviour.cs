/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework
{
    public interface IFrameworkTick
    {
        void OnUpdate();
        void OnLateUpdate();
        void OnDestroy();
    }
    public class FBehaviour : MonoBehaviour
    {
        private static int _uid;
        public const string NAME_ID = "[EasyFramework]";
        
        private static FBehaviour _instance;
        public static FBehaviour Instance
        {
            get
            {
                if (_instance == null && _uid == 0)
                {
                    _uid = 1;
                    var gameObject = new GameObject(NAME_ID);
                    _instance = gameObject.AddComponent<FBehaviour>();
                }

                return _instance;
            }
        }
        
        private readonly Ticker _ticker = new();
        
        void Awake()
        {
            FDebug.Log($"{transform.name} OnAwake");
            Object.DontDestroyOnLoad(gameObject);
        }
        void Update()
        {
            _ticker.Update();
        }
        void LateUpdate()
        {
            _ticker.LateUpdate();
        }
        void OnDestroy()
        {
            FDebug.Log($"[{transform.name}] OnDestroy");
            _ticker.Destroy();
        }
        
        public void Add(IFrameworkTick v) => _ticker.Add(v);
        public void Remove(IFrameworkTick v) => _ticker.Remove(v);

        class Ticker
        {
            private readonly List<IFrameworkTick> _updateList = new();
            private readonly List<IFrameworkTick> _addList = new();
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
            
            public void Add(IFrameworkTick v)
            {
                if (_addList.Contains(v)) return;
                _addList.Add(v);
                _needRefresh = true;
            }

            public void Remove(IFrameworkTick v)
            {
                if (_addList.Contains(v)) _addList.Remove(v);
                _needRefresh = true;
            }
        }
    }
}
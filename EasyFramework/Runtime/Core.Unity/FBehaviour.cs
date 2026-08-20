
using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework
{
    public class FBehaviour : SingletonBehaviour<FBehaviour>
    {
        private readonly List<IEvent> _eventList = new();
        
        void Awake()
        {
            transform.name = "[EasyFramework]";
            FDebug.Log($"[{transform.name}] OnAwake");
            
            Object.DontDestroyOnLoad(gameObject);
        }
        void Update()
        {
            foreach (var obj in _eventList) obj.OnUpdate();
            F.World.Update();
        }
        void LateUpdate()
        {
            foreach (var obj in _eventList) obj.OnLateUpdate();
            F.World.LateUpdate();
        }
        private void OnDestroy()
        {
            FDebug.Log($"[{transform.name}] OnDestroy");
            
            foreach (var obj in _eventList) obj.OnDestroy();
            F.World.Destroy();
        }
        internal void Register(IEvent obj)
        {
            _eventList.Add(obj);
        }
        internal interface IEvent
        {
            void OnUpdate();
            void OnLateUpdate();
            void OnDestroy();
        }
    }
}
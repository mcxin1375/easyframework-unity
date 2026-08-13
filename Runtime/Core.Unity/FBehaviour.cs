
using System.Collections.Generic;

namespace EasyFramework
{
    public class FBehaviour : SingletonBehaviour<FBehaviour>
    {
        private readonly List<IEvent> _eventList = new();
        
        void Awake()
        {
            transform.name = "[EasyFramework]";
            FDebug.Log("FBehaviour OnAwake");
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
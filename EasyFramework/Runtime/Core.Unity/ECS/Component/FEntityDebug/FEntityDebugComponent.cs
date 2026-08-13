using UnityEngine;

namespace EasyFramework
{
    public class FEntityDebugComponent : FEntityComponent
    {
        private GameObject _gameObject;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            
            _gameObject = new GameObject(FEntity.ToString());
            _gameObject.AddComponent<FEntityDebug>().Entity = FEntity;
            _gameObject.transform.SetParent(F.Behaviour.transform);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            Object.Destroy(_gameObject);
            _gameObject = null;
        }
    }
}
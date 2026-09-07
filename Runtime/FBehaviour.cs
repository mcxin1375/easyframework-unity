/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public class FBehaviour : SingletonBehaviour<FBehaviour>
    {
        void Awake()
        {
            transform.name = "[EasyFramework]";
            FDebug.Log($"{transform.name} OnAwake");
            
            Object.DontDestroyOnLoad(gameObject);
        }
        void Update()
        {
            ControllerManager.Instance.Update();
            WorldManager.Instance.Update();
        }
        void LateUpdate()
        {
            ControllerManager.Instance.LateUpdate();
            WorldManager.Instance.LateUpdate();
        }
        private void OnDestroy()
        {
            FDebug.Log($"[{transform.name}] OnDestroy");
            
            ControllerManager.Instance.Destroy();
            WorldManager.Instance.Destroy();
        }
    }
}
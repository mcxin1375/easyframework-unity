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
            WorldManager.Instance.Update();
            ControllerManager.Instance.Update();
        }
        void LateUpdate()
        {
            WorldManager.Instance.LateUpdate();
            ControllerManager.Instance.LateUpdate();
        }
        private void OnDestroy()
        {
            FDebug.Log($"[{transform.name}] OnDestroy");
            
            WorldManager.Instance.Destroy();
            ControllerManager.Instance.Destroy();
        }
    }
}
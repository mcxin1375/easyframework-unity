/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public abstract class StaticBehaviour<T> : MonoBehaviour where T : StaticBehaviour<T>
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                FDebug.LogError($"Static Behaviour already exists! Type: {typeof(T).Name}");
                return;
            }

            Instance = this as T;

            OnAwake();
        }
        public static bool HasInstance() => Instance != null;
        protected virtual void OnAwake() { }
    }
}

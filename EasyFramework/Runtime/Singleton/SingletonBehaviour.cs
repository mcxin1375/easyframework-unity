/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static Guid _uid = Guid.Empty;
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null && _uid == Guid.Empty)
                {
                    var gameObject = new GameObject(typeof(T).Name);
                    DontDestroyOnLoad(gameObject);
                    _uid = Guid.NewGuid();
                    _instance = gameObject.AddComponent<T>();
                    if (_instance is SingletonBehaviour<T> singletonBehaviour) singletonBehaviour.OnCreate();
                }

                return _instance;
            }
        }

        public static T CreateInstance() => Instance;
        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }

            _uid = Guid.Empty;
        }
        public static bool HasInstance() => _instance != null;

        /// <summary>
        /// 提前挂载在对象上的组件，需要手动初始化
        /// </summary>
        /// <param name="newInstance"></param>
        protected static void SetSingleton(T newInstance)
        {
            if (_instance != null)
            {
                FDebug.LogError($"{nameof(SingletonBehaviour<T>)} already contains an instance of {typeof(T)}");
                return;
            }

            if (_instance == newInstance) return;

            _uid = Guid.NewGuid();
            _instance = newInstance;
            if (_instance is SingletonBehaviour<T> singletonBehaviour) singletonBehaviour.OnCreate();
        }
        
        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
    }
}

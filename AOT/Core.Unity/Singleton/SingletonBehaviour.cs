/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var gameObject = new GameObject(typeof(T).Name);
                    DontDestroyOnLoad(gameObject);
                    _instance = gameObject.AddComponent<T>();
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
        }
        public static bool HasInstance() => _instance != null;
        
        /// <summary>
        /// 提前挂载在对象上的组件，需要手动初始化
        /// </summary>
        /// <param name="newInstance"></param>
        protected static void SetSingleton(T newInstance) => _instance = newInstance;
    }
}

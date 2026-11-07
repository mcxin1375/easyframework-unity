/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new();
                    if (_instance is Singleton<T> singleton) singleton.OnCreate();
                }

                return _instance;
            }
        }
        
        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }

        public static T CreateInstance() => Instance;

        public static void DestroyInstance()
        {
            if (_instance is Singleton<T> singleton) singleton.OnDestroy();
            _instance = null;
        }

        public static bool HasInstance() => _instance != null;
    }
}
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
                if (_instance == null) _instance = new();
                return _instance;
            }
        }

        public static T CreateInstance() => Instance;
        public static bool HasInstance() => _instance != null;
    }
}
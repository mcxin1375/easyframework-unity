/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public abstract partial class Controller<T> where T : Controller<T>, new()
    {
        private static T _instance;
        public static T Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.Create();
                }
                return _instance;
            }
        }

        public static void DestroyInstance()
        {
            _instance?.Destroy();
            _instance = null;
        }
    }
}

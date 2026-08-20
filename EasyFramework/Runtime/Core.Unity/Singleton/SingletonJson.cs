/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2022/6/1
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public class SingletonJson<T> where T : class, new()
    {
        private static T _instance;
        public static T Instance => _instance ?? LoadOrCreate();

        public static T LoadOrCreate()
        {
            if (_instance == null)
            {
                _instance = F.LocalStorageManager.LoadOrCreate<T>($"{typeof(T).Name}.json", ELocalStorageType.Config);
            }
            return _instance;
        }
        
        public void Save()
        {
            F.LocalStorageManager.SaveObject($"{typeof(T).Name}.json", ELocalStorageType.Config);
        }
    }
}
/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2022/6/1
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public class SingletonConfig<T> where T : SingletonConfig<T>, new()
    {
        public string FilePath { get; private set; }
        
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var filePath = F.LocalStorageManager.GetFilePath($"{typeof(T).Name}.json", ELocalStorageType.Config);
                    _instance = ConfigHelper.LoadOrCreate<T>(filePath);
                    _instance.FilePath = filePath;
                }

                return _instance;
            }
        }

        public void Save() => ConfigHelper.Save(this, FilePath);
    }
}
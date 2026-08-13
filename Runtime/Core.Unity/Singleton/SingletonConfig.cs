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
        public static T Instance => _instance ?? LoadOrCreate();

        public static T LoadOrCreate()
        {
            // var filePath = $"{LocalStorageHelper.Instance.DataPath}/ConfigSingleton/{typeof(T).Name}.json";
            var filePath = FAOT.LocalStorageManager.GetFilePath($"{typeof(T).Name}.json", ELocalStorageType.Config);
            return LoadOrCreate(filePath);
        }
        public static T LoadOrCreate(string filePath)
        {
            _instance = NewtonsoftHelper.LoadOrCreate<T>(filePath);
            _instance.FilePath = filePath;
            return _instance;
        }
        
        public void Save() => SaveTo(FilePath);
        public void SaveTo(string filePath)
        {
            NewtonsoftHelper.Save(filePath, this);
        }
        
        public ETask SaveAsync() => SaveToAsync(FilePath);
        public ETask SaveToAsync(string filePath)
        {
            return ETask.RunOnThreadPool(() =>
            {
                NewtonsoftHelper.Save(filePath, this);
            });
        }
    }
}
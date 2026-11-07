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
                    var filePath = $"{EasyFrameworkSettings.Instance.ConfigPath}/{typeof(T).Name}.json";
                    _instance = ConfigHelper.LoadOrCreate<T>(filePath);
                    _instance.FilePath = filePath;
                    _instance.OnCreate();
                }

                return _instance;
            }
        }

        public void Save()
        {
            ConfigHelper.Save(this, FilePath);
            OnSave();
        }

        protected virtual void OnCreate()
        {
            
        }
        
        protected virtual void OnSave() { }
    }
}
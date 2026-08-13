/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2022/6/1
// describe:
//----------------------------------------------------------------*/


using System;

namespace EasyFramework
{
    public abstract class ConfigBase<T> where T : ConfigBase<T>, new()
    {
        public string FilePath { get; private set; }

        public void Save() => Save(FilePath);
        public void Save(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new Exception("filePath is null or empty");
            NewtonsoftHelper.Save(filePath, this);
        }
        
        public ETask SaveAsync() => SaveAsync(FilePath);
        public ETask SaveAsync(string filePath)
        {
            return ETask.RunOnThreadPool(() =>
            {
                NewtonsoftHelper.Save(filePath, this);
            });
        }

        public static T LoadFromFile(string configFile)
        {
            var t = NewtonsoftHelper.LoadOrCreate<T>(configFile);
            t.FilePath = configFile;
            return t;
        }
    }
}
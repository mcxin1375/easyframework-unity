
using System.IO;
using UnityEngine;

namespace EasyFramework
{
    public class LocalStorage : ILocalStorage
    {
        public virtual string DataPath
        {
            get
            {
#if UNITY_EDITOR
                return Application.persistentDataPath;
#endif
                
#if UNITY_IOS
                return Application.temporaryCachePath;
#else
                return Application.persistentDataPath;
#endif
            }
        }

        public bool Exists(string path) => File.Exists(path);
        public void Delete(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
        public void ClearDirectory(string path)
        {
            FileHelper.ClearDirectory(path);
        }
        public void WriteAllBytes(string path, byte[] bytes)
        {
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir)) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(path, bytes);
        }
        public void WriteAllText(string path, string contents)
        {
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir)) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, contents);
        }
        public byte[] ReadAllBytes(string path) => File.Exists(path) ? File.ReadAllBytes(path) : null;
        public string ReadAllText(string path) => File.Exists(path) ? File.ReadAllText(path) : null;
    }
}
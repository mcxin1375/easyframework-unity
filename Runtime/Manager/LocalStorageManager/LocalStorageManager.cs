
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework
{
    public enum ELocalStorageType
    {
        Untagged,
        DLC,
        DownloadTemp,
        Config,
    }

    public class LocalStorageManager : Singleton<LocalStorageManager>
    {
        public const string DirectoryTag = "Untagged";
        public string DataPath => _localStorage.DataPath;

        private readonly ILocalStorage _localStorage = new LocalStorage();

        private readonly Dictionary<ELocalStorageType, string> _typeDict = new();
        // private readonly Dictionary<string, string> _filePathDict = new();

        public LocalStorageManager()
        {
            foreach (ELocalStorageType type in Enum.GetValues(typeof(ELocalStorageType)))
                _typeDict[type] = type.ToString();
        }

        public string GetFilePath(string fileName, ELocalStorageType type) => GetFilePath(fileName, _typeDict[type]);
        public string GetFilePath(string fileName, string directoryTag = DirectoryTag)
        {
            return $"{DataPath}/{directoryTag}/{fileName}";
        }

        public string GetDirectoryPath(ELocalStorageType type) => GetDirectoryPath(_typeDict[type]);
        public string GetDirectoryPath(string directoryTag)
        {
            return $"{DataPath}/{directoryTag}";
        }

        public void ClearDirectory(ELocalStorageType type) => ClearDirectory(_typeDict[type]);
        public void ClearDirectory(string directoryTag)
        {
            var dir = $"{DataPath}/{directoryTag}";
            _localStorage.ClearDirectory(dir);
        }

        public bool Exists(string fileName, ELocalStorageType type) => Exists(fileName, _typeDict[type]);
        public bool Exists(string fileName, string directoryTag = DirectoryTag)
        {
            var filePath = $"{DataPath}/{directoryTag}/{fileName}";
            return _localStorage.Exists(filePath);
        }
        
        public void Delete(string fileName, ELocalStorageType type) => Delete(fileName, _typeDict[type]);
        public void Delete(string fileName, string directoryTag = DirectoryTag)
        {
            var filePath = $"{DataPath}/{directoryTag}/{fileName}";
            _localStorage.Delete(filePath);
        }
        
        public void SaveObject(string fileName, object obj, ELocalStorageType type) => SaveObject(fileName, obj, _typeDict[type]);
        public void SaveObject(string fileName, object obj, string directoryTag = DirectoryTag)
        {
            var filePath = $"{DataPath}/{directoryTag}/{fileName}";
            _localStorage.WriteAllText(filePath, JsonUtility.ToJson(obj));
        }
        
        public void SaveString(string fileName, string content, ELocalStorageType type) => SaveString(fileName, content, _typeDict[type]);
        public void SaveString(string fileName, string content, string directoryTag = DirectoryTag)
        {
            var filePath = $"{DataPath}/{directoryTag}/{fileName}";
            _localStorage.WriteAllText(filePath, content);
        }
        
        public T LoadObject<T>(string fileName, ELocalStorageType type) => LoadObject<T>(fileName, _typeDict[type]);
        public T LoadObject<T>(string fileName, string directoryTag = DirectoryTag)
        {
            var filePath = $"{DataPath}/{directoryTag}/{fileName}";
            var content = _localStorage.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content)) return default;
            return JsonUtility.FromJson<T>(content);
        }
        
        public T LoadOrCreate<T>(string fileName, ELocalStorageType type) where T : class, new() => LoadOrCreate<T>(fileName, _typeDict[type]);
        public T LoadOrCreate<T>(string fileName, string directoryTag = DirectoryTag) where T : class, new()
        {
            return LoadObject<T>(fileName, directoryTag) ?? new T();
        }

        public byte[] ReadAllBytes(string fileName, ELocalStorageType type) => ReadAllBytes(fileName, _typeDict[type]);
        public byte[] ReadAllBytes(string fileName, string directoryTag = DirectoryTag)
        {
            var filePath = $"{DataPath}/{directoryTag}/{fileName}";
            return _localStorage.ReadAllBytes(filePath);
        }
        
        public string ReadAllText(string fileName, ELocalStorageType type) => ReadAllText(fileName, _typeDict[type]);
        public string ReadAllText(string fileName, string directoryTag = DirectoryTag)
        {
            var filePath = $"{DataPath}/{directoryTag}/{fileName}";
            return _localStorage.ReadAllText(filePath);
        }
    }
}
/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2022/6/1
// describe:
//----------------------------------------------------------------*/

using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace EasyFramework
{
    public static class ConfigHelper
    {
        public static T DeepCopyEx<T>(this T obj)
        {
            var json = ToJson(obj);
            return LoadFromText<T>(json, EConfigEncrypt.None, string.Empty);
        }

        public static T LoadOrCreate<T>(string file) where T : new() => Load<T>(file) ?? new T();
        public static T Load<T>(string filePath)
        {
            if (!File.Exists(filePath)) return default;
            return LoadFromText<T>(File.ReadAllText(filePath));
        }

        public static T LoadOrCreateFromText<T>(string content) where T : new() => LoadFromText<T>(content) ?? new T();
        public static T LoadFromText<T>(string content)
        {
            if (string.IsNullOrEmpty(content)) return default;
            
            var attribute = typeof(T).GetCustomAttribute<ConfigSettingsAttribute>();
            return LoadFromText<T>(content, attribute?.Type ?? EConfigType.Unity, attribute?.Encrypt ?? EConfigEncrypt.None, attribute?.Key);
        }
        public static T LoadFromText<T>(string content, EConfigEncrypt encrypt, string key)
        {
            if (string.IsNullOrEmpty(content)) return default;
            
            var attribute = typeof(T).GetCustomAttribute<ConfigSettingsAttribute>();
            return LoadFromText<T>(content, attribute?.Type ?? EConfigType.Unity, encrypt, key);
        }
        public static T LoadFromText<T>(string content, EConfigType type, EConfigEncrypt encrypt, string key)
        {
            switch (type)
            {
                case EConfigType.Unity:
                    return JsonUtility.FromJson<T>(DecryptContent(content, encrypt, key));
                default:
                    return JsonConvert.DeserializeObject<T>(DecryptContent(content, encrypt, key));
            }
        }

        public static void Save<T>(T config, string filePath, bool format = false)
        {
            var attribute = typeof(T).GetCustomAttribute<ConfigSettingsAttribute>();
            var content = ToJson(config, attribute?.Type ?? EConfigType.Unity, format);
            Save(content, filePath, attribute?.Encrypt ?? EConfigEncrypt.None, attribute?.Key);
        }

        public static void Save(string content, string filePath, EConfigEncrypt encrypt, string key)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            string dirName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            File.WriteAllText(filePath, EncryptContent(content, encrypt, key));
        }

        public static string ToJson<T>(T config, bool format = false)
        {
            var attribute = typeof(T).GetCustomAttribute<ConfigSettingsAttribute>();
            return ToJson(config, attribute?.Type ?? EConfigType.Unity, format);
        }
        public static string ToJson<T>(T config, EConfigType type, bool format = false)
        {
            switch (type)
            {
                case EConfigType.Unity:
                    return JsonUtility.ToJson(config, format);
                case EConfigType.Newtonsoft:
                    return format
                        ? JsonConvert.SerializeObject(config, Formatting.Indented)
                        : JsonConvert.SerializeObject(config);
                default: 
                    return JsonUtility.ToJson(config, format);
            }
        }

        public static string EncryptContent(string content, EConfigEncrypt encrypt, string secretKey)
        {
            switch (encrypt)
            {
                case EConfigEncrypt.DES:
                    return EncryptHelper.EncryptDES(content, secretKey);
                default:
                    return content;
            }
        }

        public static string DecryptContent(string content, EConfigEncrypt encrypt, string secretKey)
        {
            switch (encrypt)
            {
                case EConfigEncrypt.DES:
                    return EncryptHelper.DecryptDES(content, secretKey);
                default:
                    return content;
            }
        }
    }
}


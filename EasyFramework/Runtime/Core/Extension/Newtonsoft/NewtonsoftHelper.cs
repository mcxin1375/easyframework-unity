using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace EasyFramework
{
    public enum EncryptType
    {
        None,
        DES
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NewtonsoftKeyAttribute : Attribute
    {
        public readonly EncryptType EncryptType;
        public readonly string Key;
        public NewtonsoftKeyAttribute(string desKey, EncryptType encryptType)
        {
            Key = desKey;
            EncryptType = encryptType;
        }
    }

    public static class NewtonsoftHelper
    {
        public static string ToJsonEx<T>(this T obj, bool format = false)
        {
            if (format)
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            return JsonConvert.SerializeObject(obj);
        }

        public static string ToJsonEx<T>(this T obj, string secretKey, EncryptType encryptType)
        {
            switch (encryptType)
            {
                case EncryptType.DES:
                    return EncryptHelper.EncryptDES(JsonConvert.SerializeObject(obj), secretKey);
            }
            return JsonConvert.SerializeObject(obj);
        }

        public static T DeepCopyEx<T>(this T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T LoadOrCreate<T>(string file) where T : new() => Load<T>(file) ?? new T();
        public static T LoadOrCreate<T>(string file, string secretKey, EncryptType encryptType) where T : new() => Load<T>(file, secretKey, encryptType) ?? new T();
        public static T Load<T>(string file)
        {
            var keyAttribute = typeof(T).GetCustomAttribute<NewtonsoftKeyAttribute>(true);
            return Load<T>(file, keyAttribute?.Key, keyAttribute?.EncryptType ?? EncryptType.None);
        }
        public static T Load<T>(string file, string secretKey, EncryptType encryptType)
        {
            if (!File.Exists(file)) return default;
            return LoadFromText<T>(File.ReadAllText(file), secretKey, encryptType);
        }

        public static T LoadOrCreateFromText<T>(string content, string secretKey = "", EncryptType encryptType = EncryptType.None) where T : new()
        {
            return LoadFromText<T>(content, secretKey, encryptType) ?? new T();
        }
        public static T LoadFromText<T>(string content, string secretKey = "", EncryptType encryptType = EncryptType.None)
        {
            if (string.IsNullOrEmpty(content)) return default;
            switch (encryptType)
            {
                case EncryptType.DES:
                    return string.IsNullOrWhiteSpace(secretKey)
                        ? JsonConvert.DeserializeObject<T>(content)
                        : JsonConvert.DeserializeObject<T>(EncryptHelper.DecryptDES(content, secretKey));
                default:
                    return JsonConvert.DeserializeObject<T>(content);
            }
        }

        public static void Save(string saveFile, object obj, bool format = false)
        {
            var content = obj.ToJsonEx(format);

            var keyAttribute = obj.GetType().GetCustomAttribute<NewtonsoftKeyAttribute>(true);
            if (keyAttribute != null)
            {
                switch (keyAttribute.EncryptType)
                {
                    case EncryptType.DES:
                        content = EncryptHelper.EncryptDES(content, keyAttribute.Key);
                        break;
                }
            }

            Save(saveFile, content);
        }

        public static void Save(string saveFile, string content)
        {
            if (string.IsNullOrEmpty(saveFile)) return;
            string dirName = Path.GetDirectoryName(saveFile);
            if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            File.WriteAllText(saveFile, content);
        }
    }
}
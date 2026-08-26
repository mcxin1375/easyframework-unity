/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2022/6/1
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public enum EConfigEncrypt
    {
        None,
        DES
    }

    public enum EConfigType
    {
        Unity,
        Newtonsoft,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ConfigSettingsAttribute : Attribute
    {
        public readonly EConfigType Type;
        public readonly EConfigEncrypt Encrypt;
        public readonly string Key;
        public ConfigSettingsAttribute(EConfigType type, EConfigEncrypt encrypt = EConfigEncrypt.None, string key = null)
        {
            Type = type;
            Encrypt = encrypt;
            Key = key;
        }
    }

    public interface IConfig
    {
        string SavePath { get; }
    }

    public static class ConfigExtensions
    {
        public static void Save<T>(this T config) where T : class, IConfig, new() 
        {
            Config<T>.Save(config, config.SavePath);
        }
    }

    public abstract class Config<T> where T : class, new()
    {

        public void Save(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            ConfigHelper.Save(this, filePath);
        }
        
        // public ETask SaveAsync() => SaveAsync(FilePath);
        // public ETask SaveAsync(string filePath)
        // {
        //     return ETask.RunOnThreadPool(() =>
        //     {
        //         NewtonsoftHelper.Save(filePath, this);
        //     });
        // }

        public static T LoadFromFile(string filePath) => ConfigHelper.Load<T>(filePath);
        public static void Save(T config, string filePath) => ConfigHelper.Save(config, filePath);
    }
}


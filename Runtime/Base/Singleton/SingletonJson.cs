/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2022/6/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.Reflection;
using UnityEngine;

namespace EasyFramework
{
    // public enum ESingletonJsonTag
    // {
    //     Resources,
    // }
    //
    // [AttributeUsage(AttributeTargets.Class)]
    // public sealed class SingletonJsonAttribute : Attribute
    // {
    //     public readonly string Path;
    //     public readonly ESingletonJsonTag Tag;
    //     public SingletonJsonAttribute(string path, ESingletonJsonTag tag = ESingletonJsonTag.Resources)
    //     {
    //         Path = path;
    //         Tag = tag;
    //     }
    // }
    
    public class SingletonJson<T> where T : class, new()
    {
        private static T _instance;
        public static T Instance => _instance ?? LoadOrCreate();

        // public static string AttributePath => typeof(T).GetCustomAttribute<SingletonJsonAttribute>()?.Path;

        public static T LoadOrCreate()
        {
            if (_instance == null)
            {
                // var attribute = typeof(T).GetCustomAttribute<SingletonJsonAttribute>();

                // switch (attribute.Tag)
                // {
                //     case ESingletonJsonTag.Resources:
                //         var ta = Resources.Load<TextAsset>(attribute.Path);
                //         _instance = UnityJsonHelper.LoadOrCreateFromText<T>(ta?.text);
                //         break;
                // }
                // _instance = F.LocalStorageManager.LoadOrCreate<T>($"{typeof(T).Name}.json", ELocalStorageType.Config);
            }
            return _instance;
        }
        
        public void Save()
        {
            F.LocalStorageManager.SaveObject($"{typeof(T).Name}.json", ELocalStorageType.Config);
        }
    }
}
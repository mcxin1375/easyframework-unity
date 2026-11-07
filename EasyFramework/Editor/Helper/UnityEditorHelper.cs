/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyFramework.Editor
{
    public static partial class UnityEditorHelper
    {
        // public static string GetPlatformName() => GetPlatformName(EditorUserBuildSettings.activeBuildTarget);
        // public static string GetPlatformName(BuildTarget target)
        // {
        //     switch (target)
        //     {
        //         case UnityEditor.BuildTarget.Android:
        //             return "Android";
        //         case UnityEditor.BuildTarget.iOS:
        //             return "IOS";
        //         case UnityEditor.BuildTarget.StandaloneWindows:
        //         case UnityEditor.BuildTarget.StandaloneWindows64:
        //             return "Windows";
        //         case UnityEditor.BuildTarget.StandaloneOSX:
        //             return "OSX";
        //         default:
        //             return target.ToString();
        //     }
        // }
        
        public static T[] FindAssets<T>(string[] searchInFolders = null) where T : UnityEngine.Object
        {
            List<T> tempList = new List<T>();
            string[] guids = searchInFolders == null
                ? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                : AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchInFolders);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) tempList.Add(asset);
            }
            return tempList.ToArray();
        }
        
        public static T FindAsset<T>(string name, string[] searchInFolders = null) where T : UnityEngine.Object
        {
            string[] guids = searchInFolders == null
                ? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                : AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchInFolders);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) return asset;
            }
            return null;
        }
        
        public static Object[] FindAssetsByType(Type type, string[] searchInFolders = null)
        {
            List<Object> tempList = new List<Object>();
            string[] guids = searchInFolders == null
                ? AssetDatabase.FindAssets($"t:{type.Name}")
                : AssetDatabase.FindAssets($"t:{type.Name}", searchInFolders);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, type);
                if (asset != null) tempList.Add(asset);
            }
            return tempList.ToArray();
        }
    }
}
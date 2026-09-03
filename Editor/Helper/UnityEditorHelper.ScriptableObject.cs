using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static partial class UnityEditorHelper
    {
        public static T LoadOrCreateScriptableObject<T>(string filePath) where T : ScriptableObject
        {
            return LoadScriptableObject<T>(filePath) ?? ScriptableObject.CreateInstance<T>();
        }
        
        public static ScriptableObject LoadOrCreateScriptableObject(string filePath, Type type)
        {
            return LoadScriptableObject(filePath) ?? ScriptableObject.CreateInstance(type);
        }
        
        public static T LoadScriptableObject<T>(string filePath) where T : ScriptableObject
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                if (arr.Length > 0 && arr[0] is T t) return t;
            }
            return null;
        }
        
        public static ScriptableObject LoadScriptableObject(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                if (arr.Length > 0 && arr[0] is ScriptableObject t) return t;
            }
            return null;
        }
        
        public static void SaveScriptableObject(string saveFile, ScriptableObject scriptableObject, bool saveAsText = true)
        {
            if (string.IsNullOrEmpty(saveFile)) return;
            if (scriptableObject == null || EditorUtility.IsPersistent(scriptableObject)) return;

            string directoryName = Path.GetDirectoryName(saveFile);
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

            scriptableObject.name = Path.GetFileNameWithoutExtension(saveFile);
            UnityEngine.Object[] obj = { scriptableObject };
            InternalEditorUtility.SaveToSerializedFileAndForget(obj, saveFile, saveAsText);
        }
        
        public static T LoadProjectSettings<T>() where T : ProjectSettings<T>
        {
            var attribute = ReflectionUtility.GetCustomAttribute<ProjectSettingsAttribute>(typeof(T));
            if (attribute == null) return null;
            switch (attribute.Tag)
            {
                case ProjectSettingsAttribute.ETag.Resources:
                    return Resources.Load<T>(typeof(T).Name);
                case ProjectSettingsAttribute.ETag.Editor:
                    return LoadScriptableObject<T>(GetProjectSettingsFilePath(typeof(T)));
                default:
                    var obj = AssetDatabase.LoadMainAssetAtPath(GetProjectSettingsFilePath(typeof(T)));
                    if (obj is T t) return t;
                    break;
            }
            return ScriptableObject.CreateInstance(typeof(T)) as T;
        }

        public static string GetProjectSettingsFilePath(Type type)
        {
            var attribute = ReflectionUtility.GetCustomAttribute<ProjectSettingsAttribute>(type);
            if (attribute == null) return null;
            switch (attribute.Tag)
            {
                case ProjectSettingsAttribute.ETag.Resources:
                    return $"{EasyFrameworkPreferences.AssetsDataResourcesPath}/{type.Name}.asset";
                // case ProjectSettingsAttribute.ETag.Resources:
                    // return $"{EasyFrameworkPreferences.AssetsDataResourcesPath}/{typeof(T).Name}.asset";
                case ProjectSettingsAttribute.ETag.Editor:
                    return $"ProjectSettings/EasyFramework/{type.Name}.asset";
                default:
                    return attribute.FilePath;
            }
        }

        public static void SaveEx<T>(this T obj) where T : ProjectSettings<T>
        {
            var assetPath = GetProjectSettingsFilePath(obj.GetType());
            SaveScriptableObject(assetPath, obj);
        }
    }
}
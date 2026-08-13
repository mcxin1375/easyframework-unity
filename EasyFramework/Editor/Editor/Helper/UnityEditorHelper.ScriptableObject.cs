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
        
        public static T LoadProjectSettings<T>() where T : ScriptableObject
        {
            var attribute = EasyFrameworkReflection.GetCustomAttribute<ProjectSettingsTagAttribute>(typeof(T));
            if (attribute == null) return null;

            var fullPath = GetProjectSettingsFullPath(typeof(T), attribute.SettingsTag);
            var o = attribute.SettingsTag == EProjectSettingsTag.Editor ? LoadScriptableObject<T>(fullPath) : AssetDatabase.LoadMainAssetAtPath(fullPath);
            if (o is T t) return t;
            return ScriptableObject.CreateInstance(typeof(T)) as T;
        }

        public static string GetProjectSettingsFullPath(Type type, EProjectSettingsTag settingsTag)
        {
            var attribute = EasyFrameworkReflection.GetCustomAttribute<ProjectSettingsAttribute>(type);
            var basePath = string.Empty;
            switch (settingsTag)
            {
                case EProjectSettingsTag.Resources:
                    basePath = attribute?.BasePath ?? EasyFrameworkPreferences.AssetsDataResourcesPath;
                    break;
                case EProjectSettingsTag.AssetBundle:
                    basePath = attribute?.BasePath ?? EasyFrameworkPreferences.AssetsDataDLCPath;
                    break;
                case EProjectSettingsTag.Editor:
                    basePath = attribute?.BasePath ?? $"ProjectSettings/EasyFramework";
                    break;
            }
            return $"{basePath}/{type.Name}.asset";
        }

        public static void SaveEx(this ScriptableObject obj)
        {
            var attribute = EasyFrameworkReflection.GetCustomAttribute<ProjectSettingsTagAttribute>(obj.GetType());
            if (attribute != null)
            {
                var assetPath = GetProjectSettingsFullPath(obj.GetType(), attribute.SettingsTag);
                SaveScriptableObject(assetPath, obj);
            }
        }
    }
}
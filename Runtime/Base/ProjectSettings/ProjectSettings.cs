/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2024/5/10
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    public abstract class ProjectSettings<T> : ScriptableObject where T : ProjectSettings<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null) CreateInstance();
                return _instance;
            }
        }
        
        public static T CreateInstance()
        {
            if (_instance != null) return _instance;

#if UNITY_EDITOR
            _instance = ReloadEditorOnly();
            return _instance;
#endif

            var attribute = EasyFrameworkReflection.GetCustomAttribute<ProjectSettingsAttribute>(typeof(T));
            if (attribute != null)
            {
                switch (attribute.Tag)
                {
                    case ProjectSettingsAttribute.ETag.Resources:
                        _instance = Resources.Load<T>(typeof(T).Name);
                        break;
                    case ProjectSettingsAttribute.ETag.Editor:
                        FDebug.Log($"Load {typeof(T).Name} error!");
                        break;
                }
            }
            if (_instance == null) _instance = ScriptableObject.CreateInstance<T>();
            _instance.OnCreate();
            return _instance;
        }

#if UNITY_EDITOR

        public static T ReloadEditorOnly()
        {
            _instance = EditorBridge.LoadProjectSetting<T>();
            if (_instance == null) _instance = ScriptableObject.CreateInstance<T>();
            _instance.OnCreate();
            return _instance;
        }

#endif

        protected virtual void OnCreate() { }
    }
}
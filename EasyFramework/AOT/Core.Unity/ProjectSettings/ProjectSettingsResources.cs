/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2024/5/10
// describe:
//----------------------------------------------------------------*/

using System.IO;
using UnityEngine;

namespace EasyFramework
{
    [ProjectSettingsTag(EProjectSettingsTag.Resources)]
    public abstract class ProjectSettingsResources<T> : ScriptableObject where T : ProjectSettingsResources<T>
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
            var type = typeof(T);
            var attribute = EasyFrameworkReflection.GetCustomAttribute<ProjectSettingsAttribute>(type);
            if (attribute != null)
            {
                _instance = Resources.Load<T>(Path.Combine(attribute.BasePath, type.Name)) ?? ScriptableObject.CreateInstance<T>();
            }
            else
            {
                _instance = Resources.Load<T>(type.Name) ?? ScriptableObject.CreateInstance<T>();
            }
            _instance.OnCreate();
            
            return _instance;
        }
        
        protected virtual void OnCreate() { }
    }
}
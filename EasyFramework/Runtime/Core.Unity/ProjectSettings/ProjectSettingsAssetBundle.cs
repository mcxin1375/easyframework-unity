/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/11/8
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    [ProjectSettingsTag(EProjectSettingsTag.AssetBundle)]
    public abstract class ProjectSettingsAssetBundle<T> : ScriptableObject where T : ProjectSettingsAssetBundle<T>
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
#if UNITY_EDITOR
            _instance = EditorBridge.LoadProjectSetting<T>();
            _instance.OnCreate();
            return _instance;
#endif
            _instance = F.ResLoader.LoadAsset<T>(typeof(T).Name) ?? CreateInstance<T>();
            _instance.OnCreate();
            return _instance;
        }

        public static async ETask CreateInstanceAsync()
        {
            _instance = await F.ResLoader.LoadAssetAsync<T>(typeof(T).Name);
            _instance ??= CreateInstance<T>();
            _instance.OnCreate();
        }
        
        protected virtual void OnCreate() { }
    }
}
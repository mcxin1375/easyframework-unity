/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EasyFrameworkProvider : ProjectSettingsProvider<EasyFrameworkSettings>
    {
        public const string SettingPath = "Project/EasyFramework";
        
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<EasyFrameworkProvider>.Instance;
        
        
        public EasyFrameworkProvider() : base(SettingPath) { }
        public static string ToChildProvider(string providerName) => $"{SettingPath}/{providerName}";
        public static string ToChildProvider<T>() => $"{SettingPath}/{typeof(T).Name}";
        
        protected override void OnSettingsChanged()
        {
            if (Application.isPlaying)
            {
                FDebug.DebugLevel = EasyFrameworkSettings.Instance.debugLevel;
            }
        }
    }
}
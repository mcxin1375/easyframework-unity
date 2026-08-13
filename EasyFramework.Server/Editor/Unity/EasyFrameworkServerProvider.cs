/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using EasyFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Server.Editor
{
    public class EasyFrameworkServerProvider : ProjectSettingsProvider<EasyFrameworkServerProvider>
    {
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public const string SettingPath = "Project/EasyFramework Server";
        public EasyFrameworkServerProvider() : base(SettingPath) { }
        public static string ToChildProvider(string providerName) => $"{SettingPath}/{providerName}";
        public static string ToChildProvider<T>() => $"{SettingPath}/{typeof(T).Name}";
        
        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                EasyFrameworkServerSettings.Instance
            };
        }
    }
}
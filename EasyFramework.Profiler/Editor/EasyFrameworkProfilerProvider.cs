/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using EasyFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Profiler.Editor
{
    public class EasyFrameworkProfilerProvider : ProjectSettingsProvider<EasyFrameworkProfilerProvider>
    {
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public const string SettingPath = "Project/EasyFramework Profiler";
        public EasyFrameworkProfilerProvider() : base(SettingPath) { }
        public static string ToChildProvider(string providerName) => $"{SettingPath}/{providerName}";
        public static string ToChildProvider<T>() => $"{SettingPath}/{typeof(T).Name}";

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                EasyFrameworkProfilerSettings.Instance
            };
        }
    }
}
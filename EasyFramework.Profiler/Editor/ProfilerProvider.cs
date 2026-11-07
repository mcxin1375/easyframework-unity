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
    public class ProfilerProvider : ProjectSettingsProvider<ProfilerSettings>
    {
        public const string SettingPath = "Project/EasyFramework Profiler";
        
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<ProfilerProvider>.Instance;

        public ProfilerProvider() : base(SettingPath) { }
    }
}
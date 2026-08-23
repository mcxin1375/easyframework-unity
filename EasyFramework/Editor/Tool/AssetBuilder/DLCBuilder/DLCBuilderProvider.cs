/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class DLCBuilderProvider : ProjectSettingsProvider<DLCBuilderSettings>
    {
        private const string SettingsPath = ToolsProvider.SettingPath + "/" + nameof(DLCBuilder);
        
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<DLCBuilderProvider>.Instance;
        public DLCBuilderProvider() : base(SettingsPath) { }

    }
}
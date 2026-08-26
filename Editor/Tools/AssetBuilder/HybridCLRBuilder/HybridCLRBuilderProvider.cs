/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

#if EF_HYBRIDCLR

using UnityEditor;

namespace EasyFramework.Editor
{
    public class HybridCLRBuilderProvider : ProjectSettingsProvider<HybridCLRBuilderSettings>
    {
        private const string SettingsPath = ToolsProvider.SettingPath + "/" + nameof(HybridCLRBuilder);
        
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<HybridCLRBuilderProvider>.Instance;
        
        public HybridCLRBuilderProvider() : base(SettingsPath) { }
    }
}

#endif
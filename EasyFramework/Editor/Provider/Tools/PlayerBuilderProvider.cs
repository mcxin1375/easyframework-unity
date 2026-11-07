/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class PlayerBuilderProvider : ProjectSettingsProvider<PlayerBuilderSettings>
    {
        private const string SettingsPath = ToolsProvider.SettingPath + "/" + nameof(PlayerBuilder);
        
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<PlayerBuilderProvider>.Instance;
        
        public PlayerBuilderProvider() : base(SettingsPath) { }

        protected override void OnDrawSettingsAfter(string searchContext)
        {
            ToolDrawHelper.DrawExtensions(PlayerBuilder.Instance.ToolExtensions);
            ToolDrawHelper.DrawExtensions(PlayerBuilder.Instance.ToolSettings);
        }
    }
}
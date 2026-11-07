/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class ToolsProvider : ProjectSettingsProvider
    {
        public const string SettingPath = "Project/EasyFramework Tools";
        
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<ToolsProvider>.Instance;

        public ToolsProvider() : base(SettingPath)
        {
        }

        protected override void OnRefresh()
        {
            foreach (var tool in ToolRegistry.Tools) tool.Refresh();
        }

        protected override void OnDrawSettings(string searchContext)
        {
            base.OnDrawSettings(searchContext);
            
            ToolDrawHelper.DrawTools(ToolRegistry.Tools);
        }
    }
}
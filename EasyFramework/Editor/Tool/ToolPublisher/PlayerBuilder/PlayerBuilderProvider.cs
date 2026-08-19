/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class PlayerBuilderProvider : ProjectSettingsProvider<PlayerBuilderProvider>
    {
        private const string SettingsPath = ToolProvider.SettingPath + "/" + nameof(PlayerBuilder);
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public PlayerBuilderProvider() : base(SettingsPath) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                PlayerBuilderSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            ToolDrawHelper.DrawToolEvents(PlayerBuilder.Instance.ToolEvents);
            ToolDrawHelper.DrawExtensions(PlayerBuilder.Instance.ToolExtensions);
            ToolDrawHelper.DrawExtensions(PlayerBuilder.Instance.ToolSettings);
        }
    }
}
/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class DLCBuilderProvider : ProjectSettingsProvider<DLCBuilderProvider>
    {
        private const string SettingsPath = ToolProvider.SettingPath + "/" + nameof(DLCBuilder);
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public DLCBuilderProvider() : base(SettingsPath) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                DLCBuilderSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);

            ToolDrawHelper.DrawToolEvents(DLCBuilder.Instance.ToolEvents);
        }
    }
}
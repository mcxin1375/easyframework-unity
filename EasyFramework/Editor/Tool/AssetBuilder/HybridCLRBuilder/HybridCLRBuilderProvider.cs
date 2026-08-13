/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

#if EF_HYBRIDCLR

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class HybridCLRBuilderProvider : ProjectSettingsProvider<HybridCLRBuilderProvider>
    {
        private const string SettingsPath = ToolProvider.SettingPath + "/" + nameof(HybridCLRBuilder);
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public HybridCLRBuilderProvider() : base(SettingsPath) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                HybridCLRBuilderSettings.CreateInstance(),
            };
        }
        
        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            ToolDrawHelper.DrawExtensions(HybridCLRBuilder.Instance.Extensions);
        }
    }
}

#endif
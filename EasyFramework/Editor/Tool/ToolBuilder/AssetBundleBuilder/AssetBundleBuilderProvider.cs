/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class AssetBundleBuilderProvider : ProjectSettingsProvider<AssetBundleBuilderProvider>
    {
        private const string SettingsPath = ToolProvider.SettingPath + "/" + nameof(AssetBundleBuilder);
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public AssetBundleBuilderProvider() : base(SettingsPath) { }

        protected override ScriptableObject[] LoadObjects()
        {
            AssetBundleBuilder.Instance.RefreshExtensions();
            return new ScriptableObject[]
            {
                AssetBundleBuilderSettings.CreateInstance(),
            };
        }
        
        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            ToolDrawHelper.DrawToolEvents(AssetBundleBuilder.Instance.ToolEvents);
            
            EditorGUILayout.HelpBox($"Type: {nameof(IAssetBundleBuilderSettings)}", MessageType.Info);
            foreach (var ex in AssetBundleBuilder.Instance.ToolSettings)
            {
                if (ex is ScriptableObject o)
                {
                    EditorGUILayout.ObjectField($"Order: {ex.Order}", o, o.GetType(), false);
                }
                else
                {
                    EditorGUILayout.LabelField($"Order: {ex.Order}", ex.GetType().Name);
                }
                
                if (ex.BuildDirectories?.Length > 0)
                {
                    foreach (var directory in ex.BuildDirectories)
                    {
                        EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}{directory}");
                    }
                }
            }
        }
    }
}
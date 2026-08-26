/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class AssetBundleBuilderProvider : ProjectSettingsProvider<AssetBundleBuilderSettings>
    {
        private const string SettingsPath = ToolsProvider.SettingPath + "/" + nameof(AssetBundleBuilder);
        
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<AssetBundleBuilderProvider>.Instance;
        
        public AssetBundleBuilderProvider() : base(SettingsPath) { }

        protected override void OnDrawSettingsAfter(string searchContext)
        {
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
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
        
        private string[] _buildDirectories;
        private IAssetBundleBuildSettings[] _buildSettings;
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public AssetBundleBuilderProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(AssetBundleBuilder))) { }

        protected override ScriptableObject[] LoadObjects()
        {
            _buildDirectories = AssetBundleBuilderSettings.Instance.BuildDirectories;
            _buildSettings = AssetBundleBuilderSettings.Instance.Extensions;
            return new ScriptableObject[]
            {
                AssetBundleBuilderSettings.CreateInstance(),
            };
        }
        
        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);

            EditorGUILayout.HelpBox("Build Directories", MessageType.Info);
            
            if (_buildDirectories?.Length > 0)
            {
                foreach (var directory in _buildDirectories)
                {
                    EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}{directory}", GUIStyles.MainStyle);
                    EditorGUILayout.Space(5);
                }
            }
            
            EditorGUILayout.HelpBox($"IEasyFrameworkTool<{nameof(AssetBundleBuilder)}>", MessageType.Info);
            
            foreach (var ex in _buildSettings)
            {
                if (ex is ScriptableObject o)
                {
                    EditorGUILayout.ObjectField($"-", o, o.GetType(), false);
                }
                else
                {
                    EditorGUILayout.LabelField($"-", ex.GetType().Name);
                }
            }
        }
    }
}
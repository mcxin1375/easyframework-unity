/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class AssetCreatorProvider : ProjectSettingsProvider<AssetCreatorProvider>
    {
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public AssetCreatorProvider() : base(AssetCreator.ProviderPath)
        {
        }

        protected override ScriptableObject[] LoadObjects()
        {
            AssetCreator.Instance.RefreshExtensions();
            
            return new ScriptableObject[]
            {
                AssetCreatorSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);

            EditorGUILayoutHelper.DrawExtensions(AssetCreator.Instance.Extensions);
        }
    }
}
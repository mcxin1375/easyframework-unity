/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class AssetImporterProvider : ProjectSettingsProvider<AssetImporterProvider>
    {

        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public AssetImporterProvider() : base(AssetImporter.ProviderPath) { }

        protected override ScriptableObject[] LoadObjects()
        {
            AssetImporter.Instance.RefreshExtensions();
            
            return new ScriptableObject[]
            {
                AssetImporterSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);

            EditorGUILayoutHelper.DrawExtensions(AssetImporter.Instance.Extensions);
        }
    }
}
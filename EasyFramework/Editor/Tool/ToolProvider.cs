/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class ToolProvider : ProjectSettingsProvider<ToolProvider>
    {
        public const string SettingPath = "Project/EasyFramework Tools";
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public ToolProvider() : base(SettingPath)
        {
        }

        protected override ScriptableObject[] LoadObjects()
        {
            AssetImporter.Instance.RefreshExtensions();
            AssetCreator.Instance.RefreshExtensions();
            AssetBuilder.Instance.RefreshExtensions();
            return null;
        }

        protected override void OnAfterDraw()
        {
            ToolDrawHelper.DrawExtensions(AssetImporter.Instance.Extensions);
            ToolDrawHelper.DrawExtensions(AssetCreator.Instance.Extensions);
            ToolDrawHelper.DrawExtensions(AssetBuilder.Instance.Extensions);
        }
    }
}
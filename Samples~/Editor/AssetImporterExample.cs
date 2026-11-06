using EasyFramework.Editor;
using UnityEditor;
using UnityEngine;
using FrameworkAssetImporter = EasyFramework.Editor.AssetImporter;

namespace EasyFramework.Samples.Modules.Editor
{
    public static class AssetImporterExample
    {
        // 显式创建禁用的配置；核对 from/to 后在 Inspector 启用，再执行工具。
        public static ResImporterSettings CreateSettings(string assetPath, string from, string to)
        {
            var settings = ScriptableObject.CreateInstance<ResImporterSettings>();
            settings.enabled = false;
            settings.order = 100;
            settings.resImporterConfigs = new[]
            {
                new ResImporterSettings.ResImporterConfig { from = from, to = to, deleteDiff = false }
            };
            AssetDatabase.CreateAsset(settings, assetPath);
            return settings;
        }

        // Create > EasyFramework > AssetImporter > ResImporterSettings 配置 from/to/deleteDiff；执行会同步并可能删除目标差异。
        [MenuItem("EasyFramework/Samples/Templates/Run Configured Asset Importer")]
        public static void Execute() => FrameworkAssetImporter.Instance.Execute();

        [MenuItem("EasyFramework/Samples/Templates/Run Configured Asset Importer", true)]
        private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}

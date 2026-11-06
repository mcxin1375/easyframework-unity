using System;
using EasyFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Samples.Modules.Editor
{
    // 创建资产并配置目录后才参与构建；.ab 目录整体打包，其他文件单独打包。
    [CreateAssetMenu(menuName = "EasyFramework/Samples/Bundle Settings")]
    public sealed class AssetBundleBuilderExample : ScriptableObject, IAssetBundleBuilderSettings
    {
        public bool enabled = true;
        public string[] buildDirectories = Array.Empty<string>();
        public AssetBundleBuildInfo[] buildInfos = Array.Empty<AssetBundleBuildInfo>();
        public int Order => 100;
        public string[] BuildDirectories => enabled ? buildDirectories : null;
        public AssetBundleBuildInfo[] BuildInfos => enabled ? buildInfos : null;

        // 先配置 Project Settings 和 IAssetBundleBuilderSettings；会替换当前平台 Bundle 输出。
        [MenuItem("EasyFramework/Samples/Templates/Build AssetBundles")]
        public static void Execute() => AssetBundleBuilder.Instance.Execute();

        // Dry Run 只用于编辑器查询清单，不发布可运行的 Bundle。
        [MenuItem("EasyFramework/Samples/Templates/Build Bundle Manifest")]
        public static void BuildManifestOnly() => AssetBundleBuilderUtility.BuildManifestOnly();

        [MenuItem("EasyFramework/Samples/Templates/Check Bundle Duplicates")]
        public static void CheckVerifyValidity() => AssetBundleBuilderUtility.CheckVerifyValidity();

        [MenuItem("EasyFramework/Samples/Templates/Build AssetBundles", true)]
        [MenuItem("EasyFramework/Samples/Templates/Build Bundle Manifest", true)]
        [MenuItem("EasyFramework/Samples/Templates/Check Bundle Duplicates", true)]
        private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}

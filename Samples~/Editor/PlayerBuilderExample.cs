using System;
using EasyFramework.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EasyFramework.Samples.Modules.Editor
{
    // 创建配置资产才参与扩展；Order=-100 使此配置优先于默认 IPlayerBuilderSettings。
    [CreateAssetMenu(menuName = "EasyFramework/Samples/Player Settings")]
    public sealed class PlayerBuilderExample : ScriptableObject, IPlayerBuilderSettings, IPlayerBuilderExtension
    {
        public string outputPath;
        public string[] scenes = Array.Empty<string>();
        public BuildOptions extraOptions;
        public int Order => -100;

        public BuildPlayerOptions BuildPlayerOptions
        {
            get
            {
                var options = new BuildPlayerOptionsSettings().BuildPlayerOptions;
                if (!string.IsNullOrEmpty(outputPath)) options.locationPathName = outputPath;
                if (scenes != null && scenes.Length > 0) options.scenes = scenes;
                options.options |= extraOptions;
                return options;
            }
        }

        public void OnBuildReport(BuildReport report)
            => Debug.Log($"Player 构建结果：{report.summary.result}，大小：{report.summary.totalSize}");

        // 先配置 AppSettings、Build Target、启动场景和 DLC 复制；默认先构建资源。
        // 分开执行资源与 Player 时先关闭 PlayerBuilder.assetBuilder，避免重复构建。
        [MenuItem("EasyFramework/Samples/Templates/Build Player")]
        public static void Execute() => PlayerBuilder.Instance.Execute();

        [MenuItem("EasyFramework/Samples/Templates/Build Player", true)]
        private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}

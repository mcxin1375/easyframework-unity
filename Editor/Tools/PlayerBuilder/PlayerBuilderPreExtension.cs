/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.IO;
using UnityEditor;
using UnityEditor.Build;

namespace EasyFramework.Editor
{
    public class PlayerBuilderPreExtension : IToolEvent<PlayerBuilder>
    {
        int IToolExtension.Order => PlayerBuilder.Instance.Order - 100;
        public void OnExecuteBefore()
        {
            switch (EasyFrameworkSettings.Instance.resLoaderMode)
            {
                case EResLoaderMode.DLC_StreamingAssets:
                    BuildDLC_StreamingAssets(PlayerBuilderSettings.Instance.releaseVersion);
                    break;
            }
        }

        void IToolEvent<PlayerBuilder>.OnExecute()
        {
            var settings = PlayerBuilderSettings.Instance;
            if (!settings.preSettingsEnabled) return;

            var appSettings = EasyFrameworkSettings.AppSettings;
            if (appSettings == null) return;
            
            PlayerSettings.companyName = appSettings.CompanyName;
            PlayerSettings.productName = appSettings.ProductName;
            PlayerSettings.bundleVersion = appSettings.BundleVersion;
            if (!appSettings.BundleIdentifier.IsNullOrEmpty())
            {
                var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                PlayerSettings.SetApplicationIdentifier(namedTarget, appSettings.BundleIdentifier);
            }
            
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    var bundleVersionCode = appSettings.BuildIndex > 0
                        ? appSettings.BuildIndex
                        : PlayerBuilder.Instance.Version.buildIndex;
                    if (bundleVersionCode < 1)
                    {
                        FDebug.LogError($"bundleVersionCode {bundleVersionCode} is less than 1.");
                        bundleVersionCode = 1;
                    }

                    PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = settings.exportAsGoogleAndroidProject;
                    break;
            }
        }
        
        public static void BuildDLC_StreamingAssets(string dlcVersion)
        {
            FileHelper.ClearDirectory(EasyFrameworkSettings.Instance.StreamingAssetsDLCPath);

            dlcVersion = dlcVersion.IsNullOrWhiteSpace()
                ? DLCBuilder.Instance.LatestVersion?.versionName
                : dlcVersion;

            var versionPath = $"{DLCBuilder.Instance.ProjectPlatformPath}/{dlcVersion}";
            if (!Directory.Exists(versionPath))
            {
                FDebug.LogError($"DLC list path {versionPath} does not exist.");
                return;
            }
            
            var files = Directory.GetFiles(versionPath,  "*.*", SearchOption.AllDirectories);
            FileHelper.CopyFiles(files, EasyFrameworkSettings.Instance.StreamingAssetsDLCPath);
        }
        
        [MenuItem("EasyFramework/Tools/PlayerBuilder - Build DLC_StreamingAssets", priority = ToolOrder.PlayerBuilder + 1)]
        public static void BuildDLC_StreamingAssets()
        {
            BuildDLC_StreamingAssets(PlayerBuilderSettings.Instance.releaseVersion);
            AssetDatabase.Refresh();
        }
    }
}

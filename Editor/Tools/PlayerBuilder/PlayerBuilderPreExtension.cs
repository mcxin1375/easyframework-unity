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
    }
}

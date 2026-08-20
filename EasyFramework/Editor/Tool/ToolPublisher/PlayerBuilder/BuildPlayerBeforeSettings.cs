/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEditor.Build;

namespace EasyFramework.Editor
{
    public class BuildPlayerBeforeSettings : IToolEvent<PlayerBuilder>
    {
        public void OnExecuteBefore()
        {
            PlayerSettings.companyName = EasyFrameworkSettings.App.CompanyName;
            PlayerSettings.productName = EasyFrameworkSettings.App.ProductName;
            PlayerSettings.bundleVersion = EasyFrameworkSettings.App.BundleVersion;
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            PlayerSettings.SetApplicationIdentifier(namedTarget, EasyFrameworkSettings.App.BundleIdentifier);
        }
    }
}
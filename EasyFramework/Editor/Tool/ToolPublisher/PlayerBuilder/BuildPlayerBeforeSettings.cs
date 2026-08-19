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
            PlayerSettings.companyName = EasyFrameworkAOTSettings.App.CompanyName;
            PlayerSettings.productName = EasyFrameworkAOTSettings.App.ProductName;
            PlayerSettings.bundleVersion = EasyFrameworkAOTSettings.App.BundleVersion;
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            PlayerSettings.SetApplicationIdentifier(namedTarget, EasyFrameworkAOTSettings.App.BundleIdentifier);
        }
    }
}
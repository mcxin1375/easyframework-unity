/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class DLCBuilder : ToolBase<DLCBuilder>, IToolEvent<AssetBuilder>
    {
        public override int Order => ToolOrder.DLCBuilder;

        public DLCBuilderVersion DLCBuilderVersion => DLCBuilderUtility.GetNewestBuilderVersion();
        public DLCVersion LatestVersion => DLCBuilderUtility.GetLatestVersion();
        
        protected override void OnSelfExecute() => DLCBuilderUtility.BuildBySettings(ProjectPlatformPath);
        protected override void OnSelfExecuteAfter()
        {
            if (!DLCBuilderSettings.Instance.copyVersionToStreamingAssets) return;
            
            DLCBuilderUtility.CopyVersionToStreamingAssets(DLCBuilderSettings.Instance.releaseVersion);
        }

        [MenuItem("EasyFramework/Tools/DLCBuilder - Execute", priority = ToolOrder.DLCBuilder)]
        public static void MenuItem1()
        {
            Instance.Execute();
        }
        
        [MenuItem("EasyFramework/Tools/DLCBuilder - CopyVersionToStreamingAssets", priority = ToolOrder.DLCBuilder + 1)]
        public static void CopyVersionToStreamingAssets()
        {
            DLCBuilderUtility.CopyVersionToStreamingAssets(DLCBuilderSettings.Instance.releaseVersion);
            AssetDatabase.Refresh();
        }
    }
}
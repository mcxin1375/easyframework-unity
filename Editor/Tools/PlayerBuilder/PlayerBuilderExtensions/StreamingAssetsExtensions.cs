/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System.IO;
using UnityEditor;

namespace EasyFramework.Editor
{
    public class StreamingAssetsExtensions : IToolEvent<PlayerBuilder>
    {
        public void OnExecuteBefore()
        {
            switch (PlayerBuilderSettings.Instance.streamingAssetsOptions)
            {
                case EStreamingAssetsOptions.DLCList:
                    BuildDLCList(PlayerBuilderSettings.Instance.dlcVersion);
                    break;
            }
            
        }

        [MenuItem("EasyFramework/Tools/PlayerBuilder - BuildDLCList", priority = ToolOrder.PlayerBuilder + 1)]
        public static void BuildDLCList()
        {
            BuildDLCList(PlayerBuilderSettings.Instance.dlcVersion);
            AssetDatabase.Refresh();
        }
        public static void BuildDLCList(string dlcVersion)
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
    }
}
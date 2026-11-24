/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    class AssetBundleBuildSettingExtension : IAssetBundleBuilderSettings
    {
        public string[] BuildDirectories => AssetBundleBuilderSettings.Instance.buildDirectories;
    }
    
    [ProjectSettings(ProjectSettingsAttribute.ETag.Editor)]
    public class AssetBundleBuilderSettings : ProjectSettings<AssetBundleBuilderSettings>
    {
        public bool createManifestFile;
        public BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle | 
                                                                 BuildAssetBundleOptions.IgnoreTypeTreeChanges |
                                                                 BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                                                 BuildAssetBundleOptions.AssetBundleStripUnityVersion |
                                                                 BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
        
        [Header("1. 普通文件按文件名生成小写 AB，同名合包（跳过 Editor 目录）\n2. .ab 目录整体打包")]
        public string[] buildDirectories = new string[]
        {
            "Assets/Res_DLC"
        };
        public string[] ignoreFileNames = new string[] { "LightingData" };
        public string[] ignoreFileExes = new string[]
        {
            ".tpsheet", 
            ".exr", 
            ".lighting", 
            ".terrainlayer",
        };
    }
}

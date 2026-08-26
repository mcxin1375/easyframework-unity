/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    class AssetBundleBuildSettingExtension : IAssetBundleBuilderSettings
    {
        public string[] BuildDirectories { get; }

        public AssetBundleBuildSettingExtension()
        {
            List<string> tmpList = new();
            
            if (AssetBundleBuilderSettings.Instance.buildDirectories?.Length > 0)
            {
                tmpList.AddRange(AssetBundleBuilderSettings.Instance.buildDirectories);
            }

            BuildDirectories = tmpList.ToArray();
        }
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
        
        [Header("1. 遍历所有目录，每个文件打成一个AB包（忽略Editor目录）\n2. 后缀为(.ab)的目录，整个目录打成AB包)")]
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
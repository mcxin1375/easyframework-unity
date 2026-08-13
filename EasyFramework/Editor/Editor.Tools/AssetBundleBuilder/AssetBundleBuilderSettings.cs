/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public interface IAssetBundleBuildSettings
    {
        string[] BuildDirectories { get; }
        AssetBundleBuildInfo[] BuildInfos => null;
    }
    
    [Serializable]
    public class AssetBundleBuildInfo
    {
        public string abName;
        public EAssetBundleBuildResType abResType;
        public string[] directories;
    }
    
    public enum EAssetBundleBuildResType
    {
        All = 0,
        Shader = 1,
    }
    
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class AssetBundleBuilderSettings : ProjectSettingsEditor<AssetBundleBuilderSettings>
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

        public IAssetBundleBuildSettings[] Extensions { get; private set; }

        public string[] BuildDirectories
        {
            get
            {
                List<string> buildList = new();

                if (buildDirectories?.Length > 0)
                {
                    buildList.AddRange(buildDirectories);
                }
                if (Extensions?.Length > 0)
                {
                    foreach (var obj in Extensions)
                    {
                        if (obj.BuildDirectories?.Length > 0)
                        {
                            buildList.AddRange(obj.BuildDirectories);
                        }
                    }
                }
                return buildList.ToArray();
            }
        }

        protected override void OnCreate()
        {
            Extensions = EasyFrameworkReflection.CreateInstances<IAssetBundleBuildSettings>().ToArray();
        }
    }
}
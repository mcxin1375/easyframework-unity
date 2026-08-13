/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework.Editor
{
    [CreateAssetMenu(menuName = "EasyFramework/AssetBundleBuildSettings")]
    public class AssetBundleBuildScriptableObject : ScriptableObject
    {
        public bool enabled = true;
        
        [Header("1. 遍历所有目录，每个文件打成一个AB包（忽略Editor目录）\n2. 后缀为(.ab)的目录，整个目录打成AB包)")]
        public string[] buildDirectories;
        [Header("自定义AB创建")]
        public AssetBundleBuildInfo[] buildInfos;
    }

    public class AssetBundleBuildScriptableObjectSettings : IAssetBundleBuildSettings
    {
        public string[] BuildDirectories { get; }
        public AssetBundleBuildInfo[] BuildInfos { get; }

        public AssetBundleBuildScriptableObjectSettings()
        {
            List<string> buildDirectories = new List<string>();
            List<AssetBundleBuildInfo> buildInfoList = new List<AssetBundleBuildInfo>();

            var arr = UnityEditorHelper.FindAssets<AssetBundleBuildScriptableObject>();
            foreach (var item in arr)
            {
                if (!item.enabled) continue;
                if (item.buildDirectories?.Length > 0)
                {
                    buildDirectories.AddRange(item.buildDirectories);
                }
                if (item.buildInfos?.Length > 0)
                {
                    buildInfoList.AddRange(item.buildInfos);
                }
            }
            
            BuildDirectories = buildDirectories.ToArray();
            BuildInfos = buildInfoList.ToArray();
        }
    }
}
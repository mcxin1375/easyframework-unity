/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Editor
{
    [CreateAssetMenu(menuName = "EasyFramework/AssetBuilder/AssetBundleBuildSettings")]
    public class AssetBundleBuildSettings : ScriptableObject, IAssetBundleBuilderSettings
    {
        public string[] BuildDirectories => enabled ? buildDirectories : null;
        public AssetBundleBuildInfo[] BuildInfos => enabled ? buildInfos : null;
        
        public bool enabled = true;
        
        [Header("1. 遍历所有目录，每个文件打成一个AB包（忽略Editor目录）\n2. 后缀为(.ab)的目录，整个目录打成AB包)")]
        public string[] buildDirectories;
        [Header("自定义AB创建")]
        public AssetBundleBuildInfo[] buildInfos;
    }
}
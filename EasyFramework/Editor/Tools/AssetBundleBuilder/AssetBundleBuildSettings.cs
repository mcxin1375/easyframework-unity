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
        
        [Header("1. 普通文件按文件名生成小写 AB，同名合包（跳过 Editor 目录）\n2. .ab 目录整体打包")]
        public string[] buildDirectories;
        [Header("自定义AB创建")]
        public AssetBundleBuildInfo[] buildInfos;
    }
}

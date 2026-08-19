/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEditor;

namespace EasyFramework.Editor
{
    public enum EAssetBundleBuildResType
    {
        All = 0,
        Shader = 1,
    }
    
    [Serializable]
    public class AssetBundleBuildInfo
    {
        public string abName;
        public EAssetBundleBuildResType abResType;
        public string[] directories;
    }

    public interface IAssetBundleBuilderSettings : IToolExtensionObject
    {
        string[] BuildDirectories { get; }
        AssetBundleBuildInfo[] BuildInfos => null;
    }
    
    public partial class AssetBundleBuilder : ToolBase<AssetBundleBuilder>
    {
        public IAssetBundleBuilderSettings[] ToolSettings => ToolExtension<IAssetBundleBuilderSettings>.Instances;
        
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - Execute", priority = ToolOrder.AssetBundleBuilder)]
        public static void MenuItem1() => Instance.Execute();
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - BuildManifestOnly", priority = ToolOrder.AssetBundleBuilder)]
        public static void MenuItem2() => Instance.BuildManifestOnly();
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - CheckVerifyValidity", priority = ToolOrder.AssetBundleBuilder)]
        public static void MenuItem3() => Instance.CheckVerifyValidity();
    }
}
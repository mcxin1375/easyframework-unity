/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public partial class AssetBundleBuilder : SingletonTool<AssetBundleBuilder>
    {
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - Execute", priority = ToolOrder.AssetBundleBuilder)]
        public static void MenuItem1() => Instance.Execute();
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - BuildManifestOnly", priority = ToolOrder.AssetBundleBuilder)]
        public static void MenuItem2() => Instance.BuildManifestOnly();
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - CheckVerifyValidity", priority = ToolOrder.AssetBundleBuilder)]
        public static void MenuItem3() => Instance.CheckVerifyValidity();

        class AssetBundleBuilderTrigger : IToolEvent<AssetBuilder>, IToolEvent<AssetBundleBuilder>
        {
            void IToolEvent<AssetBuilder>.OnExecute()
            {
                Instance.Execute();
            }

            void IToolEvent<AssetBundleBuilder>.OnExecute()
            {
                Instance.BuildBySettings();
            }
        }
    }
}
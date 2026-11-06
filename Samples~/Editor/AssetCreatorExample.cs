using EasyFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Samples.Modules.Editor
{
    // 创建并启用配置资产，再执行 AssetCreator；导入脚本不会自动生成资源。
    [CreateAssetMenu(menuName = "EasyFramework/Samples/Asset Creator")]
    public sealed class AssetCreatorExample : ToolScriptableObject<AssetCreator>
    {
        [MenuItem("EasyFramework/Samples/Templates/Run Configured Asset Creator")]
        public static void Execute() => AssetCreator.Instance.Execute();

        [MenuItem("EasyFramework/Samples/Templates/Run Configured Asset Creator", true)]
        private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode;

        protected override void OnExecute()
        {
            // 校验业务输入后生成配置、图集等派生资源；重复执行应保持结果稳定。
            // 必须执行的释放逻辑放在本方法的 try/finally 中。
        }
    }
}

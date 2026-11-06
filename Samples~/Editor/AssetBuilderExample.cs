using EasyFramework.Editor;
using UnityEditor;

namespace EasyFramework.Samples.Modules.Editor
{
    public static class AssetBuilderExample
    {
        // 确认平台与配置后执行 HybridCLRBuilder（启用时）→ AssetBundleBuilder → DLCBuilder。
        // Execute 自动发现 IToolEvent<AssetBuilder>，按 Order 升序执行并升级工具版本。
        [MenuItem("EasyFramework/Samples/Templates/Build Assets")]
        public static void Execute() => AssetBuilder.Instance.Execute();

        [MenuItem("EasyFramework/Samples/Templates/Build Assets", true)]
        private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}

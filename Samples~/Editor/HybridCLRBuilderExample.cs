using UnityEditor;

namespace EasyFramework.Samples.Modules.Editor
{
#if EF_HYBRIDCLR
    public static class HybridCLRBuilderExample
    {
        // 需安装 HybridCLR、选好 Build Target 并配置 Project Settings > EasyFramework Tools > HybridCLRBuilder。
        [MenuItem("EasyFramework/Samples/Templates/Build HybridCLR")]
        public static void Execute() => EasyFramework.Editor.HybridCLRBuilder.Instance.Execute();

        // 先编译当前 Build Target 的热更新程序集，再打包 Builder 输出。
        [MenuItem("EasyFramework/Samples/Templates/Compile And Build HybridCLR")]
        public static void CompileAndExecute()
        {
            HybridCLR.Editor.Commands.CompileDllCommand.CompileDllActiveBuildTarget();
            Execute();
        }

        // HybridCLR GenerateAll 会写项目生成文件；Builder 随后重建当前平台输出。
        [MenuItem("EasyFramework/Samples/Templates/Generate All And Build HybridCLR")]
        public static void GenerateAllAndExecute()
        {
            HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
            Execute();
        }

        [MenuItem("EasyFramework/Samples/Templates/Build HybridCLR", true)]
        [MenuItem("EasyFramework/Samples/Templates/Compile And Build HybridCLR", true)]
        [MenuItem("EasyFramework/Samples/Templates/Generate All And Build HybridCLR", true)]
        private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
#endif
}

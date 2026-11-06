using EasyFramework.Editor;
using UnityEditor;

namespace EasyFramework.Samples.Modules.Editor
{
    public static class DLCBuilderExample
    {
        // 先构建 AssetBundles，并配置 Project Settings > EasyFramework Tools > DLCBuilder；会提交版本化输出。
        [MenuItem("EasyFramework/Samples/Templates/Build DLC")]
        public static void Execute() => DLCBuilder.Instance.Execute();

        // 先确认 releaseVersion；会替换 StreamingAssets/DLC 底包。
        [MenuItem("EasyFramework/Samples/Templates/Copy DLC To StreamingAssets")]
        public static void CopyVersionToStreamingAssets() => DLCBuilder.CopyVersionToStreamingAssets();

        [MenuItem("EasyFramework/Samples/Templates/Build DLC", true)]
        [MenuItem("EasyFramework/Samples/Templates/Copy DLC To StreamingAssets", true)]
        private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}

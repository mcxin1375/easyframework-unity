using EasyFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Samples.Modules.Editor
{
    // 自定义工具：仅显式调用时执行，不注册到其他工具的构建链。
    public sealed class ToolBaseExample : ToolBase<ToolBaseExample>
    {
        public override int Order => 100;

        protected override void OnSelfExecuteBefore() => Debug.Log("Before：校验输入");
        protected override void OnSelfExecute() => Debug.Log($"Execute：输出目录 {ProjectPlatformPath}");
        protected override void OnSelfExecuteAfter() => Debug.Log("After：校验结果");

        // Execute 自动 Refresh、按 Order 升序分发三阶段，成功后保存版本并刷新 AssetDatabase。
        // After 是成功阶段；必须执行的资源释放放在业务自己的 try/finally。
        [MenuItem("EasyFramework/Samples/Templates/Run Tool Lifecycle")]
        public static void Run() => Instance.Execute();

        public static void RefreshExtensions()
        {
            Instance.Refresh();
            foreach (var extension in Instance.Extension)
                Debug.Log($"{extension.GetType().Name}：Order={extension.Order}");
        }

        [MenuItem("EasyFramework/Samples/Templates/Run Tool Lifecycle", true)]
        private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}

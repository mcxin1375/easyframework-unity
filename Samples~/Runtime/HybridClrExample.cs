using System;
using System.Reflection;

namespace EasyFramework.Samples.Modules
{
    // 先配置 HybridCLRBuilder、业务入口与 DLC；调用程序集需要 EF_HYBRIDCLR。
    // 当前 Player 流程尚未完整，需项目补齐；方法只发起流程，回调通知完成。
    public static class HybridClrExample
    {
#if EF_HYBRIDCLR
        public static HybridCLRManager.EState State => F.HybridCLRManager.State;

        public static async ETask EnterAsync()
        {
            await F.InitializeAsync();
#if UNITY_EDITOR
            F.HybridCLRManager.EnterEditor(OnCompleted);
#else
            F.HybridCLRManager.Enter(OnCompleted);
#endif
        }

        private static void OnCompleted(HybridCLRManager.EResult result)
            => FDebug.Log($"HybridCLR 结果：{result}");

        // 显式选择对应环境入口；管理器忙碌或已结束时拒绝再次进入。
        public static void EnterRuntime(Action<HybridCLRManager.EResult> completed)
            => F.HybridCLRManager.Enter(completed);

#if UNITY_EDITOR
        public static void EnterEditor(Action<HybridCLRManager.EResult> completed)
            => F.HybridCLRManager.EnterEditor(completed);
#endif

        // 仅在初始化及项目热更新集成已就绪后调用；Dll / MetaData 均使用此入口。
        // 当前 Editor 分支查找已加载程序集并执行入口，Player 分支仍未完成，可能返回 null。
        public static ETask<Assembly> LoadAsync(string assemblyName, HybridCLRManager.ELoadType loadType)
            => F.HybridCLRManager.LoadAsync(assemblyName, loadType);
#endif
    }
}

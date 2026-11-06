using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace EasyFramework.Samples.Modules
{
    // Unity 主线程、Play Mode 下调用；ETask 是单消费者，await 或 Forget 后不再读取旧任务。
    // 不会自动随组件销毁取消；等待后检查 this，注册的 Timer / Tick 在 OnDestroy 中移除。
    public sealed class TaskExample : MonoBehaviour, ITimerObject, ITickerNode
    {
        private Guid _oneShotTimer;
        private Guid _repeatTimer;
        private int _timerCalls;
        private int _nodeCalls;
        private int _funcCalls;
        private Action _actionTick;
        private Func<bool> _funcTick;
        public bool IsTimerAlive => this != null;

        // 完成任务、结果任务，以及秒、毫秒、TimeSpan、帧数等待。
        public async ETask RunAsync()
        {
            await F.InitializeAsync();
            if (this == null) return;
            await ETask.CompletedTask;
            int value = await ETask.FromResult(42);
            await ETask.DelaySeconds(0.5f);
            if (this == null) return;
            await ETask.DelayMilliSeconds(100);
            if (this == null) return;
            await ETask.Delay(TimeSpan.FromMilliseconds(100));
            if (this == null) return;
            await ETask.DelayFrame(2); // 帧数非负，按 ETask 调度 tick 计数。
            if (this == null) return;
            Debug.Log($"异步步骤完成：{value}", this);
        }

        // Delay / WaitUntil 没有 CancellationToken 重载，条件包含拥有者结束条件。
        public async ETask WaitForConditionAsync(Func<bool> ready)
        {
            await ETask.WaitUntil(() => this == null || ready());
            if (this == null) return;
            Debug.Log("等待条件满足", this);
        }

        // Status 仅在消费前读取；不要把同一个池化任务交给第二个 await / Forget。
        public async ETask ReadPendingStatusAsync()
        {
            var task = ETask.DelayFrame(1);
            Debug.Log($"消费前状态：{task.Status}");
            await task;
        }

        public async ETask CatchAsync(ETask operation)
        {
            try { await operation; }
            catch (OperationCanceledException) { Debug.Log("操作已取消"); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        // void 入口显式消费任务；不写丢弃结果的 _ = RunAsync()。
        public void RunWithoutWaiting() => RunAsync().Forget();
        public static void ForgetResult<T>(ETask<T> operation) => operation.Forget();

        // 全局未观察异常订阅；结束时配对移除。取消异常默认不进入此事件。
        public void ObserveForgottenErrors()
        {
            EVoidScheduler.UnobservedTaskException -= OnUnobservedError;
            EVoidScheduler.UnobservedTaskException += OnUnobservedError;
        }
        private void OnUnobservedError(Exception exception) => Debug.LogException(exception);

        // 这是全局异常策略；调用方保存返回旧值，业务结束后用同一方法恢复。
        public static bool SetReportCancellation(bool enabled)
        {
            bool previous = EVoidScheduler.PropagateOperationCanceledException;
            EVoidScheduler.PropagateOperationCanceledException = enabled;
            return previous;
        }

        // Action Timer 到时单次执行；Func<bool> 返回 true 按间隔继续，false 移除。
        public void StartTimers()
        {
            StopTimers();
            _timerCalls = 0;
            _oneShotTimer = ETask.AddTimer(1, () => Debug.Log("单次 Timer"), this);
            _repeatTimer = ETask.AddTimer(1, () => ++_timerCalls < 3, this);
        }

        public void StopTimers()
        {
            if (_oneShotTimer != Guid.Empty) ETask.RemoveTimer(_oneShotTimer);
            if (_repeatTimer != Guid.Empty) ETask.RemoveTimer(_repeatTimer);
            _oneShotTimer = _repeatTimer = Guid.Empty;
        }

        // 三种 Tick：对象和 Func 返回 true 继续，Action 的 loop 决定是否重复。
        public void StartTicks(bool loopAction = false)
        {
            StopTicks();
            _nodeCalls = _funcCalls = 0;
            _actionTick = () => Debug.Log("Action Tick");
            _funcTick = () => ++_funcCalls < 3;
            ETask.AddTick(this);
            ETask.AddTick(_actionTick, loopAction);
            ETask.AddTick(_funcTick);
        }
        public bool OnTick() => this != null && ++_nodeCalls < 3;

        // 移除的是注册，不负责取消或完成其他 ETask；委托必须使用原实例。
        public void StopTicks()
        {
            ETask.RemoveTick(this);
            if (_actionTick != null) ETask.RemoveTick(_actionTick);
            if (_funcTick != null) ETask.RemoveTick(_funcTick);
            _actionTick = null;
            _funcTick = null;
        }

        private void OnDestroy()
        {
            StopTimers();
            StopTicks();
            EVoidScheduler.UnobservedTaskException -= OnUnobservedError;
        }

        // ETask 调度线程及时间；Unity 2022.1+ 使用 PlayerLoop，Time 来自 Unity Time.time。
        public static void ShowScheduler()
            => Debug.Log($"ETask：{ETask.ThreadType}，线程 {ETask.ThreadId}，时间 {ETask.Time}");

        // 工作线程只访问普通数据；无论成功或异常，访问 Unity 前切回主线程。
        public static async ETask<int> CalculateOnThreadPoolAsync(int value, CancellationToken token)
        {
            int result = 0;
            try { await ETask.RunOnThreadPool((Action)(() => result = value * value), token); }
            finally { await ETask.SwitchToMainThread(); }
            Debug.Log($"后台计算结果：{result}");
            return result;
        }

        public static async ETask RunOnThreadPoolAsync(Func<ETask> operation, CancellationToken token)
        {
            try { await ETask.RunOnThreadPool(operation, token); }
            finally { await ETask.SwitchToMainThread(); }
        }

        public static async ETask SwitchThreadsAsync()
        {
            int threadId;
            try
            {
                await ETask.SwitchToThreadPool();
                threadId = Thread.CurrentThread.ManagedThreadId; // 此处不访问 Unity 对象。
            }
            finally { await ETask.SwitchToMainThread(); }
            Debug.Log($"此前的工作线程：{threadId}");
        }

        // 通用 AsyncOperation 当前没有专用 await 扩展，用真实公开的 WaitUntil 等待 isDone。
        public static async ETask WaitOperationAsync(AsyncOperation operation)
            => await ETask.WaitUntil(() => operation == null || operation.isDone);

        // IObjectTask 由业务保证最终 IsCompleted；两种入口分别创建和消费自己的任务。
        public static async ETask WaitObjectAsync(IObjectTask operation) => await ETask.WaitObject(operation);
        public static async ETask WaitObjectExtensionAsync(IObjectTask operation) => await operation.WaitTaskCompleted();

        // 原生请求和 Bundle 的生命周期由调用者管理；取消适配器不等于中止原生加载。
        public static async ETask<Object> LoadAssetAsync(AssetBundle bundle, string assetName, CancellationToken token)
            => await ETask.AssetBundleRequestAsync(bundle.LoadAssetAsync(assetName), token);
        public static async ETask<Object> AwaitAssetRequestAsync(AssetBundle bundle, string assetName)
            => await bundle.LoadAssetAsync(assetName);

        // URL 与下载路径由业务提供；返回请求会在适配器清理时 Dispose，await 后立即复制所需结果。
        public static async ETask<string> GetTextAsync(string url)
        {
            var request = await ETask.UnityWebRequest(url);
            return request != null && request.result == UnityWebRequest.Result.Success
                ? request.downloadHandler.text : null;
        }
        public static async ETask<bool> DownloadAsync(string url, string file, CancellationToken token, IHttpReceiver receiver = null)
        {
            var request = await ETask.UnityWebRequestDownload(url, file, receiver, token);
            return request != null && request.result == UnityWebRequest.Result.Success;
        }

#if UNITY_2022_3_OR_NEWER
        // 返回的实例由调用方 Destroy；取消仅停止等待，原生 operation 与迟到结果仍由调用方清理。
        public static async ETask<GameObject[]> WaitInstantiateAsync(AsyncInstantiateOperation<GameObject> operation, CancellationToken token)
            => await ETask.AsyncInstantiateOperationAsync(operation, token);
        public static async ETask<GameObject[]> InstantiateAsync(GameObject prefab, int count, Transform parent = null)
            => await Object.InstantiateAsync(prefab, count, parent);
#endif
    }
}

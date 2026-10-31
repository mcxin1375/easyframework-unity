# ETask

`ETask` / `ETask<T>` 是框架的可等待值类型；业务异步方法返回 ETask，由调用方 await。内部使用池化 awaiter 和状态机减少分配，实际 GC 仍取决于委托、闭包、日志和业务代码。

## 基础 API

| API | 说明 |
| --- | --- |
| `CompletedTask` / `FromResult<T>(value)` | 已完成任务 |
| `DelaySeconds(float)` / `DelayMilliSeconds(int)` / `Delay(TimeSpan)` | 时间等待 |
| `DelayFrame(int)` | 按调度 tick 等待，参数应非负 |
| `WaitUntil(Func<bool>)` | 每 tick 检查条件，满足后完成 |
| `Forget()` | 在 Unity void 入口发起任务；异常交给 EVoidScheduler |
| `AddTimer` / `RemoveTimer` | 注册和移除定时器 |
| `AddTick` / `RemoveTick` | 注册和移除 tick 回调 |

```csharp
using EasyFramework;
using UnityEngine;

public sealed class TaskExample : MonoBehaviour
{
    private void Start() => RunAsync().Forget();

    private async ETask RunAsync()
    {
        await F.InitializeAsync();
        await ETask.DelaySeconds(1);
        if (this == null) return;
        Debug.Log("One second later");
    }
}
```

Unity 2022.1+ 使用 UnityThread / PlayerLoop，其他编译分支使用独立线程实现。Delay 使用 ETask 时钟。延迟不会自动随 MonoBehaviour 销毁取消，await 后要检查业务对象是否仍有效。

## 生命周期与异常

对池化任务按一次消费使用，不要缓存已完成的 ETask 并反复 await，也不要假设支持 System.Threading.Tasks.Task 的全部并发、取消接口。当前 Delay / WaitUntil 没有 CancellationToken 重载；长时间条件等待要自行保证结束条件。

EVoid 用于无需等待结果的异步入口；通常业务返回 ETask 并 await，仅在 Unity 的 void 入口调用 Forget()。未处理异常由 EVoidScheduler.UnobservedTaskException 接收，未订阅时写入 FDebug；需要业务恢复时应在异步方法内捕获。OperationCanceledException 默认不报告，可通过 PropagateOperationCanceledException 改变。

Unity 扩展包含 AsyncOperation、AssetBundleRequest、UnityWebRequest 与异步实例化适配；可用性取决于 Unity 编译条件，不能据此推断 package 最低版本。

源码：[ETask.cs](../../Runtime/Core/ETask/ETask.cs)、[ETask.Delay.cs](../../Runtime/Core/ETask/ETask.Delay.cs)、[ETask.WaitUntil.cs](../../Runtime/Core/ETask/ETask.WaitUntil.cs)、[EVoid.cs](../../Runtime/Core/ETask/EVoid.cs)。

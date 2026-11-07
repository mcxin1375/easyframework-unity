# ETask

`ETask` / `ETask<T>` 是框架的可等待值类型；业务异步方法返回 ETask，由调用方 await。内部使用池化 awaiter 和状态机减少分配，实际 GC 仍取决于委托、闭包、日志和业务代码。

## 基础 API

| API | 说明 |
| --- | --- |
| `CompletedTask` / `FromResult<T>(value)` | 已完成任务 |
| `WhenAll` / `WhenAny` | 等待多个 ETask；支持数组和 2 / 3 个固定参数 |
| `DelaySeconds` / `DelayMilliSeconds` / `Delay(TimeSpan)` | 时间等待，可选择 `ETaskDelayType` 时钟并传入 `CancellationToken` |
| `DelayFrame` / `Yield` / `NextFrame` | 按调度 tick 等待，可传 `CancellationToken`；帧数参数应非负 |
| `WaitUntil` / `WaitWhile` | 每 tick 检查条件，支持 state 参数和 `CancellationToken` |
| `RunOnThreadPool<T>` | 在线程池运行同步委托，或在线程池启动异步委托并返回结果 |
| `ETaskCompletionSource<T>` | 从回调或外部事件完成一个 ETask |
| `Preserve()` | 创建可多次等待与读取的 `SharedETask` |
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

Unity 2022.1+ 使用 UnityThread / PlayerLoop，其他编译分支使用独立线程实现。Delay 默认使用受 `Time.timeScale` 影响的 scaled 时钟；`ETaskDelayType.Unscaled` 使用 unscaled 时钟，`Realtime` 使用 realtime since startup 时钟。非 Unity 编译分支的三种时钟都映射到 ETask 线程的 Stopwatch 时钟。`Yield()` 等待下一个 ETask 调度 tick；`NextFrame()` 等待一个完整 tick 后的下一次 tick。延迟不会自动随 MonoBehaviour 销毁取消，await 后要检查业务对象是否仍有效。

## 生命周期与异常

池化任务只有一个消费者：调用一次 `await` 或 `Forget()`，不要让多个调用方等待同一个 ETask，也不要在消费后再次查询 Status、读取结果或重复 await。需要共享结果时，对任务调用 `Preserve()` 并把返回的 `SharedETask` / `SharedETask<T>` 分发给多个消费者；共享任务缓存结果或异常，允许重复 await 和 GetResult。`CompletedTask` 和 `FromResult` 不持有池化源，不受回池限制。

`ETaskCompletionSource` / `ETaskCompletionSource<T>` 为回调式 API 提供 `Task`、`TrySetResult`、`TrySetException` 和 `TrySetCanceled`。其 `Task` 仍遵循普通 ETask 的单消费者规则；多个消费者应共享 `completionSource.Task.Preserve()`。完成源对象本身不池化。`RunOnThreadPool<T>` 在线程池开始调用委托前和返回前检查 token；`Func<ETask<T>>` 委托在线程池启动，但后续 continuation 由它等待的 awaiter 决定。取消是协作式的，不能中断已经运行的同步委托。

`WhenAll` 会消费所有输入并等待全部完成；泛型结果按输入顺序返回。一个输入失败时重新抛出原异常，多个失败时抛出 `AggregateException`；没有普通异常但有取消时，重抛首个 `OperationCanceledException`。空数组立即完成。`WhenAny` 注册并消费所有输入，返回首个完成项的索引和（泛型时）结果；首个完成项的异常或取消会传播，后续输入的结果与异常会被消费并丢弃。它不适用于需要之后再次等待 loser 的场景。空输入抛出 `ArgumentException`。

可以先启动任务，完成若干 tick 后再第一次 await。完成会将任务从调度器摘除，但结果和原始异常保留到首次消费；首次消费及运行中的续体结束后，才在安全的调度时机清理并回池。延后读取 UnityWebRequest 等资源结果时，应在 await 后及时读取内容，不能把返回的原生请求对象当作长期资源持有。不要在旧一轮清理完成前重新启动同一个 awaiter 实例。

完成但未消费的任务不会为等待消费者而留在 Ticker 中。仍持有 ETask 时结果保持可读；所有引用都丢弃后由 GC 回收，该任务不会回到对象池。无需结果的调用也应使用 `Forget()` 完成消费和异常报告，而不是只写 `_ = RunAsync()`。`Delay`、`DelayFrame`、`Yield`、`NextFrame`、`WaitUntil` 和 `WaitWhile` 的取消在下一次 ETask tick 检查，不即时中断当前回调；await 时以 `Canceled` 状态抛出带原 token 的 `OperationCanceledException`。

`async ETask`、`async ETask<T>` 以及等待条件/任务 tick 中产生的异常会由 await 重新抛出，保留原异常；调用方可用 try/catch 恢复。OperationCanceledException 使任务状态为 Canceled，消费时仍抛出该异常。`RemoveTick` 和 `Ticker.Dispose` 仅停止注册，不通知任务、不取消或释放任务；拥有者需要显式完成或取消任务。移除后仍需回收的源必须重新安排一次调度。具体 Unity 适配器原有的 null/false 失败返回约定不因此自动改为取消异常。

EVoid 用于无需等待结果的异步入口；通常业务返回 ETask 并 await，仅在 Unity 的 void 入口调用 Forget()。Forget 消费任务后把未处理异常交给 EVoidScheduler.UnobservedTaskException，未订阅时写入 FDebug。OperationCanceledException 默认不报告，可通过 PropagateOperationCanceledException 改变。

`ITickerNode` 只提供 `bool OnTick()`，不再提供 `OnRemove`。Ticker 内部用双向链表和 `Dictionary<ITickerNode, Node>` 管理注册；目标按引用身份比较，不调用目标自定义的 Equals / GetHashCode。每次 Add（包括 Action / Func 重载）都使旧注册过期并建立新注册，不是幂等去重；同一目标只有最新注册继续更新。在 tick 中新增或重新注册的节点从下一次 tick 开始。Remove 使当前注册失效，已经选中或开始执行的回调可以完成一次；同一 Ticker 的 Tick 串行执行，递归或并发 Tick 不重复派发。

Ticker 只回收内部 Node，不清理目标对象。TaskAwaiter 的显式 OnTick 仅分派两个分支：IsCompleted 时结束本轮注册、重置基础数据并调用 OnTaskResult，随后返回 false；否则调用 OnTaskTick 推进任务。IsCompleted 表示本轮已消费且生产者/续体交接已经结束，与 awaiter 表示结果就绪的 IsCompleted 含义不同。完成但尚未首次消费的任务停止业务轮询并保留结果；首次 GetResult 重新安排一次 Tick，续体或生产者仍在交接时等待其退出，之后才进入 OnTaskResult。OnTaskResult 是最终完成和回池事件，归还对象池必须是最后一次访问实例的操作。归还后旧调用只返回 false，新租借注册不受影响。后台命令自行扩展 IsCompleted，等队列和执行者放弃本轮引用后才终结，不再增加 CanRecycle 事件。

Ticker 逐节点隔离 OnTick 异常，失败注册退出，后续节点继续；任务轮询抛错会令对应任务以 Faulted 完成。Timer.Tick 出错也不会跳过同次 ETask 调度中的 Ticker.Tick；这不表示 Timer 内部每个定时节点都已逐一隔离。Start 的初始化钩子在发布到 Ticker 前执行。完成、续体注册及结果消费仍通过任务自身的同步协议交接，Node 身份隔离不能替代任务线程同步。

等待续体使用 `IETaskAwaiter.OnCompleted(Action, Guid)`，不再传递单独的 state 参数；自定义实现需要同步更新签名。TaskAwaiter 基类仅保留 Token、Status、异常、续体、消费交接五个实例字段，没有 SyncRoot、生命周期枚举或 CanRecycle。Start 的独占初始化由子类 Create/Begin 保证，不支持同一实例并发或重复启动；初始化结束后才注册 Ticker。完成方以 CAS 取得本轮唯一完成权，先写结果/异常再发布终态，随后原子交接续体；运行中交接不会提前回池。GetResult 先复制结果和异常，再申请单次消费、发布新注册，最后发布消费结束。完成与消费的最终发布之后不再由旧调用访问实例。

同一池化任务仅允许一个消费者，OnCompleted 与 GetResult 不应由不同消费者并发使用；消费后禁止继续使用旧 ETask、旧 token 或旧生产者调用来操作复用后的实例。轮询子类在 OnTaskTick 开头以 `if (!IsPending) return base.OnTaskTick();` 保留完成结果，不重复执行业务更新；返回 false 仅表示停止注册，业务需要自行通过 TrySetResult / TrySetException 结束操作。

Unity 扩展包含 AsyncOperation、AssetBundleRequest、UnityWebRequest 与异步实例化适配；可用性取决于 Unity 编译条件，不能据此推断 package 最低版本。

源码：[ETask.cs](../../EasyFramework/Runtime/ETask/ETask.cs)、[ETask.Delay.cs](../../EasyFramework/Runtime/ETask/ETask.Delay.cs)、[ETask.WaitUntil.cs](../../EasyFramework/Runtime/ETask/ETask.WaitUntil.cs)、[EVoid.cs](../../EasyFramework/Runtime/ETask/EVoid.cs)。

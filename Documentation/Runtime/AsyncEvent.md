# F.Event 异步事件

`F.Event` 通过 `AddAsync`、`RemoveAsync` 和 `InvokeAsync` 管理异步事件。对象处理器实现 `IAsyncEvent<T>`，委托处理器使用 `Func<T, ETask>`。调用方等待 `InvokeAsync` 返回的 ETask，直到本次快照内所有处理器依次结束。

| API | 说明 |
| --- | --- |
| `AddAsync<T>(IAsyncEvent<T>)` | 注册异步对象处理器；空对象和重复注册被忽略 |
| `AddAsync<T>(Func<T, ETask>)` | 注册异步委托处理器；空委托和重复注册被忽略 |
| `RemoveAsync<T>(IAsyncEvent<T>)` | 移除异步对象处理器；重复移除被忽略 |
| `RemoveAsync<T>(Func<T, ETask>)` | 移除异步委托处理器；重复移除被忽略 |
| `InvokeAsync<T>(T args)` | 捕获本次订阅快照，按顺序逐个 await 处理器 |

对象处理器实现 `ETask IAsyncEvent<T>.ExecuteAsync(T args)`。同步与异步事件共用 `F.Event` 入口，分别使用 `Add/Remove/Invoke` 与 `AddAsync/RemoveAsync/InvokeAsync` 方法；两组订阅完全独立，即使参数类型相同，也只派发对应方法组的处理器。同步行为见 [F.Event](Event.md)。

## 注册与等待

```csharp
using System;
using EasyFramework;

readonly struct LoginCompleted { }

sealed class LoginAudit : IAsyncEvent<LoginCompleted>
{
    public async ETask ExecuteAsync(LoginCompleted args)
    {
        await ETask.DelayFrame(1);
        FDebug.Log("Login audit finished");
    }
}

static class LoginFlow
{
    public static async ETask RunAsync()
    {
        var audit = new LoginAudit();
        Func<LoginCompleted, ETask> save = args => SaveAsync(args);
        F.Event.AddAsync<LoginCompleted>(audit);
        F.Event.AddAsync(save);
        try
        {
            await F.Event.InvokeAsync(new LoginCompleted());
            FDebug.Log("All login handlers finished");
        }
        finally
        {
            F.Event.RemoveAsync<LoginCompleted>(audit);
            F.Event.RemoveAsync(save);
        }
    }

    private static async ETask SaveAsync(LoginCompleted args)
    {
        await ETask.DelayFrame(1);
        FDebug.Log("Login save finished");
    }
}
```

lambda 注册时保存委托，解绑时传入保存的同一委托；重新写一个 lambda 不能代替原委托。业务对象结束订阅生命周期时应主动解绑，订阅不会随对象销毁自动解除。

每个 `InvokeAsync` 返回的池化 ETask 只消费一次：由调用方 `await`，或在无需等待的 void 入口调用 `F.Event.InvokeAsync(args).Forget()`。不要丢弃返回值、重复 await，或让多个消费者共享同一个池化 ETask；详见 [ETask 生命周期](ETask.md#生命周期与异常)。每个处理器每次调用也应返回本次独立的 ETask，不要缓存同一个池化任务供多个派发复用。同步完成的处理器可以返回 `ETask.CompletedTask`。

## 派发与订阅变更

每次 `InvokeAsync` 在第一个处理器执行前捕获对象和委托两组订阅。先按注册顺序逐个等待对象处理器，再逐个等待委托处理器；前一个处理器结束后才开始下一个。

派发期间新增或移除订阅不改变该次快照：已移除的处理器仍执行本次调用，新增处理器从下一次 `InvokeAsync` 开始接收。移除后重新注册会排在所属组的末尾。嵌套派发捕获调用时已完成的订阅变化，外层继续使用自己的快照。

多个尚未完成的 `InvokeAsync` 可以在 await 边界交错运行，各自持有独立快照，完成顺序可以与启动顺序不同。处理器需要等待嵌套派发时，应 await 新一次 `InvokeAsync` 返回的任务。

处理器同步抛出或 await 后产生的异常，包括 `OperationCanceledException`，都会记录到 `FDebug.LogError`，随后继续执行后续处理器。调用方 await 用于等待本次派发结束，处理器的失败不会聚合后重新抛给调用方；需要汇总业务成功或失败时，由事件参数或业务状态显式表达。

## 线程与内存

调用事件 API、修改订阅以及处理器 await 后的续体都必须在同一线程执行。ETask 不会自动将续体切回 Unity 主线程；处理器使用其他异步来源时，需要自行安排回到事件所属线程后再访问事件或 Unity 对象。

每次派发从池中租借独立 Snapshot，其中保存两组可复用 List；全部处理器结束后，在 `finally` 中清除引用并归池。挂起期间 Snapshot 仍归该次派发独占，因此同类型嵌套和多个交错调用可分别保留快照。

池化快照减少重复分配，不保证整个调用过程零 GC。首次创建快照、List 扩容、同时挂起派发数量超过池中历史可用数量，以及业务委托、闭包、ETask 和异常日志都可能分配；池中快照保留各自的 List 容量供后续复用。

源码：[Event.cs](../../EasyFramework/Runtime/Manager/Event/Event.cs)、[AsyncEvent.cs](../../EasyFramework/Runtime/Manager/Event/AsyncEvent.cs)。

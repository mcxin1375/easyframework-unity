# F.Event

`F.Event` 的类型是 `Event`，同步方法 `Add/Remove/Invoke` 按事件参数类型分组管理 `IEvent<T>` 和 `Action<T>` 处理器。异步处理器使用同一入口的 `AddAsync/RemoveAsync/InvokeAsync`，见 [异步事件](AsyncEvent.md)。两组订阅完全独立，即使参数类型相同，`Invoke` 也只触发同步处理器，`InvokeAsync` 也只触发异步处理器。

| API | 说明 |
| --- | --- |
| `Add<T>(IEvent<T>)` | 注册对象事件处理器；重复对象不会重复加入 |
| `Add<T>(Action<T>)` | 注册委托处理器；空委托忽略 |
| `Remove<T>(IEvent<T>)` | 移除对象处理器 |
| `Remove<T>(Action<T>)` | 移除委托处理器 |
| `Invoke<T>(in T args)` | 按参数类型同步派发事件 |

```csharp
readonly struct LoginCompleted { }

sealed class LoginEvents
{
    public void Subscribe()
    {
        F.Event.Add<LoginCompleted>(OnLoginCompleted);
        F.Event.Invoke(new LoginCompleted());
        F.Event.Remove<LoginCompleted>(OnLoginCompleted);
    }

    private void OnLoginCompleted(LoginCompleted args) { }
}
```

每次 `Invoke` 在执行任何回调前，捕获当前 `IEvent<T>` 和 `Action<T>` 的订阅快照；先调用对象处理器，再调用委托处理器，各组内按注册顺序执行。回调中新增或移除订阅不会改变当前派发：已移除的处理器仍会收到本次事件，新增的处理器从下一次 `Invoke` 开始接收。嵌套 `Invoke` 是一次独立派发，会看到调用前已经完成的订阅变化，外层继续使用自己的快照。移除后重新注册会排在该组末尾，重复注册、空处理器和重复移除均被忽略。

订阅表和快照均使用可复用的 `List`。每次 `Invoke` 将两组订阅追加到快照尾部，记录各自的起点和数量，仅遍历本次区间；嵌套派发追加独立区间，退出时在 `finally` 中删除自己的尾段、清除引用并保留容量，不覆盖外层快照。

有效 Add/Remove 不再每次创建数组；订阅表和快照容量足够后，框架内部的订阅变更与派发不产生新的托管分配。首次使用、订阅增加或嵌套深度增加导致 List 扩容时仍会分配，业务回调、委托创建及异常日志的分配不在此保证内。代价是每次 `Invoke` 都有 O(n) 的复制和清引用开销，快照保留所有活跃嵌套区间所需的历史峰值容量。

事件 API 用于同一线程内的同步调用，支持回调重入，不提供跨线程订阅或派发同步。

事件处理器异常会被框架捕获并记录，不应依赖异常中断后续处理器。订阅对象销毁时必须解除订阅，避免持有失效对象或重复响应。

源码：[Event.cs](../../Runtime/Core/Event/Event.cs)。

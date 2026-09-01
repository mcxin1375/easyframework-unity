# F.Event

`F.Event` 的类型是 `Event`，按事件参数类型分组管理 `IEvent<T>` 和 `Action<T>` 处理器。

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

事件处理器异常会被框架捕获并记录，不应依赖异常中断后续处理器。订阅对象销毁时必须解除订阅，避免持有失效对象或重复响应。

源码：[Event.cs](../../Runtime/Core/Event/Event.cs)。

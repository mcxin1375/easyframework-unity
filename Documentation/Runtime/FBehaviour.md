# F.Behaviour

`F.Behaviour` 是继承 MonoBehaviour 的生命周期宿主。首次访问时创建名为 `[EasyFramework]` 的 GameObject，通过 DontDestroyOnLoad 跨场景保留。

## 帧循环

宿主通过 `Add(IFrameworkTick)` 和 `Remove(IFrameworkTick)` 管理订阅者。Update 刷新注册列表并调用 OnUpdate，LateUpdate 调用 OnLateUpdate。ControllerManager、WindowManager、InputManager 等模块在自身创建时注册。WindowManager 直接承担窗口更新和超时销毁，不再依赖独立的 UIWindowBehaviour 组件。

```csharp
using EasyFramework;

public sealed class GameTick : IFrameworkTick
{
    public void OnUpdate() { }
    public void OnLateUpdate() { }
    public void OnDestroy() { }
}
```

初始化后调用 `F.Behaviour.Add(tick)`，停止使用时传同一个实例调用 Remove。注册列表在下一次 Update 刷新；不要同时手动驱动已注册对象。ETask 使用自己的 PlayerLoop 驱动。

宿主不会逐订阅者捕获异常，模块自行决定回调的异常隔离范围。ControllerManager 在逐实例范围记录帧资格求值及 Update / LateUpdate 回调异常，WindowManager 在窗口帧入口统一记录异常；具体范围见 [ControllerManager](ControllerManager.md)。不要将这些模块和 ETask / Ticker 的异常隔离等同于宿主全部订阅者的隔离。

## 销毁

宿主销毁时通知当前活动列表中的 IFrameworkTick.OnDestroy。移除订阅不会调用该回调，业务需要自行清理移除对象。

当前宿主销毁后不会自动重建，也不会统一销毁所有单例或 ECS World。不要手动销毁宿主后继续使用框架。启动流程放在业务组件中并等待 F.InitializeAsync()。

源码：[FBehaviour.cs](../../Runtime/Behaviour/FBehaviour.cs)、[IFrameworkTick.cs](../../Runtime/Behaviour/IFrameworkTick.cs)。

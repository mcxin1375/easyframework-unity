# F.ControllerManager

`F.ControllerManager` 的类型是 IControllerManager，保存已进入的 Controller 栈。创建与业务生命周期由 `Controller<T>` 管理，通过业务类型的 Instance 使用。

## 管理器接口

| API | 说明 |
| --- | --- |
| `TopController` | 栈顶 Controller，空栈时为 null |
| `EnterList` | 当前进入列表的只读视图 |
| `HasEnter(IController)` | 是否在进入列表中 |
| `HasActive(IController)` | 是否为当前栈顶 |
| `Enter(IController)` / `Remove(IController)` | 底层栈维护，不执行完整业务进入、退出回调 |
| `ExitAllAsync()` | 当前实现存在退出回调问题，见下文 |

当前没有管理器级 EnterAsync<T>()、GetOrCreate<T>()、Current、SetActiveAsync<T>() 或 EControllerEnter。业务不应直接使用 Enter / Remove 代替完整生命周期。

## 定义与使用

```csharp
using EasyFramework;

public sealed class GameController : Controller<GameController>
{
    protected override void OnEnter() => FDebug.Log("Enter");
    protected override ETask OnEnterAsync() => ETask.CompletedTask;
    protected override void OnExit() => FDebug.Log("Exit");
}

public static class ControllerExample
{
    public static async ETask RunAsync()
    {
        await F.InitializeAsync();
        await GameController.Instance.EnterAsync();
        bool active = F.ControllerManager.HasActive(GameController.Instance);
        await GameController.Instance.ExitAsync();
        GameController.DestroyInstance();
    }
}
```

默认保留已进入的其他 Controller，仅栈顶激活。IsEnter、IsActive 查询当前状态；OnEnterEvent、OnExitEvent 在各自流程结束时触发。不要在同一 Controller 的异步进入尚未结束时再次进入。

## 生命周期与组件

首次访问 Instance 时执行 OnCreate 并创建发现的 IControllerComponent。进入依次执行 OnPreEnterAsync、入栈、OnAddListeners、OnEnter / OnEnterAsync、OnAfterEnter / OnAfterEnterAsync，并将对应阶段转发给组件。退出时先移出栈，再移除监听、执行同步和异步退出回调。

需要参数时实现对应的 ITParams<T1...T3>，通过实例的 EnterAsync(value1, ...) 传入；接口成员需要自行实现或由包内 Roslyn 生成器生成。DestroyInstance() 不会自动退出，应先等待 ExitAsync()。

## 当前实现限制

- ControllerManager 的 _refresh 未在栈变动时置为 true，内部更新列表不会填充，Controller 的 OnUpdate / OnLateUpdate 当前不会正常分发。基础示例不依赖这些回调。
- ExitAllAsync() 在调用 Controller.ExitAsync() 前先移除列表元素，导致 IsEnter 检查提前返回，跳过退出回调与监听清理。ExitOtherBeforeEnter = true 同样触发此路径。
- 需要批量退出时，先复制 EnterList，再逆序等待各实例的 ExitAsync()，避免枚举过程中修改原列表。

源码：[IControllerManager.cs](../../Runtime/Manager/ControllerManager/IControllerManager.cs)、[ControllerManager.cs](../../Runtime/Manager/ControllerManager/ControllerManager.cs)、[Controller.cs](../../Runtime/Manager/ControllerManager/Controller.cs)、[Controller.Singleton.cs](../../Runtime/Manager/ControllerManager/Controller.Singleton.cs)。

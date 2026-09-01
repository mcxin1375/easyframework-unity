# F.ControllerManager

`F.ControllerManager` 的类型是 `IControllerManager`，负责 Controller 的创建、进入、退出、激活切换，以及 Update/LateUpdate 生命周期。

## 状态

| API | 说明 |
| --- | --- |
| `Current` | 当前栈顶 Controller |
| `EnterList` | 当前进入列表，只读视图 |
| `OnEnter` / `OnExit` | Controller 进入或退出事件 |
| `Get<T>()` / `Get(Type)` | 获取已创建 Controller，不存在时返回空 |
| `GetOrCreate<T>()` / `GetOrCreate(Type)` | 获取或创建 Controller |
| `HasEnter<T>()` / `HasEnter(Type)` | 判断是否已进入 |
| `HasActive<T>()` | 判断是否激活 |
| `Create(Assembly)` | 扫描程序集并创建 Controller |

## 生命周期 API

| API | 说明 |
| --- | --- |
| `EnterAsync<T>(mode)` | 进入 Controller |
| `EnterAsync<T, TK1..TK3>(...)` | 带 1 至 3 个参数进入，实现对应 `ITParams` |
| `EnterAsync(Type/IController, mode)` | 按类型或实例进入 |
| `SetActiveAsync<T>(bool)` | 切换已进入 Controller 的激活状态 |
| `ExitAsync<T>()` / `ExitAsync(Type)` | 退出指定 Controller |
| `ExitAllAsync()` | 退出全部 Controller |

`EControllerEnter` 有 `Single`、`Additive` 和 `AdditiveBackground` 三种模式。所有进入、退出和激活操作都是异步命令，应等待返回的 `ETask`，再依赖状态继续执行。

项目示例在 `GameLoginState` 中调用 `F.ControllerManager.EnterAsync<LoginController>()`，在 `LoginController.OnEnterAsync` 中再等待窗口和资源操作。

源码：[IControllerManager.cs](../../Runtime/Manager/ControllerManager/IControllerManager.cs)。

# F.ControllerManager

`F.ControllerManager` 的类型是 `IControllerManager`，通过 `IController` 统一管理实例、进入栈、激活及帧分发。`Controller<T>` 提供业务回调、组件支持、监听维护、实例入口转发和只读查询，不保存另一份实例缓存，也不决定进入退出时机。

管理器不引用 IControllerComponent，也不反射发现或分发组件。组件、监听、AfterEnter 和完成事件完全是 Controller<T> 的内部实现；业务可以直接定义 CustomController : IController，使用同一套管理器入口进入、刷新和退出，自行实现内部行为，无需继承基类或实现组件、AfterEnter、完成通知等额外接口约定。

所有入口在 Unity 主线程调用。继承基类的业务优先调用 X.Instance.EnterAsync(...) / ExitAsync()，它们等待管理器完成后，在实例入口末尾发送自身的进入或退出事件；自定义 IController 使用管理器的泛型入口。每个返回的 ETask 只消费一次。

## 管理器接口

| API | 说明 |
| --- | --- |
| `EnterAsync<T>()` | 创建或复用 T，使用默认参数进入，返回 `ETask<T>` |
| `EnterAsync<T, T1>(in p1)` | 强类型传入一个参数，T 必须实现 `ITParams<T1>` |
| `EnterAsync<T, T1, T2>(in p1, in p2)` | 强类型传入两个参数 |
| `EnterAsync<T, T1, T2, T3>(in p1, in p2, in p3)` | 强类型传入三个参数 |
| `ExitAsync<T>()` / `ExitAsync(IController)` | 退出当前实例；尚未创建或已退出时直接完成 |
| `ExitAllAsync()` | 逆序退出调用时捕获的进入批次，跳过已经换轮次的实例 |
| `GetController<T>()` | 查询当前缓存的实例，不创建、不进入 |
| `TopController` | 当前栈顶 Controller，空栈时为 null |
| `EnterList` | 当前进入栈的只读视图 |
| `HasEnter(IController)` | 当前实例是否已经入栈 |
| `HasActive(IController)` | 当前实例是否为栈顶 |

进入用的泛型类型需要满足 `class, IController, new()`；有参重载另有对应的 `ITParams` 约束。按实例退出只处理与管理器缓存引用一致的实例；未登记或已经替换的旧实例直接完成，不影响当前实例。底层 Enter / Remove 及 DestroyAsync 不作为公共入口。退出后实例仍保留在会话缓存中，后续进入复用该实例。

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
        var controller = await GameController.Instance.EnterAsync();
        bool active = F.ControllerManager.HasActive(controller);
        await controller.ExitAsync();
    }
}
```

`Controller<T>.Instance` 转发到管理器，按需创建同一份实例。实例的无参、1～3 参数 EnterAsync 和 ExitAsync 将生命周期调用交给管理器，成功完成后再由实例入口发送 OnEnterEvent / OnExitEvent；业务可以直接写 `BattleController.Instance.EnterAsync(args)`。不提供公开 DestroyAsync 或 DestroyInstance，结束本轮业务时等待 ExitAsync。

实例入口与管理器入口的事件行为不同：

| 入口 | 生命周期 | 基类的完成事件 |
| --- | --- | --- |
| `X.Instance.EnterAsync(...)` / `ExitAsync()` | 委托管理器执行 | Controller 的实例入口在管理器完成后自行发送 |
| `F.ControllerManager.EnterAsync<X>(...)` / `ExitAsync<X>()` | 管理器执行 | 不发送 OnEnterEvent / OnExitEvent |
| `F.ControllerManager.ExitAllAsync()` | 管理器执行批量退出 | 不发送基类的 OnExitEvent |

事件不属于管理器的接口约定。自定义 IController 可以按自己的实现决定内部阶段和通知，管理器不会附加基类事件行为。

## 参数与刷新进入

管理器先检查并占用本实例的进入操作，再调用 `SetParams`；无参进入对实现 `ITParams` 的实例调用 `SetParamsDefault()`。被拒绝的重复或反向调用不会先覆盖正在使用的参数。参数 setter 本身抛错也会释放操作标记，然后向调用方传播。

相关参数较多时，推荐组成一个参数结构体，并让 Controller 实现已有的 `ITParams<TArgs>`：

```csharp
var args = new BattleEnterArgs(levelId, mode);
await BattleController.Instance.EnterAsync(args);
// 也可以使用管理器的强类型入口：
await F.ControllerManager.EnterAsync<BattleController, BattleEnterArgs>(args);
```

接口成员可自行实现，或按包内 Roslyn 生成器约定生成。实例转发支持 0～3 个参数，参数不匹配对应的 ITParams 接口时明确抛异常，不再静默忽略；管理器的有参泛型入口则通过约束在编译期拒绝不匹配类型。两种入口都先由管理器检查过渡冲突，再调用 SetParams，被拒绝请求不会提前修改参数。

已经进入的实例再次 `EnterAsync` 时应用最新参数，重新执行 PreEnter、Enter、AfterEnter，并将原实例移到栈顶；不先 Exit，不重复添加栈项，也不重复绑定监听。退出后再次进入才会重新绑定监听。无参刷新同样会执行默认参数设置和进入阶段。

## 生命周期与组件

首次创建时，管理器仅调用 IController.OnCreate。Controller<T> 在自己的接口实现中发现 IControllerComponent，先为全部组件注册 owner，再执行业务 OnCreate 和组件 OnCreate。OnCreate 属于实例生命周期，发生在本轮进入参数绑定之前，不应依赖某次进入参数。自定义 IController 自行实现该阶段，管理器不为它附加组件行为。

每次进入按以下顺序执行。管理器只调用 IController 的准备、进入接口；使用基类时，进入接口内部由基类按业务 Controller 在前、组件在后的顺序转发：

1. 占用进入操作，设置本轮参数。
2. 执行 OnPreEnterAsync。
3. 入栈或将原实例移到栈顶，提交新的进入轮次，更新激活状态。
4. 调用 IController.OnEnterAsync；基类在此处按需执行内部 OnAddListeners，再执行 OnEnter / OnEnterAsync 及组件进入。
5. 基类继续执行内部 OnAfterEnter / OnAfterEnterAsync 及组件 AfterEnter。
6. 进入接口返回后，由管理器释放本次操作标记并完成管理器的 EnterAsync。
7. 如果通过 Controller 实例入口进入，实例入口等待上述流程成功后，再发送 OnEnterEvent。

IController 不声明 OnAfterEnterAsync、OnEntered 或 OnExited。protected OnAfterEnter / OnAfterEnterAsync 仍作为基类业务扩展点保留，由基类在 IController.OnEnterAsync 内部编排；完成事件由实例入口自行发送，管理器既不调度 AfterEnter，也不发送完成通知。

保持提前入栈语义：进入回调中 IsEnter 已为 true；异步 Enter 尚未结束时，已经入栈的实例也可以接收帧更新。默认只有栈顶激活，只有实际变化的旧、新栈顶收到激活通知。

管理器按实例记录激活版本，回调中切到其他 Controller 后又切回，旧激活通知不会再次发送。Controller<T> 自己用本地进入/退出版本保护组件帧分发，用激活版本及当前激活结果保护组件激活通知；与本实例激活状态无关的其他栈顶切换或失败的 PreEnter，不会丢失它仍有效的停用通知。

退出时先占用退出操作并移栈，停止该轮帧分发、更新栈顶激活，再调用 IController.OnExitAsync。Controller<T> 在该接口实现内先按需执行内部 OnRemoveListeners，再执行 OnExit / OnExitAsync 及组件退出。通过 Controller 实例入口退出时，实例入口等待管理器成功完成并释放操作标记后发送 OnExitEvent；直接管理器退出不发送该事件。监听绑定状态由基类维护：刷新进入不重复绑定，退出后再次进入才重新绑定；IController 和管理器均不暴露或调度 OnAddListeners / OnRemoveListeners。自定义 IController 自行选择自己的监听方式。

实例入口的完成事件在管理器释放操作保护后发送，事件中可以对同一实例再次进入或退出。旧操作不会在事件返回后覆盖新一轮状态。也可以在外层 await Instance.EnterAsync() 或 await Instance.ExitAsync() 完成后顺序发起下一次操作。

## 重复调用与异常

| 当前情况 | EnterAsync | ExitAsync |
| --- | --- | --- |
| 尚未创建或已退出 | 创建或复用实例，开始进入 | 直接完成，不创建实例 |
| 已进入且没有过渡操作 | 刷新进入，应用最新参数 | 开始退出 |
| 正在进入 | 抛 InvalidOperationException | 抛 InvalidOperationException |
| 正在退出 | 抛 InvalidOperationException | 抛 InvalidOperationException |

不同 Controller 可以独立进入退出；同一实例的冲突调用直接失败，不自动排队，也不共享一个 ETask 给多个等待者。

生命周期回调首错中断，异常正常向上传播；管理器通过 finally 释放操作标记，不自动补偿 Exit，也不设置 Faulted 来禁止后续调用：

- 首次进入的 PreEnter 失败：尚未入栈。
- 刷新进入的 PreEnter 失败：保留原栈位置，已经写入的新参数不回滚。
- 入栈后的 Enter / AfterEnter 失败：保留已提交的栈和激活结果。
- 退出清理失败：已经移栈，保持退出，后续清理阶段不再执行。

IsEnter 表示管理器中的入栈状态，不代表全部业务回调成功。业务负责处理自身回调失败后未完成的资源清理；管理器不会据此推断业务资源是否已释放。

完成事件是 Controller 实例入口在管理器成功完成后的行为。事件回调抛错会使对应的实例 EnterAsync / ExitAsync 向调用方传播异常，但此时管理器流程已经完成，不撤销已提交的栈关系；此前生命周期阶段失败则不发送事件。直接调用管理器没有这项事件阶段。ExitAsync 结束本轮业务但不移除实例缓存；业务没有公开销毁接口，不依赖内部清理方式。

## 批量退出与帧分发

每次入栈或刷新进入提交时更新进入轮次。ExitAllAsync 捕获“实例及轮次”，逆序执行前确认轮次仍匹配。即使 B 的退出回调先退出再重新进入 A，旧批次也不会再次退出 A；回调中新进入的实例不属于这一批。调用时尚未入栈的进入操作不会被隐式取消。

ExitAllAsync 尝试处理整个批次：一个实例退出失败后仍继续其他实例，最后向调用方报告失败。只有一个错误时保留原异常；多个错误以 AggregateException 返回。单实例的清理仍然是首错中断。

栈变化和新轮次提交会标记帧快照待刷新。下次 OnUpdate 捕获实例和轮次，OnLateUpdate 使用同一快照；调用 IController 前检查成员资格、轮次和 UpdateEnabled。帧回调中新进入或刷新进入的轮次从下一帧参与管理器更新，退出的轮次立即停止后续分发。基类在自己的帧回调返回后检查本地进入/退出版本，再决定是否继续组件调用；即使刷新进入在激活回调处失败，也不会继续旧的组件帧工作。自定义 IController 的内部转发方式由它自己决定。

帧资格检查及 Update / LateUpdate 回调均在现有逐实例 try 内执行。自定义 IController 的 UpdateEnabled getter，或基类重写的 UpdateEnabled 抛错时，只记录该实例异常并继续后续实例，不向宿主传播该异常。2026-11-04 的资格求值隔离缺口已修复，对应 Update / LateUpdate 故障注入均通过；没有增加捕获层。

需要先退出其他 Controller 再进入时，由业务显式编排：

```csharp
await F.ControllerManager.ExitAllAsync();
await NextController.Instance.EnterAsync();
```

这不是事务切换：旧 Controller 已退出后，新 Controller 进入失败不会自动恢复旧栈。原来的 ExitOtherBeforeEnter 不再放在薄基类中。

源码：[IControllerManager.cs](../../Runtime/Manager/ControllerManager/IControllerManager.cs)、[ControllerManager.cs](../../Runtime/Manager/ControllerManager/ControllerManager.cs)、[IController.cs](../../Runtime/Manager/ControllerManager/IController.cs)、[Controller.cs](../../Runtime/Manager/ControllerManager/Controller.cs)、[ControllerComponent.cs](../../Runtime/Manager/ControllerManager/ControllerComponent.cs)。

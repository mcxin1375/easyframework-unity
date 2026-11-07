# F.WindowManager

`F.WindowManager` 的类型是 `IWindowManager`，负责 UI Root、UI Layer 和窗口实例的创建、打开、关闭、刷新与销毁。业务代码通过 `F.WindowManager` 访问它，不直接依赖内部的 `WindowManager` 实现类。窗口管理逻辑集中在 WindowManager，通过 IFrameworkTick 注册到 FBehaviour 的帧循环，不再添加独立的 UIWindowBehaviour 组件。

## 前置条件

调用窗口 API 前，必须先等待框架初始化完成：

```csharp
await F.InitializeAsync();
await F.WindowManager.OpenAsync<HelloWorldWindow1>(UILayer.HUD);
```

UI Root、各个 `UILayer` 和窗口资源也必须已经配置。`UILayer` 当前包含 `HUD`、`Popup`、`Notice`、`Higher`、`Loading` 和组合值 `All`。

## 管理器属性

| API | 说明 |
| --- | --- |
| `UIRoot` | 当前 UI 根节点 |
| `EventSystem` | 当前 Unity UI 事件系统 |
| `Resolution` | UI 分辨率，可读写 |
| `EventSystemEnabled` | UI 事件系统开关，可读写 |

## 打开窗口

| API | 说明 |
| --- | --- |
| `Open<T>(UILayer)` | 同步创建并打开窗口，默认层为 `HUD` |
| `Open<T, T1>(layer, value1)` | 带一个参数打开窗口，窗口实现 `ITParams<T1>` |
| `Open<T, T1, T2>(layer, value1, value2)` | 带两个参数打开窗口，窗口实现 `ITParams<T1, T2>` |
| `Open<T, T1, T2, T3>(layer, value1, value2, value3)` | 带三个参数打开窗口，窗口实现 `ITParams<T1, T2, T3>` |
| `Open(Type, layer, object[])` | 按运行时类型和参数打开窗口 |
| `OpenAsync<T>(UILayer)` | 异步创建并打开窗口，返回 `ETask<T>` |
| `OpenAsync<T, T1..T3>(...)` | 异步带参打开窗口 |
| `OpenAsync(Type, layer, object[])` | 异步按运行时类型打开窗口 |

泛型窗口需要满足 `class, IWindow, new()`。优先使用泛型 API，这样窗口类型和参数约束可以在编译期检查。

窗口入口在 Unity 主线程调用。Window 的对象在首次 Open / OpenAsync 时创建，IWindow.Create() 是管理器创建实例时的入口。异步加载由 IsLoading 标记保护，加载结束或抛错都会在 finally 中复位；仅在加载中直接重复调用 IWindow.Open / OpenAsync 时抛出 InvalidOperationException，已加载对象仍可复用。管理器在修改层登记前检查 IsLoading，重叠请求记录错误并返回 null，保留首个请求及其登记；打开过程抛错则清理对应实例并返回 null。资源对象为 null 也视为打开失败，后续请求可以重新创建。参数仍在管理器入口写入，拒绝的重叠请求也可能改变参数，因此同类型打开应顺序 await。

Window 的 Alive 初始为 true，Close 后仍为 true，Destroy 入口将其置为 false；Create 不会重新激活已销毁的实例。Open / OpenAsync 在入口检查 Alive，为 false 时抛出包含窗口类型的 InvalidOperationException。通过管理器调用时，由管理器捕获并记录异常、返回 null；直接调用 IWindow 时由调用方处理异常。该入口检查不等于异步创建期间的取消或迟到对象回收。


## 关闭与销毁

| API | 说明 |
| --- | --- |
| `Close(IWindow)` / `Close<T>()` | 关闭指定窗口；窗口对象通常保留以便再次打开 |
| `CloseLayer(UILayer)` | 关闭指定层的窗口 |
| `CloseAll()` | 关闭全部窗口 |
| `Destroy(IWindow)` / `Destroy<T>()` | 销毁窗口实例及其 GameObject |

`Close` 和 `Destroy` 语义不同：需要暂时隐藏时关闭，需要释放实例时销毁。关闭会移除层映射及层列表登记，类型缓存仍保留实例供重新打开。WindowManager 在需要刷新时，从类型缓存建立打开窗口的 `_updateList` 和已关闭窗口的 `_closedList`；更新时跳过已经移除层映射或仍在加载的实例，关闭缓存的 `KeepAlive` 为 `false` 时经管理器销毁。`Window` 默认在关闭后保留 60 秒，可通过重写 `KeepAliveTime` 调整时长。

管理器销毁前先验证类型缓存仍指向传入实例，再删除该缓存并移除层关系；旧关闭缓存不能删除同类型的新实例。Window.Destroy 先同步调用 Close，再执行窗口 OnDestroy、组件 OnDestroy 和原生对象销毁。窗口快照按需在后续 Update 刷新，本帧已无层映射的实例停止更新，回调中新开的实例从下一次快照刷新参与 Update。

窗口销毁后不能继续使用原实例；经管理器销毁后，再次通过管理器 Open 得到新实例。管理器拒绝不属于当前类型缓存的旧实例。Window.Destroy 通过 Alive 防止重复及递归执行，销毁后的 SetActive、Refresh、RefreshOrder 和 Update 直接返回。直接调用 IWindow.Destroy 不会主动撤销管理器登记；下次通过管理器打开命中该旧实例时，打开失败并清除旧缓存，返回 null。创建中 Destroy 不会取消加载或回收迟到对象，业务应等待打开操作完成再销毁。

Destroy 用一层 try/catch 包住 Close、窗口 OnDestroy 和组件 OnDestroy。普通生命周期回调抛错后，记录原异常并停止后续业务回调，随后仍清空组件列表、销毁 GameObject 并清空 WindowObject。Alive 在回调前已经置为 false，重复 Destroy 不重试业务清理。

Destroy 不主动调用资源卸载。窗口作为 IResRequest 的 Alive=false 后，不再以活跃持有者身份阻止资源回收；AssetBundleLoader 在缓存期限到期且没有其他活跃持有者时，通过后续 Tick 回收请求。关闭窗口时 Alive 仍为 true，继续保留资源供重新打开使用。

窗口帧更新由 FBehaviour 驱动，禁用 UI Root 不会停止管理器的更新和超时检查。窗口更新异常在管理器的帧入口统一记录，避免跳过宿主中后续模块；运行时层级和窗口调试信息显示在 UI Root 的 Inspector。

## 查询与刷新

| API | 说明 |
| --- | --- |
| `IsOpen<T>()` | 查询窗口是否打开 |
| `GetWindow<T>()` / `GetWindow(Type)` | 获取已创建窗口，不负责打开 |
| `GetTopWindow(UILayer)` | 获取指定层顶部窗口 |
| `GetWindows(UILayer)` | 获取指定层全部窗口 |
| `GetLayerData(UILayer)` | 获取层行为数据 |
| `GetCamera(UILayer)` / `GetCanvas(UILayer)` | 获取层对应的 Camera 或 Canvas |
| `RefreshAllWindow()` | 刷新全部窗口及其组件 |

IsOpen 查询管理器中的层登记，因此打开已登记但仍在加载时也可能为 true；需要使用原生对象时必须等待 OpenAsync 完成。GetWindows / GetTopWindow 同样依据层列表返回实例。RefreshAllWindow 使用当前更新快照，并跳过已移除或加载中的窗口。

每层由 WindowLayerInfo 保存顺序。普通 Add / Remove 只修改列表；只有正在排序时发生变更才立即嵌套重排，旧循环在当前窗口回调返回后检测 UID 并退出，finally 复位标记。打开操作结束后管理器刷新该层排序；窗口及组件回调逐次读取 Canvas.sortingOrder。移除窗口后允许排序值留空，只要求剩余窗口相对顺序正确，不为压紧数值额外重排。

## 定义窗口

### 基础窗口

继承 `Window`，重写生命周期回调：

```csharp
using EasyFramework;
using UnityEngine;

public class HelloWorldWindow1 : Window
{
    protected override void OnOpen()
    {
        FDebug.Log("HelloWorldWindow - Open");
    }

    protected override void OnClose()
    {
        FDebug.Log("HelloWorldWindow - Close");
    }

    protected override GameObject CreateWindowObject(Transform parent)
    {
        return base.CreateWindowObject(parent);
    }

    protected override ETask<GameObject> CreateWindowObjectAsync(Transform parent)
    {
        return base.CreateWindowObjectAsync(parent);
    }
}
```

`Window` 默认通过 `F.ResLoader` 加载资源对象，资源名必须与窗口类型名一致；上例中资源名为 `HelloWorldWindow1`。同步创建使用 `F.ResLoader.CreateObj(Type.Name, parent)`，异步创建使用 `F.ResLoader.CreateObjAsync(Type.Name, parent, this)`。如果默认规则满足需求，可以像上例一样直接调用基类实现；需要自定义资源来源时再重写这两个方法。

窗口对象创建后先为组件 Register 注册 owner，再执行窗口与组件的 OnLoadObject；打开时注册监听器、执行 OnOpen 和刷新，关闭后重新打开不会重复 OnLoadObject，但会重新注册监听器。关闭时移除监听器、执行 OnClose 并隐藏 GameObject。加载、打开阶段的回调抛错会中止本次打开并由管理器清理；排序回调异常由 WindowLayerInfo 记录。

### 自动绑定 UI

实现 `IWindowUI<TUI>` 后，窗口会把 UI 对象中同名字段绑定到 `TUI`：

```csharp
using EasyFramework;
using UnityEngine.UI;

public class HelloWorldWindow2UI : WindowComponent
{
    public Image TestImg;
    public Text TestText;
}

public partial class HelloWorldWindow2 : Window, IWindowUI<HelloWorldWindow2UI>
{
    protected override void OnOpen()
    {
        FDebug.Log(UI.TestText.text);
    }
}
```

此片段依赖包内 Roslyn 生成器，为 partial 窗口生成 UI 属性与 InitializeUI。TUI 必须实现 IWindowComponent 并有公开无参构造函数，因此这里继承 WindowComponent。字段名必须与 UI 对象绑定名称和生成约定一致；绑定失败时不要假设 UI 成员已可用。无需生成器的完整窗口见 [基础示例](../Examples.md)。

### 带参数窗口

通过 ITParams 接收参数。下面的 partial 类型依赖包内 Roslyn 生成器生成 T1、T2、SetParams 等接口成员；如果不使用生成器，需要显式实现这些成员：

```csharp
using EasyFramework;

public struct HelloWorldWindow3Params
{
    public int Value1;
}

public partial class HelloWorldWindow3 : Window, ITParams<HelloWorldWindow3Params, string>
{
    protected override void OnOpen()
    {
        FDebug.Log(T1.Value1);
        FDebug.Log(T2);
    }
}
```

打开时参数顺序必须与 `ITParams<T1, T2>` 的类型顺序一致：

```csharp
await F.WindowManager.OpenAsync<HelloWorldWindow3,
    HelloWorldWindow3Params, string>(
    UILayer.HUD,
    new HelloWorldWindow3Params { Value1 = 1 },
    "123");
```

### 指定资源路径

继承 `WindowResources` 时，默认资源路径为 `Resources/{Type.Name}`。资源位于其他路径时，重写 `ResourcePath` 指定相对于 `Resources` 的路径：

```csharp
using EasyFramework;

public class HelloWorldResourcesWindow : WindowResources
{
    protected override string ResourcePath => "Windows/HelloWorldResourcesWindow";
}
```

资源路径必须与项目资源布局一致。该用法适用于窗口类型名与资源名不一致的情况。

## 项目示例

业务异步方法中的调用方式如下（先在启动流程等待 F.InitializeAsync）：

```csharp
public static async ETask Test()
{
    await F.WindowManager.OpenAsync<HelloWorldWindow1>(UILayer.HUD);
    await F.WindowManager.OpenAsync<HelloWorldWindow3,
        HelloWorldWindow3Params, string>(
        UILayer.HUD,
        new HelloWorldWindow3Params(),
        "123");
}
```

业务层应等待 OpenAsync 完成后再访问窗口对象。完整的可导入窗口、加载中销毁与关闭清理示例见 [基础示例](../Examples.md)。

## 源码

- [IWindowManager.cs](../../EasyFramework/Runtime/Manager/WindowManager/IWindowManager.cs)
- [IWindow.cs](../../EasyFramework/Runtime/Manager/WindowManager/IWindow.cs)
- [Window.cs](../../EasyFramework/Runtime/Manager/WindowManager/Window.cs)
- [WindowLayerInfo.cs](../../EasyFramework/Runtime/Manager/WindowManager/WindowLayerInfo.cs)
- [WindowResources.cs](../../EasyFramework/Runtime/Manager/WindowManager/WindowResources.cs)

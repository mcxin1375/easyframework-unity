# F.WindowManager

`F.WindowManager` 的类型是 `IWindowManager`，负责 UI Root、UI Layer 和窗口实例的创建、打开、关闭、刷新与销毁。业务代码通过 `F.WindowManager` 访问它，不直接依赖内部的 `WindowManager` 实现类。

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

## 关闭与销毁

| API | 说明 |
| --- | --- |
| `Close(IWindow)` / `Close<T>()` | 关闭指定窗口；窗口对象通常保留以便再次打开 |
| `CloseAsync(IWindow)` / `CloseAsync<T>()` | 等待窗口关闭前的异步流程 |
| `CloseLayer(UILayer)` | 关闭指定层的窗口 |
| `CloseAll()` | 关闭全部窗口 |
| `Destroy(IWindow)` / `Destroy<T>()` | 销毁窗口实例及其 GameObject |

`Close` 和 `Destroy` 语义不同：需要暂时隐藏时关闭，需要彻底释放实例时销毁。窗口关闭后不要假设它已经被销毁；窗口销毁后则不能继续使用原实例。

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

窗口对象创建后会初始化窗口组件并触发 `OnCreate`；打开时触发监听器注册、`OnOpen` 和刷新；关闭时移除监听器、触发 `OnClose` 并隐藏 GameObject。

### 自动绑定 UI

实现 `IWindowUI<TUI>` 后，窗口会把 UI 对象中同名字段绑定到 `TUI`：

```csharp
using EasyFramework;
using UnityEngine.UI;

public class HelloWorldWindow2UI
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

示例工程标注该 UI 代码由 Roslyn 生成。字段名必须与 UI 对象中的绑定名称和生成代码约定一致；绑定失败时不要假设 `UI` 成员已可用。

### 带参数窗口

通过 `ITParams` 接收打开窗口时传入的参数。示例工程使用两个参数：

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

继承 `WindowResources` 时，默认资源路径为 `Resources/{Type.Name}`；也可以使用 `WindowResourcesPath` 指定路径：

```csharp
using EasyFramework;

[WindowResourcesPath("Windows/HelloWorldResourcesWindow")]
public class HelloWorldResourcesWindow : WindowResources
{
}
```

资源路径必须与项目资源布局一致。该用法适用于窗口类型名与资源名不一致的情况。

## 项目示例

示例工程中的 `WindowTests.Test()` 展示了实际调用：

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

业务层应等待 `OpenAsync` 完成后再访问窗口对象或继续依赖窗口资源的流程。登录流程中的 `LoginController.OnEnterAsync` 也遵循这一规则：先等待 `F.WindowManager.OpenAsync<LoginWindow>()`，再关闭旧窗口并创建资源对象。

## 源码

- [IWindowManager.cs](../../Runtime/Manager/WindowManager/IWindowManager.cs)
- [IWindow.cs](../../Runtime/Manager/WindowManager/IWindow.cs)
- [Window.cs](../../Runtime/Manager/WindowManager/Window.cs)
- [WindowResources.cs](../../Runtime/Manager/WindowManager/WindowResources.cs)

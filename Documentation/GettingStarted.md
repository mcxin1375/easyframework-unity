# 快速开始

从 UPM 安装开始，完成初始化、资源、窗口、Controller 与发布。示例与当前源码接口对应；资源和 Player 流程需要在你的 Unity 工程中配置并验证。

## 1. 安装与导入示例

Unity Package Manager 中选择 Add package from git URL：

```text
https://github.com/mcxin1375/easyframework-unity.git
```

包名为 `com.cookie.easyframework`，当前版本为 `1.0.0`。package.json 自动声明 Newtonsoft.Json 与 Scriptable Build Pipeline 依赖。版本变更见 [更新日志](../CHANGELOG.md)。

当前 `EasyFramework/Runtime/EasyFramework.asmdef` 还无条件引用 TextMesh Pro、Universal RP、HybridCLR 程序集，Profiler 也引用 URP。因此纯净工程仅添加 Git URL 可能不能编译。先在目标 Unity 的 Package Manager 安装兼容版本的以下包，再导入框架：

- Unity UI：com.unity.ugui。
- TextMesh Pro：com.unity.textmeshpro。
- Universal RP：com.unity.render-pipelines.universal。
- HybridCLR：通过 Git URL https://github.com/focus-creative-games/hybridclr_unity.git 安装，并按其要求配置。

不使用这些集成的项目，需要在项目维护的包中拆分相应 asmdef 引用；当前包还没有完全隔离可选依赖。不要只删除 GUID 而忽略实际代码引用。业务 asmdef 引用 EasyFramework；Editor 工具单独引用 EasyFramework.Editor。Profiler 示例另外引用 EasyFramework.Profiler。

在 `Package Manager > Easy Framework > Samples` 导入唯一的 **EasyFramework Samples** 条目，源码位于 [Samples~](../Samples~)。Unity 将其复制到 `Assets/Samples/<框架显示名>/<框架版本>/EasyFramework Samples`。所有简短示例类直接放在 `Runtime` 和 `Editor` 两个目录，必要前提、调用和释放方式写在代码注释中，没有独立说明 Markdown 或运行场景。导入本身不会自动执行业务；资源、URL 和构建配置由接入工程提供。源码索引见 [示例](Examples.md)。

## 2. 资源目录与请求

在 Project Settings > EasyFramework Tools > AssetBundleBuilder 配置 Build Directories，默认 Assets/Res_DLC。将 Cube.prefab 保存到该目录，再由业务显式调用示例中的 `await ResourceExample.RunAsync("Cube", parent)`。

普通文件单独打包，以 .ab 结尾的目录整体打包；跳过 Editor、.meta 和配置的过滤项。多个 IAssetBundleBuilderSettings 自动合并配置。业务传入资源名 Cube，无须拼接 .ab、哈希名或下载路径。

Editor 默认 resLoaderEditorMode = true，使用编辑器桥接，可不打包运行资源示例。Player 前必须构建资源并准备 DLC。

## 3. 等待初始化

Unity 的 void 入口调用返回 ETask 的异步方法：

```csharp
using EasyFramework;
using UnityEngine;

public sealed class Bootstrap : MonoBehaviour
{
    private void Start() => StartGameAsync().Forget();

    private async ETask StartGameAsync()
    {
        await F.InitializeAsync();
        if (this == null) return;
        await GameController.Instance.EnterAsync();
    }
}

public sealed class GameController : Controller<GameController>
{
    protected override void OnEnter() => Debug.Log("Game entered");
}
```

即使开启 autoInitialize，也先等待 F.InitializeAsync()。业务通过 GameController.Instance.EnterAsync(...) 进入，通过 Instance.ExitAsync() 退出；基类入口将调用转发到 F.ControllerManager，状态和参数写入时机由管理器统一处理。退出后保留实例缓存，后续进入复用；不提供公开 DestroyAsync。OnAddListeners / OnRemoveListeners 及监听去重在 Controller 内部实现。

资源加载和清理的最小调用见 [Runtime/ResourceExample.cs](../Samples~/Runtime/ResourceExample.cs)：`RunAsync(string resourceName = "Cube", Transform parent = null)` 先等待初始化，通过局部 `IResRequest` 创建实例，并在 `finally` 中销毁实例、令请求不再存活，再以同一个请求调用 `Unload`。该模板展示一次完整使用范围；跨帧持有时，业务自行保存请求和实例，退出后释放，并在异步完成后检查自身生命周期。

## 4. 定义资源窗口

正常业务窗口按类型名加载资源：

```csharp
using EasyFramework;
using UnityEngine;

public sealed class HelloWindow : Window
{
    protected override void OnOpen() => Debug.Log("HelloWindow opened");
    protected override void OnClose() => Debug.Log("HelloWindow closed");
}
```

准备以 RectTransform 为根的 HelloWindow.prefab，放入构建目录。初始化后调用 `await F.WindowManager.OpenAsync<HelloWindow>(UILayer.HUD)`。未指定 UI Root 时使用包内 Resources/UIRoot。自定义 Root 要配置层、Camera、EventSystem。

暂时隐藏用 Close<HelloWindow>()，释放实例先关闭再 Destroy<HelloWindow>()。继承 WindowResources 并重写 `ResourcePath` 可指定 `Resources` 下的资源路径。[Runtime/WindowExample.cs](../Samples~/Runtime/WindowExample.cs) 提供 `OpenAsync()`、`Close()` 和 `Release()` 模板；需先准备以 RectTransform 为根的 `TemplateWindow.prefab`，顺序等待打开后再关闭或释放。

## 5. 应用配置与发布

1. 通过 Create > EasyFramework > AppSettings 创建配置，设置公司、产品、App Name、包标识、版本、Build Index、DLC URL 和 App Version URL。
2. 在 Project Settings > EasyFramework 的 App Settings 字段引用配置。
3. 确认当前 Build Target 和 Build Settings 中启用的启动场景。
4. DLC_StreamingAssets 模式下，在 DLCBuilder 开启 copyVersionToStreamingAssets。releaseVersion 留空使用最新版本，也可手动执行 DLCBuilder - CopyVersionToStreamingAssets。
5. 执行 EasyFramework/Tools/PlayerBuilder - Execute。

```csharp
using EasyFramework.Editor;

public static class GameBuild
{
    public static void Execute()
    {
        PlayerBuilder.Instance.Execute();
    }
}
```

PlayerBuilder.assetBuilder 默认 true，会先触发 AssetBuilder，再构建 Player；preSettingsEnabled 默认 true，会同步 AppSettings。AssetBuilder 按顺序触发 HybridCLRBuilder（启用时）、AssetBundleBuilder、DLCBuilder。

如果单独执行 `AssetBuilder.Instance.Execute()`，随后还要运行 PlayerBuilder，应先关闭 PlayerBuilder.assetBuilder，避免二次打包。StreamingAssets 复制在 DLCBuilder 中，默认关闭；只运行 PlayerBuilder 而未开启复制，不能保证资源进入底包。

## 当前边界

DLC 独立分包下载暂不可用。HybridCLR 是可选集成，需要安装插件、配置 EF_HYBRIDCLR 对应环境，并验证程序集构建、下载与加载。

当前没有 F.WorldManager，ECS 的包外入口尚不完整。Controller 支持管理器帧分发和 `ExitAllAsync()`；入口与异常处理见 [Controller 文档](Runtime/ControllerManager.md)。

文档核对不能替代目标 Unity 场景运行和平台构建。包内 Profiler 的唯一 Manager 入口、启动配置、模块开关、缓存点击遮罩及计数器边界见 [Profiler](Runtime/Profiler.md)；自动启动遵循已保存的 autoInitialize 设置。

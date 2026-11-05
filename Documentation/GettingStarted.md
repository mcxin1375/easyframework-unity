# 快速开始

从 UPM 安装开始，完成初始化、资源、窗口、Controller 与发布。示例与当前源码接口对应；资源和 Player 流程需要在你的 Unity 工程中配置并验证。

## 1. 安装与导入示例

Unity Package Manager 中选择 Add package from git URL：

```text
https://github.com/mcxin1375/easyframework-unity.git
```

包名 com.cookie.easyframework，当前版本 0.1.1。package.json 未声明最低 Unity 版本，自动声明的依赖只有 Newtonsoft.Json 与 Scriptable Build Pipeline。

当前 Runtime/EasyFramework.asmdef 还无条件引用 TextMesh Pro、Universal RP、HybridCLR 程序集，Profiler 也引用 URP。因此纯净工程仅添加 Git URL 可能不能编译。先在目标 Unity 的 Package Manager 安装兼容版本的以下包，再导入框架：

- Unity UI：com.unity.ugui。
- TextMesh Pro：com.unity.textmeshpro。
- Universal RP：com.unity.render-pipelines.universal。
- HybridCLR：通过 Git URL https://github.com/focus-creative-games/hybridclr_unity.git 安装，并按其要求配置。

不使用这些集成的项目，需要在项目维护的包中拆分相应 asmdef 引用；当前包还没有完全隔离可选依赖。不要只删除 GUID 而忽略实际代码引用。业务 asmdef 引用 EasyFramework，UI 示例还引用 UnityEngine.UI；Editor 工具单独引用 EasyFramework.Editor。

在 Package Manager 的 Samples 中导入“基础示例”。新建空场景，在 GameObject 上挂载 EventExample、ControllerExample 或 WindowExample，每种一份。事件与 Controller 在 Console 输出日志，窗口示例无需预制体便能显示半透明绿色面板。完整源文件见 [基础示例](Examples.md)。

## 2. 资源目录与请求

在 Project Settings > EasyFramework Tools > AssetBundleBuilder 配置 Build Directories，默认 Assets/Res_DLC。将 Cube.prefab 保存到该目录，再挂载示例中的 ResourceExample。

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

资源组件使用 `await F.ResLoader.CreateObjAsync("Cube", request: this)`，并实现 IResRequest.Alive；清理时销毁实例并调用 `F.ResLoader.Unload("Cube", this)`。使用同一个请求持有与释放，处理异步完成前组件已销毁的情况。完整处理见 ResourceExample，而不是直接丢弃加载结果。

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

暂时隐藏用 Close<HelloWindow>()，释放实例先关闭再 Destroy<HelloWindow>()。继承 WindowResources 并设置 WindowResourcesPath 可改用 Resources 路径。无需资源的窗口创建示例见 WindowExample.cs。

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

当前没有 F.WorldManager，ECS 的包外入口尚不完整；Controller 的帧分发与 ExitAllAsync 有实现问题，基础示例不依赖这些路径，见 [Controller 文档](Runtime/ControllerManager.md)。

文档核对不能替代目标 Unity 场景运行和平台构建。包内 Profiler 的唯一 Manager 入口、启动配置、模块开关、缓存点击遮罩及计数器边界见 [Profiler](Runtime/Profiler.md)；自动启动遵循已保存的 autoInitialize 设置。

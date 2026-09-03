# PlayerBuilder

`PlayerBuilder` 是 Unity Player 发布构建工具。它继承 `ToolBase<PlayerBuilder>`，通过 `IPlayerBuilderSettings` 获取 `BuildPlayerOptions`，调用 Unity `BuildPipeline.BuildPlayer` 完成目标平台构建，并通过 `IPlayerBuilderExtension` 处理构建报告。

## 菜单

| 菜单 | 作用 |
| --- | --- |
| `EasyFramework/Tools/PlayerBuilder - Execute` | 执行 Player 构建 |
| `EasyFramework/Tools/PlayerBuilder - Build DLC_StreamingAssets` | 将指定 DLC 版本复制到 `StreamingAssets/DLC` |

## 配置入口

配置位于 `Project Settings > EasyFramework Tools > PlayerBuilder`，对应 `PlayerBuilderSettings`：

| 字段 | 说明 |
| --- | --- |
| `preSettingsEnabled` | 是否在构建前将 `AppSettings` 同步到 Unity `PlayerSettings`，默认关闭 |
| `releaseVersion` | 发布时使用的 DLC 版本；留空时使用 DLCBuilder 最近一次构建的版本 |
| `enabled` | 是否执行 Player 构建，默认开启 |
| `exportAsGoogleAndroidProject` | Android 是否导出 Gradle 工程 |
| `developmentBuild` | 是否添加 Unity `BuildOptions.Development` |
| `cleanupTempDir` | 构建完成后是否清理 `*_BurstDebugInformation_DoNotShip` 目录，默认开启 |

应用发布信息在 `AppSettings` 中配置。启用 `preSettingsEnabled` 后，PlayerBuilder 会将以下内容同步到 Unity：

- `CompanyName` -> `PlayerSettings.companyName`
- `ProductName` -> `PlayerSettings.productName`
- `BundleVersion` -> `PlayerSettings.bundleVersion`
- `BundleIdentifier` -> 当前 Build Target 对应的应用标识
- Android 的 `BuildIndex` -> `PlayerSettings.Android.bundleVersionCode`

在 Project 窗口右键选择 `Create > EasyFramework > AppSettings` 创建应用配置，并在 `Project Settings > EasyFramework` 的 `App Settings` 字段中引用该对象。

## 构建流程

执行 `PlayerBuilder.Instance.Execute()` 或菜单中的 `PlayerBuilder - Execute` 时，流程如下：

1. 刷新并发现 `PlayerBuilder` 的工具扩展和构建选项提供者。
2. `PlayerBuilderPreExtension.OnExecuteBefore` 根据 `EasyFrameworkSettings.Instance.resLoaderMode` 准备 `StreamingAssets/DLC`。
3. 当 `preSettingsEnabled` 开启时，`PlayerBuilderPreExtension` 将 `AppSettings` 同步到 Unity `PlayerSettings`。
4. `BuildPlayerOptionsSettings` 读取 Build Settings 中启用的场景和当前 `EditorUserBuildSettings.activeBuildTarget`，生成 `BuildPlayerOptions`。
5. `PlayerBuilderUtility` 调用 `BuildPipeline.BuildPlayer`。
6. 构建完成后按 `Order` 调用所有 `IPlayerBuilderExtension.OnBuildReport`。
7. Unity 的 PostProcessBuild 阶段按配置清理临时目录。

如果 `PlayerBuilderSettings.enabled` 为 `false`，构建流程在调用 `BuildPipeline.BuildPlayer` 前结束。若没有发现有效的 `IPlayerBuilderSettings`，工具会记录 `BuildPlayerOptionsSettings is null` 错误并结束。

## DLC StreamingAssets

当 `EasyFrameworkSettings.Instance.resLoaderMode` 为 `DLC_StreamingAssets` 时，PlayerBuilder 的前置扩展会清空 `EasyFrameworkSettings.Instance.StreamingAssetsDLCPath`，并将 `releaseVersion` 对应的 DLC 版本目录复制到该路径。

`releaseVersion` 为空时，使用 `DLCBuilder` 最近一次构建的版本。指定版本目录不存在时会记录错误，不会继续复制。单独执行 `Build DLC_StreamingAssets` 菜单可以在不构建 Player 的情况下执行这一步。

使用该流程前，应先完成 AssetBundle、HybridCLR（启用时）和 DLC 构建，确保目标版本已经存在。

## BuildPlayerOptions

内置 `BuildPlayerOptionsSettings` 会：

- 使用 Build Settings 中所有启用的场景。
- 使用当前 `EditorUserBuildSettings.activeBuildTarget`。
- 使用 `BundleIdentifier_BundleVersion` 作为基础输出名。
- Android 默认输出 APK，并将 Android `bundleVersionCode` 加入文件名。
- Windows 和 Windows 64 位输出到独立目录，并生成对应的 `.exe` 文件。
- Android 开启 `exportAsGoogleAndroidProject` 时输出 Gradle 工程目录。
- 根据 `developmentBuild` 设置 `BuildOptions.Development`。

默认输出根目录为 `PlayerBuilder.ProjectPlatformPath`。项目可以提供自定义 `IPlayerBuilderSettings`，但当前构建实现使用按 `Order` 排序后的第一个设置对象，因此应确保最终只有一个有效的 `BuildPlayerOptions` 提供者。

## 扩展接口

### IPlayerBuilderSettings

用于提供 Player 构建参数：

```csharp
using UnityEditor;
using EasyFramework.Editor;

internal sealed class GamePlayerBuilderSettings : IPlayerBuilderSettings
{
    public int Order => 100;

    public BuildPlayerOptions BuildPlayerOptions => new BuildPlayerOptions
    {
        scenes = new[] { "Assets/Scenes/Main.unity" },
        target = EditorUserBuildSettings.activeBuildTarget,
        locationPathName = "Build/Game"
    };
}
```

### IPlayerBuilderExtension

用于在构建完成后处理 `BuildReport`：

```csharp
using UnityEditor.Build.Reporting;
using EasyFramework.Editor;

internal sealed class GamePlayerBuildExtension : IPlayerBuilderExtension
{
    public int Order => 100;

    public void OnBuildReport(BuildReport report)
    {
        // 根据 report.summary.result 生成发布记录或处理构建产物。
    }
}
```

### IToolEvent<PlayerBuilder>

需要在 Player 构建前或工具生命周期中执行逻辑时，实现 `IToolEvent<PlayerBuilder>`。内置 `PlayerBuilderPreExtension` 就使用该接口完成 DLC 复制和 PlayerSettings 同步。

```csharp
using EasyFramework.Editor;

internal sealed class GamePlayerPreBuildExtension : IToolEvent<PlayerBuilder>
{
    public int Order => 100;

    public void OnExecuteBefore()
    {
        // 构建前准备。
    }

    public void OnExecuteAfter()
    {
        // 工具执行后的处理。
    }
}
```

扩展由工具系统自动发现，并按 `Order` 升序执行。扩展应避免修改其他工具的输出约定，尤其不要在 PlayerBuilder 开始后改变当前 Build Target。

## 构建后清理

`PlayerBuilderPostProcessor` 注册 Unity `PostProcessBuild` 回调。当 `cleanupTempDir` 开启时，会在 Player 输出目录下删除以产品名命名的 `*_BurstDebugInformation_DoNotShip` 目录。该清理只针对构建后目录，不会删除 Player 主输出文件。

源码：[PlayerBuilder.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilder.cs)、[BuildPlayerOptionsSettings.cs](../../Editor/Tools/PlayerBuilder/BuildPlayerOptionsSettings.cs)、[PlayerBuilderSettings.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderSettings.cs)、[PlayerBuilderUtility.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderUtility.cs)、[PlayerBuilderPreExtension.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderPreExtension.cs)、[PlayerBuilderPostProcessor.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderPostProcessor.cs)。

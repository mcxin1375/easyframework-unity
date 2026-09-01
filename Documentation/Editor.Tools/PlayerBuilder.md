# PlayerBuilder

`PlayerBuilder` 是 Unity Player 发布构建工具。它读取 `IPlayerBuilderSettings` 提供的 `BuildPlayerOptions`，调用 `BuildPipeline.BuildPlayer` 完成指定平台的 Player 构建，并通过 `IPlayerBuilderExtension` 处理构建报告。

## 菜单

| 菜单 | 作用 |
| --- | --- |
| `EasyFramework/Tools/PlayerBuilder - Execute` | 执行 Player 构建 |
| `EasyFramework/Tools/PlayerBuilder - BuildDLCList` | 把指定 DLC 版本内容复制到 StreamingAssets |

## 配置

配置位于 `Project Settings > EasyFramework Tools > PlayerBuilder`：

| 字段 | 说明 |
| --- | --- |
| `companyName` / `productName` | 发布前写入 Unity PlayerSettings |
| `preSettingsEnabled` | 是否执行发布前设置同步 |
| `streamingAssetsOptions` | StreamingAssets 处理方式，当前支持 `None`、`DLCList` |
| `dlcVersion` | `DLCList` 模式使用的 DLC 版本，留空时使用最新版本 |
| `enabled` | 是否执行 Player 构建 |
| `exportAsGoogleAndroidProject` | Android 是否导出 Gradle 工程 |
| `developmentBuild` | 是否添加 Unity Development Build 选项 |
| `cleanupTempDir` | 是否清理标记为不发布的临时目录 |

框架设置中的应用名、Bundle 版本、Bundle Identifier 会在启用前置设置时同步到 Unity `PlayerSettings`。Android 的 `bundleVersionCode` 优先使用框架应用设置的构建索引，否则使用 `PlayerBuilder.Version.buildIndex`，并保证不小于 `1`。

## BuildPlayerOptions

内置 `BuildPlayerOptionsSettings` 实现 `IPlayerBuilderSettings`：

- 使用 Build Settings 中启用的场景。
- 使用当前 `EditorUserBuildSettings.activeBuildTarget`。
- 根据平台生成输出文件名和目录。
- Android 可生成 APK 或 Google Android 工程。
- 结合 `developmentBuild` 设置 `BuildOptions.Development`。

项目可以提供自己的 `IPlayerBuilderSettings`。当前构建实现使用发现结果中的第一个配置，因此项目应确保最终只有一个有效的 Player 选项提供者，或明确它们的 `Order` 和注册策略。

## 构建扩展

```csharp
using UnityEditor.Build.Reporting;
using EasyFramework.Editor;

internal sealed class GamePlayerBuildExtension : IPlayerBuilderExtension
{
    public int Order => 100;

    public void OnBuildReport(BuildReport report)
    {
        // 根据 report.summary.result 生成发布记录或上传构建产物。
    }
}
```

`PlayerBuilderUtility.BuildBySettings` 会在构建完成后按顺序调用所有 `IPlayerBuilderExtension.OnBuildReport`，然后打开 Player 输出目录。HybridCLR 集成还可以通过 `IToolEvent<PlayerBuilder>` 在 Player 构建前执行 HybridCLR 的准备工作。

源码：[PlayerBuilder.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilder.cs)、[BuildPlayerOptionsSettings.cs](../../Editor/Tools/PlayerBuilder/BuildPlayerOptionsSettings.cs)、[PlayerBuilderSettings.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderSettings.cs)、[PlayerBuilderUtility.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderUtility.cs)、[StreamingAssetsExtensions.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderExtensions/StreamingAssetsExtensions.cs)。

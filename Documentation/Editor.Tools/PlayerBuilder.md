# PlayerBuilder

PlayerBuilder 继承 ToolBase<PlayerBuilder>，使用 IPlayerBuilderSettings 提供的 BuildPlayerOptions 调用 Unity BuildPipeline.BuildPlayer，通过 IPlayerBuilderExtension 处理构建报告。

## 菜单与配置

菜单：`EasyFramework/Tools/PlayerBuilder - Execute`。
配置：`Project Settings > EasyFramework Tools > PlayerBuilder`。

| 字段 | 默认值与作用 |
| --- | --- |
| `enabled` | true，是否执行 Player 构建主体 |
| `assetBuilder` | true，前置阶段先执行 AssetBuilder |
| `preSettingsEnabled` | true，将 AppSettings 同步到 Unity PlayerSettings |
| `exportAsGoogleAndroidProject` | false，Android 是否导出 Gradle 工程 |
| `developmentBuild` | false，是否加入 Development 选项 |
| `cleanupTempDir` | true，构建后清理 *_BurstDebugInformation_DoNotShip |

## 推荐发布方式

默认配置下执行一次 PlayerBuilder 即会在前置阶段构建资源：

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

需要分别执行资源和 Player 构建时，先关闭 PlayerBuilder.assetBuilder，再依次执行 AssetBuilder.Instance.Execute() 和 PlayerBuilder.Instance.Execute()，避免重复升级资源版本和打包。

AppSettings 通过 `Create > EasyFramework > AppSettings` 创建，并在 EasyFramework 的 App Settings 字段引用。启用 preSettingsEnabled 时同步公司名、产品名、版本与包标识；Android BuildIndex 不大于零时回退到 PlayerBuilder 工具版本，并保证至少为 1。

## 构建流程

1. 刷新工具扩展。
2. PlayerBuilderPreExtension.OnExecuteBefore 按 assetBuilder 配置执行 AssetBuilder。
3. 升级 PlayerBuilder 工具版本。
4. 按 Order 执行配置同步与 Player 构建。
5. 使用启用的 Build Settings 场景和当前 Build Target 生成输出。
6. 调用 OnBuildReport 扩展；PostProcessBuild 按配置清理临时目录。

enabled 仅控制 Player 构建主体，前置资源构建仍可能执行。不要用 enabled 代替 assetBuilder 关闭资源构建。

## StreamingAssets

当前 PlayerBuilder 没有 releaseVersion 字段或 Build DLC_StreamingAssets 菜单，前置扩展也不负责复制 DLC。该流程已移至 [DLCBuilder](DLCBuilder.md)。

使用 DLC_StreamingAssets 时，应启用 DLCBuilder.copyVersionToStreamingAssets，或先执行 DLCBuilder - CopyVersionToStreamingAssets。确认复制完成后再发布 Player，详见 [快速开始](../GettingStarted.md)。

源码：[PlayerBuilder.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilder.cs)、[PlayerBuilderSettings.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderSettings.cs)、[PlayerBuilderPreExtension.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderPreExtension.cs)、[BuildPlayerOptionsSettings.cs](../../Editor/Tools/PlayerBuilder/BuildPlayerOptionsSettings.cs)、[PlayerBuilderUtility.cs](../../Editor/Tools/PlayerBuilder/PlayerBuilderUtility.cs)。

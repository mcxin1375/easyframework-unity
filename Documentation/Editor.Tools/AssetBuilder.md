# AssetBuilder

`AssetBuilder` 是资源构建总入口，用于统一触发参与资源发布的工具。它本身不实现具体打包逻辑，也没有自己的 `OnSelfExecute` 工作；资源构建由 `IToolEvent<AssetBuilder>` 扩展完成。

## 菜单

`EasyFramework/Tools/AssetBuilder - Execute`

## 触发链

调用 `AssetBuilder.Execute()` 后，工具会发现并按 `Order` 执行所有 `IToolEvent<AssetBuilder>`：

```text
AssetBuilder
  -> HybridCLRBuilder（启用 EF_HYBRIDCLR 时）
  -> AssetBundleBuilder
  -> DLCBuilder
```

当前内置工具的顺序由 `ToolOrder` 控制。项目还可以注册自己的 `IToolEvent<AssetBuilder>`，把资源生成或其他构建步骤接入同一条链路。

需要注意：被 `AssetBuilder` 触发的工具执行的是事件阶段，不会重新进入各自完整的 `ToolBase.Execute()`，因此不会自动执行它们自己的 `Refresh`、版本升级和 AssetDatabase 刷新。需要版本记录或额外准备逻辑时，应在扩展生命周期中显式处理。

## 自定义构建步骤

```csharp
using EasyFramework.Editor;

internal sealed class GameAssetBuildExtension : IToolEvent<AssetBuilder>
{
    public int Order => 1000;

    public void OnExecuteBefore()
    {
        // 构建前准备。
    }

    public void OnExecute()
    {
        // 执行项目自己的资源构建。
    }

    public void OnExecuteAfter()
    {
        // 构建后校验或清理。
    }
}
```

扩展中应使用稳定的输出目录和明确的失败日志，并确保后续 `DLCBuilder` 需要的文件已经写入对应平台目录。

源码：[AssetBuilder.cs](../../Editor/Tools/AssetBuilder/AssetBuilder.cs)、[ToolOrder.cs](../../Editor/Tools/ToolOrder.cs)。

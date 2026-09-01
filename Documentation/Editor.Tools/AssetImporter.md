# AssetImporter

`AssetImporter` 是资源目录导入和同步工具。当前工具类负责提供标准菜单入口和生命周期，具体导入动作通过 `IToolEvent<AssetImporter>` 扩展实现。

## 菜单

`EasyFramework/Tools/AssetImporter - Execute`

## 执行流程

直接执行时，工具按 [ToolBase](ToolBase.md) 的标准生命周期刷新扩展，然后依次调用导入扩展的 `OnExecuteBefore`、`OnExecute` 和 `OnExecuteAfter`。

工具本身没有内置的目录扫描实现。资源导入规则应封装在扩展中，这样项目可以根据资源来源增加不同的同步策略。

## 内置配置对象

`ResImporterSettings` 是一个 `ToolScriptableObject<AssetImporter>`，可通过以下菜单创建：

`Create > EasyFramework > AssetImporter > ResImporterSettings`

每条 `ResImporterConfig` 包含：

| 字段 | 说明 |
| --- | --- |
| `from` | 源目录 |
| `to` | 目标目录 |
| `deleteDiff` | 同步时是否删除目标中源目录不存在的内容，默认开启 |

执行时会复制目录内容，并清理目标目录中不再存在的资源元数据。路径应使用项目可识别的路径格式，并在执行前确认源目录存在、目标目录可写。

## 自定义扩展

```csharp
using EasyFramework.Editor;

internal sealed class GameAssetImportExtension : IToolEvent<AssetImporter>
{
    public int Order => 100;

    public void OnExecute()
    {
        // 执行项目自定义导入或转换逻辑。
    }
}
```

多个扩展按 `Order` 升序执行。需要共享配置时可以使用 `ToolScriptableObject<AssetImporter>`，而不是把项目路径硬编码到工具类中。

源码：[AssetImporter.cs](../../Editor/Tools/AssetImporter/AssetImporter.cs)、[ResImporterSettings.cs](../../Editor/Tools/AssetImporter/ScriptableObject/ResImporterSettings.cs)。

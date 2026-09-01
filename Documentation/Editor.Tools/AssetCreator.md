# AssetCreator

`AssetCreator` 是资源生成流程的统一 Editor 入口。它当前不包含具体的资源生成算法，项目通过 `IToolEvent<AssetCreator>` 扩展接入图集、配置、表格或其他派生资源的生成逻辑。

## 菜单

`EasyFramework/Tools/AssetCreator - Execute`

## 执行模型

工具继承 `ToolBase<AssetCreator>`，因此执行时会：

1. 刷新 `IToolEvent<AssetCreator>` 扩展。
2. 按 `Order` 执行扩展的前置、执行和后置阶段。
3. 升级 `AssetCreator` 的工具版本。
4. 刷新 Unity AssetDatabase。

## 自定义资源生成

```csharp
using EasyFramework.Editor;

internal sealed class GameAssetCreator : IToolEvent<AssetCreator>
{
    public int Order => 100;

    public void OnExecuteBefore()
    {
        // 清理或准备生成目录。
    }

    public void OnExecute()
    {
        // 生成项目资源，并通过 AssetDatabase 保存。
    }

    public void OnExecuteAfter()
    {
        // 写入索引或执行生成后的校验。
    }
}
```

扩展应保证重复执行结果稳定，并在写入文件后依赖工具末尾的 AssetDatabase 刷新让 Unity 重新导入。需要可视化配置时，可继承 `ToolScriptableObject<AssetCreator>` 并提供 `enabled` 与 `order`。

源码：[AssetCreator.cs](../../Editor/Tools/AssetCreator/AssetCreator.cs)、[ToolScriptableObject.cs](../../Editor/Tools/ToolScriptableObject.cs)。

# ToolBase

`ToolBase<T>` 是 EasyFramework Editor 工具的统一基类。工具通过单例实例访问，并以 `ToolBase<T>.Execute()` 作为标准执行入口。`T` 必须是继承自 `ToolBase<T>` 的具体工具类型。

## 设计职责

- 提供工具单例、执行入口和执行顺序。
- 为每个工具按平台划分资源、工程、工具中间文件和调试文件目录。
- 统一刷新扩展、升级工具版本、执行扩展生命周期和刷新 Unity AssetDatabase。
- 通过 `IToolEvent<T>` 为具体工具提供可组合的扩展点。

## 接口

### ITool

```csharp
public interface ITool
{
    int Order { get; }
    ToolVersion Version { get; }
    void Refresh();
    void Execute();
}
```

`Order` 用于工具注册和显示排序，`Version` 保存工具执行版本，`Refresh` 刷新扩展缓存，`Execute` 执行工具。

### IToolExtension

```csharp
public interface IToolExtension
{
    int Order { get; }
}
```

所有工具扩展都可用 `Order` 控制执行顺序，默认值为 `0`。

### IToolEvent<T>

```csharp
public interface IToolEvent<T> : IToolExtension
    where T : ToolBase<T>, new()
{
    void OnExecuteBefore();
    void OnExecute();
    void OnExecuteAfter();
}
```

三个方法都是可选扩展点。工具自身也实现了 `IToolEvent<T>`：接口调用会转发到 `OnSelfExecuteBefore`、`OnSelfExecute` 和 `OnSelfExecuteAfter`。

## 执行顺序

调用 `Execute()` 时，流程固定为：

1. `Refresh()`，重新发现普通对象和 ScriptableObject 扩展。
2. 按 `Order` 顺序调用所有扩展的 `OnExecuteBefore()`。
3. 增加工具 `Version.buildIndex` 并保存版本文件。
4. 按 `Order` 顺序调用所有扩展的 `OnExecute()`。
5. 按 `Order` 顺序调用所有扩展的 `OnExecuteAfter()`。
6. 记录耗时并调用 `AssetDatabase.Refresh()`。

工具的实际工作应放在 `OnSelfExecute()`，或通过 `IToolEvent<T>` 注册扩展。直接执行某个工具时会升级该工具版本；当工具只是作为另一个工具的事件扩展被调用时，事件调用只进入其自身处理方法，不会再次执行完整的 `Execute()` 生命周期。

## 路径属性

路径由 `EasyFrameworkPreferences` 和当前平台生成，工具类型名会作为目录名的一部分：

| 属性 | 用途 |
| --- | --- |
| `AssetsPath` / `AssetsPlatformPath` | 工具需要写入 Unity `Assets` 的资源和平台目录 |
| `ProjectPath` / `ProjectPlatformPath` | 项目构建结果和版本化输出 |
| `ToolsPath` / `ToolsPlatformPath` | 工具中间文件和工具版本文件 |
| `DebugPath` / `DebugPlatformPath` | 调试输出 |
| `Version` | 当前工具的 `ToolVersion`，保存为工具平台目录下的 `Version.json` |

## 扩展发现

`ToolExtension<T>` 会通过反射发现两类扩展：

- 普通 C# 对象：创建类型实例；如果类型本身是工具，则使用其单例实例。
- `ScriptableObject`：查找项目中该类型的资源对象。

发现结果按 `Order` 升序缓存。`ToolScriptableObject<T>` 提供了 `enabled` 和 `order` 字段；禁用对象不会执行 `OnExecute`。

## 扩展示例

下面的扩展会在 `AssetCreator` 执行阶段生成项目资源。它应放在 Editor 程序集或 Editor 目录中：

```csharp
using EasyFramework.Editor;

internal sealed class GameAssetCreatorExtension : IToolEvent<AssetCreator>
{
    public int Order => 100;

    public void OnExecute()
    {
        // 执行项目自己的资源生成逻辑。
    }
}
```

需要可配置资产时，可以继承 `ToolScriptableObject<AssetCreator>`，通过 `CreateAssetMenu` 创建配置对象，再在 `OnExecute` 中读取字段执行逻辑。

## 使用建议

- 工具入口保持轻量，把可复用逻辑放到 `Utility` 或扩展中。
- 依赖其他工具输出时，优先接入 `IToolEvent<AssetBuilder>` 并明确 `Order`。
- 每次构建前确认 `EditorUserBuildSettings.activeBuildTarget`，不要在扩展中隐式切换平台。
- 需要重新扫描扩展时调用工具的 `Refresh()`；不要长期保存可能已经失效的扩展数组。

源码：[ToolBase.cs](../../Editor/Tools/ToolBase.cs)、[ITool.cs](../../Editor/Tools/ITool.cs)、[ToolExtension.cs](../../Editor/Tools/ToolExtension.cs)、[ToolExtensionObjectPool.cs](../../Editor/Tools/ToolExtensionObjectPool.cs)、[ToolScriptableObject.cs](../../Editor/Tools/ToolScriptableObject.cs)。

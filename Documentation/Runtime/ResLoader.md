# F.ResLoader

F.ResLoader 的静态类型是 IResLoader。Player 使用 AssetBundleLoader；Editor 默认启用 resLoaderEditorMode，使用编辑器资源桥接。调用前等待 F.InitializeAsync()。

AssetBundleLoader 初始化直接清空并填充内部 _abDict，不额外分配 bundles 字典，也不复制第二遍；全部条目成功后才设置 Manifest。Manifest 无效会向上传播异常，中途失败可能留下部分条目，下次初始化重新清空并填充，不承诺失败回滚。初始化状态及自动重试规则见 [F 总览](F.md)。

## 资源加载

| API | 说明 |
| --- | --- |
| `Load` / `LoadAsync` | 同步或异步加载 Bundle，可传 IResRequest |
| `LoadAsset<T>` / `LoadAssetAsync<T>` | 加载单个对象 |
| `LoadAllAssets<T>` / `LoadAllAssetsAsync<T>` | 加载包内全部指定类型对象 |
| `CreateObj` / `CreateObjAsync` | IResLoaderEx 扩展：加载 GameObject 并实例化 |
| `LoadScene` / `LoadSceneAsync` / `UnloadSceneAsync` | 场景资源加载与释放 |
| `IsLoading` / `IsUnloading` / `GetLoadingProgress` | 状态查询 |
| `GetAllDependencies` | 查询依赖 Bundle |

资源名来自构建 Manifest，不需要自行拼接 .ab 或哈希文件名。资源不存在时 CreateObjAsync 会记录错误并返回 null。

## 持有与释放

```csharp
using EasyFramework;
using UnityEngine;

public sealed class ResourceOwner : IResRequest
{
    public bool Alive { get; private set; } = true;
    private GameObject cube;

    public async ETask LoadAsync()
    {
        await F.InitializeAsync();
        cube = await F.ResLoader.CreateObjAsync("Cube", request: this);
    }

    public void Release()
    {
        Alive = false;
        if (cube != null) Object.Destroy(cube);
        cube = null;
        F.ResLoader.Unload("Cube", this);
    }
}
```

此片段展示配对关系，调用方应等待 LoadAsync 完成后再 Release；能处理加载中销毁的完整组件见 [基础示例](../Examples.md)。

长期使用资源时传入 Alive 为 true 的请求。无请求时仅按 resRequestAliveTime 保持缓存，每次加载刷新倒计时，不是无限持有或每次调用累加一个引用。释放时传相同请求；同一资源的其他活跃请求仍会阻止卸载。

请求的 Alive 变为 false 后，也可以等待加载器自动回收：缓存期限到期且全部持有者均不再存活时，后续 Tick 移除请求、释放 Bundle 引用并清空持有者集合。Window.Destroy 使用这个约定，只结束自身 Alive 持有，不主动调用 Unload；其他活跃持有者和剩余缓存期限仍然有效。

Unload 不负责 Destroy 已实例化的 GameObject。先停止使用并销毁实例，再释放请求。UnloadForce / UnloadAllForce 绕过持有关系，可选择卸载已加载对象，不应作为常规清理方式。

批量 CreateObjAsync(resName, count, ...) 在所有分支都等待实例化完成后返回；Editor Bundle 模式随后恢复 Shader。count 为 0 返回空数组且不加载资源，负数抛出 ArgumentOutOfRangeException；资源加载失败返回 null。

源码：[IResLoader.cs](../../EasyFramework/Runtime/Manager/ResLoader/IResLoader.cs)、[IResLoaderEx.cs](../../EasyFramework/Runtime/Manager/ResLoader/IResLoaderEx.cs)、[IResRequest.cs](../../EasyFramework/Runtime/Manager/ResLoader/IResRequest.cs)、[AssetBundleRequest.cs](../../EasyFramework/Runtime/Manager/ResLoader/AssetBundleLoader/AssetBundleRequest.cs)。

# F.ResLoader

`F.ResLoader` 的静态类型是 `IResLoader`。在 Player 中使用 AssetBundleLoader；在 Editor 中，当 `F.Settings.resLoaderEditorMode` 开启时可能使用编辑器资源桥接。因此资源名、Bundle 名和 Editor 行为要按项目配置确认。

## AssetBundle API

| API | 说明 |
| --- | --- |
| `Load` / `LoadAsync` | 同步或异步加载 AssetBundle，可传 `IResRequest` 管理持有关系 |
| `Unload` | 按请求释放引用 |
| `UnloadForce` | 强制释放指定 Bundle，可选择释放已加载对象 |
| `UnloadAllForce` | 强制释放全部 Bundle |
| `IsLoading` / `IsUnloading` | 查询状态 |
| `GetLoadingProgress` | 查询加载进度 |
| `GetAllDependencies` | 获取依赖 Bundle |

## 资源与场景 API

| API | 说明 |
| --- | --- |
| `LoadAsset<T>` / `LoadAssetAsync<T>` | 加载单个 Unity 对象 |
| `LoadAllAssets<T>` / `LoadAllAssetsAsync<T>` | 加载 Bundle 内全部指定类型对象 |
| `LoadScene` / `LoadSceneAsync` | 从资源系统加载场景 |
| `UnloadSceneAsync` | 卸载场景并释放对应资源请求 |

```csharp
using UnityEngine;

GameObject cube = await F.ResLoader.CreateObjAsync("Cube");
```

`CreateObjAsync` 是 `IResLoader` 的扩展方法，内部先加载 `GameObject` 再实例化。使用完资源要调用相应的 `Unload`；不要在 Bundle 释放后继续使用未独立持有的资源对象。

源码：[IResLoader.cs](../../Runtime/Manager/ResLoader/IResLoader.cs)、[IResLoaderEx.cs](../../Runtime/Manager/ResLoader/IResLoaderEx.cs)。

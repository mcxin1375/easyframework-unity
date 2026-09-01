# F.PoolManager

`F.PoolManager` 的类型是 `IPoolManager`，按资源名管理 GameObject 对象池。

| API | 说明 |
| --- | --- |
| `Rent(string resName, Transform parent)` | 从资源池租出对象，可指定父节点 |
| `Return(GameObject gameObject)` | 将对象归还对象池 |
| `CreatePool(string resName, int preLoadCount)` | 创建对象池并预加载数量 |
| `DestroyPool(string resName)` | 销毁指定对象池 |
| `DestroyAllPool()` | 销毁全部对象池 |

```csharp
using UnityEngine;

sealed class PoolExample
{
    public void Use(Transform parent)
    {
        GameObject item = F.PoolManager.Rent("Item", parent);
        if (item != null)
        {
            // 使用 item
            F.PoolManager.Return(item);
        }
    }
}
```

只有从池中租出的对象才能按池生命周期归还。归还后不要继续操作对象，也不要在业务侧重复 `Destroy`。

源码：[IPoolManager.cs](../../Runtime/Manager/PoolManager/IPoolManager.cs)。

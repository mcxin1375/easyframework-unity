# F.SceneLoader

`F.SceneLoader` 的类型是 `ISceneLoader`，管理场景加载、卸载、激活和状态查询。

| API | 说明 |
| --- | --- |
| `CurrentActiveScene` | 当前活动场景名称 |
| `LoadScene` / `LoadSceneAsync` | 加载场景，可指定 `LoadSceneMode` 和是否自动激活 |
| `UnloadSceneAsync` / `UnloadAllSceneAsync` | 卸载一个或全部已登记场景 |
| `GetLoadProgress` / `GetUnloadProgress` | 查询加载或卸载进度 |
| `IsLoaded` / `IsLoading` / `IsUnloading` | 查询场景状态 |
| `GetScene` | 获取 Unity `Scene` |
| `SetActive` | 设置活动场景 |
| `FindActiveSceneRootObj` | 在当前活动场景查找根对象 |
| `FindSceneRootObj` | 在指定场景查找根对象 |

```csharp
using UnityEngine.SceneManagement;

await F.SceneLoader.LoadSceneAsync("Main", LoadSceneMode.Additive, true);
F.SceneLoader.SetActive("Main");
```

场景名必须与资源和 Unity 场景配置一致。卸载场景后不要继续使用该场景中的对象引用。

源码：[ISceneLoader.cs](../../Runtime/Manager/SceneLoader/ISceneLoader.cs)。

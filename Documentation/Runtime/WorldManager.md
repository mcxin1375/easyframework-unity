# F.WorldManager

`F.WorldManager` 的类型是 `WorldManager`，负责管理多个 `FWorld`。它由 `F.Behaviour` 在 Update/LateUpdate 中驱动。

## 设计边界

EasyFramework 的 ECS 是引用类型、列表驱动的轻量实现，面向项目内的逻辑组织。它不是 archetype/chunk、Burst 或 Job System ECS，不提供数据导向 ECS 的内存布局和 Job 调度能力。

## 属性与方法

| API | 说明 |
| --- | --- |
| `MainWorld` | 获取主 World；首次访问时创建 |
| `WorldList` | 当前已加入管理的 World 只读列表 |
| `CreateWorld()` | 创建并排队一个 World，下一次 Update 时加入活动列表 |
| `Update()` / `LateUpdate()` | 更新或晚更新所有活动 World，通常由 `F.Behaviour` 调用 |
| `Destroy()` | 销毁所有活动及排队中的 World；销毁后不能再创建 |

```csharp
FWorld world = F.WorldManager.MainWorld;
var system = world.GetOrCreateSystem<ExampleSystem>();
FEntity entity = world.EntityManager.Create();

sealed class ExampleSystem : FSystem { }
```

业务应以源码当前签名为准。World 销毁是终态；创建、更新和注册都应发生在有效生命周期内。System 会按 `Order` 降序执行，同序时按类型全名确定顺序。

源码：[WorldManager.cs](../../Runtime/Core/ECS/WorldManager.cs)、[FWorld.cs](../../Runtime/Core/ECS/FWorld.cs)、[FEntityManager.cs](../../Runtime/Core/ECS/FEntityManager.cs)。

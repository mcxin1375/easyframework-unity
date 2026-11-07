# ECS（FWorld）

当前源码保留轻量 ECS 类型，但没有公开的 WorldManager 或 `F.WorldManager` 入口。保留此文档路径兼容已有链接，不再提供不可编译的管理器示例。

## 当前可用范围

| 类型或 API | 状态 |
| --- | --- |
| `FWorld` | 构造函数为 internal，包外业务不能直接 new |
| `FWorld.EntityManager` | 已有 World 的实体管理器 |
| `CreateSystem<T>()` | 为已有 World 创建或返回同类型 System |
| `CreateSystems(Assembly)` | 扫描程序集创建 System |
| `Update()` / `Destroy()` | 由持有 World 的集成层驱动和清理 |
| `FSystem` / `ISystem` | System 生命周期与排序 |
| `FEntityManager` / `FEntity` | 实体和组件组织 |

GetOrCreateSystem<T>() 和 GetSystem<T>() 当前被注释，不能作为可用 API。F.Behaviour 也不再内置 ECS 更新。

## 接入边界

在恢复公开 World 工厂或管理器前，不能仅靠旧文档从业务程序集启动完整 ECS 流程。不要使用反射绕过 internal 构造函数。项目可先使用 [Controller](ControllerManager.md)、[事件](Event.md) 和 [帧循环注册](FBehaviour.md) 组织业务。

已有框架内集成持有 World 时，System 按 Order 降序更新，同序按类型全名排序；Destroy 是终态。该实现基于引用对象和列表，不提供 Burst、Job System 或 chunk 存储。

源码：[FWorld.cs](../../EasyFramework/Runtime/Core/ECS/FWorld.cs)、[FSystem.cs](../../EasyFramework/Runtime/Core/ECS/FSystem.cs)、[FEntityManager.cs](../../EasyFramework/Runtime/Core/ECS/FEntityManager.cs)、[F.cs](../../EasyFramework/Runtime/F.cs)。

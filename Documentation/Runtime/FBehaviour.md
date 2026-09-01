# F.Behaviour

`F.Behaviour` 的类型是 `FBehaviour`，继承自 `SingletonBehaviour<FBehaviour>`。它是框架的 Unity 生命周期宿主，初始化时创建并通过 `DontDestroyOnLoad` 跨场景保留。

## 帧循环

`F.Behaviour.Update()` 每帧驱动：

- `F.WorldManager.Update()`
- `F.ControllerManager` 的内部 Update 分发

`F.Behaviour.LateUpdate()` 每帧驱动对应的 LateUpdate。业务代码不需要手动重复驱动这些模块。

## 销毁

宿主销毁时会销毁 `WorldManager` 和 `ControllerManager`。因此不要在 `OnDestroy` 之后继续创建 World、进入 Controller 或依赖框架帧循环。

`F.Behaviour` 是观察和生命周期入口，不是业务逻辑容器；业务启动应放在项目自己的启动状态中，并等待 `F.InitializeAsync()`。

源码：[F.cs](../../Runtime/F.cs)、[FBehaviour.cs](../../Runtime/FBehaviour.cs)。

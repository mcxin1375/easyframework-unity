# 源码审查记录

审查日期：2026-10-31。基准：5a6c4e4361d98b1c7541a4f76a64c20bb3f70ca1。范围为当前 Runtime、Editor 工具、Profiler、文档与示例的接口和生命周期；不是特定分支差异审查。以下问题仍存在于运行时代码，本次更新文档、示例与官网同步，没有改动 Runtime / Editor 实现。

## 发现的问题

### [P1] 批量退出跳过 Controller 退出回调

位置：[ControllerManager.cs:55](Runtime/Manager/ControllerManager/ControllerManager.cs#L55)、[Controller.cs:117](Runtime/Manager/ControllerManager/Controller.cs#L117)。

ExitAllAsync 在调用 ctr.ExitAsync() 前 RemoveAt；Controller.ExitAsync 的 IsEnter 检查因此为 false，直接返回。OnRemoveListeners、OnExit、OnExitAsync 与组件退出均被跳过，订阅和资源清理可能残留。ExitOtherBeforeEnter 也使用此路径。

建议：由 Controller.ExitAsync 自己移除栈元素，逆序退出时处理重入与异步栈变更。暂时复制 EnterList 后逐个 await ExitAsync；基础示例只使用实例退出。

### [P1] Controller 帧更新列表始终为空

位置：[ControllerManager.cs:18](Runtime/Manager/ControllerManager/ControllerManager.cs#L18)、[ControllerManager.cs:60](Runtime/Manager/ControllerManager/ControllerManager.cs#L60)。

_refresh 初始为 false，Enter / Remove 没有置为 true，OnUpdate 从不将 _enterList 复制到 _updateList。进入成功的 Controller 也收不到 OnUpdate / OnLateUpdate，依赖这些回调的业务停止更新。

建议：栈变更时刷新更新列表，并验证激活切换、回调内进入/退出及组件更新。基础示例不依赖该分发。

### [P1] 批量异步实例化可能在完成前读取 Result

位置：[IResLoaderEx.cs:69](Runtime/Manager/ResLoader/IResLoaderEx.cs#L69)。

Unity 2022.3+ 的 CreateObjAsync(resName, count, ...) 调用 Object.InstantiateAsync；只有 UNITY_EDITOR 且 resLoader 为 AssetBundleLoader 时才 await。Player 以及 Editor 资源桥接模式会立即读取 Result，无法保证已创建对象，也可能留下未返回给调用方的实例。

建议：所有分支先 await 操作，再对需要的 Editor 分支恢复 Shader。基础示例只用单实例重载。

### [P1] UPM 清单没有声明 asmdef 的全部依赖

位置：[EasyFramework.asmdef](Runtime/EasyFramework.asmdef)、[Profiler.asmdef](Module/Profiler/Runtime/EasyFramework.Profiler.asmdef)、[package.json](package.json)。

Runtime 无条件引用 TMP、HybridCLR.Runtime、URP 的 GUID，Profiler 同样无条件引用 URP；package.json 仅声明 Newtonsoft.Json 和 Scriptable Build Pipeline。缺少这些包的纯净工程可能因找不到程序集而无法编译。versionDefines 只控制代码宏，不会将 asmdef.references 变成可选引用。

GUID 已与本机 Unity 工程的 PackageCache 中的原始 asmdef.meta 对照。建议拆分可选集成程序集或明确声明需要的依赖；安装指南已明确现状，未通过加入强制依赖改变包行为。

### [P2] 初始化异常后后续等待无法完成

位置：[F.cs:64](Runtime/F.cs#L64)、[F.cs:76](Runtime/F.cs#L76)、[F.cs:83](Runtime/F.cs#L83)。

状态先置 Initializing，再等待两个加载器；若异常传播出来，状态没有失败分支或复位。后续 InitializeAsync 只能一直等待 Initialized。另一方面加载器也可能仅记录错误后返回，Initialized 不保证 Manifest 有效。

建议：增加明确的失败状态与错误传播，再定义是否允许重试；同时覆盖并发等待、失败与重复调用。文档已说明完成等待不等于资源一定有效。

## 文档与示例修正

- 删除失效的 F.Initialize、F.ControllerManager.EnterAsync<T>、F.WorldManager 示例，说明 Controller<T>.Instance 和 ECS 的实际边界。
- 修正 FBehaviour 路径、宿主创建和 IFrameworkTick 注册行为。
- 修正 PlayerBuilder 默认先构建资源、默认同步 AppSettings，以及 DLCBuilder 负责 StreamingAssets 复制的流程。
- 修正父工具触发子工具时仍执行完整 Execute、版本升级与扩展刷新的说明。
- 修正 IWindowUI<TUI> 的组件约束，标明 UI 与参数片段依赖 Roslyn 生成器。
- 新增 ETask、Profiler、统一快速开始、基础示例说明；package.json 注册 Samples~/Basics。
- 新增 5 个完整 C# 文件，覆盖资源持有与异步清理、程序创建窗口、事件解绑、Controller 进入退出和 Editor 发布入口。

## 验证与限制

- 5 个示例 C# 文件以 C# 9 编译，通过 Unity 2022.3.62f3 的引用程序集和现有 EasyFramework / Editor 编译程序集验证。没有修改正在打开的示例工程；这不是 Unity UPM 导入或场景运行测试。
- 对真实 ControllerManager.cs、Controller.cs、Singleton 部分与接口建立隔离 .NET 诊断：仅将 ETask 映射为 Task，并替换日志、宿主、反射等依赖。进入后两种帧回调计数均为 0；ExitAllAsync 后退出和解绑计数均为 0；逐个 ExitAsync 时退出与解绑均正常。此诊断验证状态逻辑，不验证 Unity 调度。
- 官网通过内容同步校验、TypeScript / Vite 生产构建及 8 项桌面/手机 Playwright 测试，覆盖全部文档、内链、搜索、复制、主题与导航。
- 未执行纯净 Unity 工程导入、Play Mode、HybridCLR 热更新或目标平台 Player 构建；以上源码问题需要后续修复和对应运行验证。

# 源码审查记录与优化进度

历史全量审查日期：2026-11-01（Asia/Shanghai）。原独立仓库审查提交：`d20f7911738d24180313b0aaca0a21df22d3e458`（`同步文档`）。原仓库执行 `git fetch origin` 后确认 `HEAD == origin/main`，当时工作区干净。此处记录的是历史审查条件，不代表当前 Unity 项目包的工作区状态。

## 当前修复进度

当前文件同步自 `D:\WorkGIT\easyframework-unity\REVIEW.md` 的完整 26 项报告；原独立仓库报告保持原样。修复目标为 `D:\WorkGIT\easyframework-projects\EasyFramework.Unity2022\Packages\easyframework-unity`。

历史基线共 **26 项：12 项 P1、14 项 P2**。当前 **4 项已修复、22 项待处理**；待处理为 **10 项 P1、12 项 P2**。已关闭 R01、R11、R16、R17；R14 的窗口容器已有本地部分修正，尚未完成生命周期验收，仍计入待处理。顺序 2–6 尚未执行。

## 顺序 1 执行记录（2026-11-01）

执行项目：`D:\WorkGIT\easyframework-projects\EasyFramework.Unity2022`；代码位于项目内的 `Packages/easyframework-unity`。本轮完成“保护用户数据和下载文件”工作包，对应后续审查方案中的 R01、R11、R16、R17；其他工作包尚未执行。下文保留 2026-11-01 全量审查的 26 项历史问题及证据，并逐项标明当前进度。

| 问题 | 本轮修正 | 验收结果 |
| --- | --- | --- |
| R01：DLC 清理边界 | 保留本地已有的 `DataPath → DLCPath` 修正；只清理合法清单下、DLC 根层已过期的 MD5 资源文件，保留元数据、业务文件、子目录和进行中的下载 | 根目录和业务哨兵文件保留；无效/缺失清单不清理；有效空清单可清理旧资源 |
| R11：下载失败残留半包 | HTTP 与 UnityWebRequest 使用同目录唯一临时文件，校验后替换目标；DLC 另校验清单长度和 MD5；失败、取消或替换失败保留旧文件并清理临时文件 | 中断、取消、HTTP 错误、长度/MD5 错误及提交失败均不覆盖旧目标；后续重试成功 |
| R16：版本切换后旧路径 | 移除资源路径缓存；新清单校验并成功保存后才提交版本和服务器地址，失败保留旧快照；使用 Newtonsoft 保留缺失/null 数组语义 | v1→v2 路径正确；新清单下载/解析/校验/保存失败均保旧并返回版本错误；请求中配置变化拒绝提交 |
| R17：未知资源被判存在 | `Exists` 校验当前清单、实际文件长度与 MD5；未知资源下载直接失败 | 未初始化、未知资源、残缺文件和同长度坏内容均为 false；不发无效网络请求 |

批量与按需下载共用同一文件请求和并发上限；不同资源名引用同一哈希文件时只下载一次。同文件名对应冲突长度的清单会被拒绝。公共 `DLCDownloader` 补充可选 MD5 参数，原有调用可继续使用。

首次验证：在当时已打开的 Unity **2022.3.62f3** 项目通过交互式 TestRunnerApi 执行真实 **Edit Mode 测试 67/67**，0 失败、0 跳过：新增 DLCUpdater 39 项、HTTP/文件提交 16 项、UnityWebRequest/DLCDownloader 6 项，以及原有 ETask/EVoid 6 项。编译使用当前项目的依赖和本地改动；没有执行 Player 构建或实际 CDN 更新，WebGL 分支仍需目标平台验证。

实现保持现有 `F.xxx` 入口与文件风格，仅抽取两条下载路径共用的文件校验/提交辅助。ETask 核心没有改动；并发消费者共享 `Task` 结果，避免多次消费同一个池化 ETask。当前项目已有的 WindowManager 改动、WindowContainer 删除及项目设置改动均保留。

DLCUpdater 的四个 Func 注入已整理为可读的 `protected virtual` 属性 `VersionUid`、`ServerUrl`，以及 `LoadTextAsync(url)`、`DownloadFileAsync(url, file)`；测试使用小型子类覆盖。整理后关闭原项目的 Editor，直接对同一原项目使用 Unity 2022.3.62f3 命令行 `-batchmode -nographics -runTests -testPlatform EditMode -testFilter EasyFramework.Tests` 重新执行测试，结果 **67/67 通过、0 失败、0 跳过**，进程已正常退出。此次使用当前项目本身，未创建临时 Unity 项目；结果独立于整理前的交互式 TestRunnerApi 记录。

本轮继续将下载文件辅助合并到已有 `FileHelper`，方法明确命名为 `CreateDownloadTempPath`、`VerifyFileIntegrity`、`ReplaceFileFromTemp`、`ExtractMD5FromFileName`，删除独立 `DownloadFileHelper.cs` 及其 `.meta`。`ExtractMD5FromFileName` 只提取文件名中的 MD5 标识，文件内容的 MD5 仍由 `MD5Helper.MD5File` 计算；保留临时下载、完整性校验、原子提交和失败保护行为。本轮合并后再次直接对同一原项目执行 Unity 命令行 Edit Mode 测试，结果 **67/67 通过、0 失败、0 跳过**；未创建临时 Unity 项目，也未启动交互式 Editor。

## 历史全量审查范围与结论

沿用上次 Review 的 `5a6c4e4361d98b1c7541a4f76a64c20bb3f70ca1` 为增量基准，差异命令为 `git diff 5a6c4e4361d98b1c7541a4f76a64c20bb3f70ca1...HEAD`；期间只有 `d20f791 同步文档` 一个提交。该提交修改文档、示例、AGENTS 和包的 Samples 注册，未修改 Runtime、Editor、Module 或 Tests 实现。

本次除检查增量外，重点复核当前 ETask / Ticker、事件、网络缓存、Controller、Window、初始化、资源下载、DLC、Editor 构建、UPM 依赖及 Profiler。历史审查记录 **26 项源码问题：12 项 P1、14 项 P2**。在该审查基线中，上次记录的 5 项问题均仍存在，对应 R06、R07、R08、R09、R13；其余 21 项为该次新识别的历史问题，不能归因于文档提交。当前修复状态以上节和各项标记为准。

P1 表示应优先修复的数据丢失、核心流程失效或发布可靠性问题；P2 表示需排期处理的条件性错误与生命周期问题。编号用于跟踪；Standards、Spec 两轴分别报告，后面的源码清单另按修复优先级组织。历史全量审查阶段仅更新报告；当前已完成顺序 1。下文保留原故障描述、诊断证据及验收标准，已修复项的历史症状不表示当前仍可复现。

## 历史审查：Standards

依据 [AGENTS.md](AGENTS.md)、[ToolBase.md](Documentation/Editor.Tools/ToolBase.md) 和相关模块文档核对最新差异。新示例遵循 `F.xxx` 访问、等待初始化、Editor 程序集隔离及标准工具入口要求。未发现可行动的规范违规，也未发现有充分证据、且值得独立整改的新增结构性异味。

**本轴：0 项。** 下文历史源码缺陷不计作本次增量的规范违规。

## 历史审查：Spec

仓库没有 `docs/agents/issue-tracker.md`，提交也没有原始 issue 引用，无法进行工单需求覆盖或 scope creep 判断。本次以公开文档与示例作为 API 契约；后续需要按工单审查时，可用 `/setup-matt-pocock-skills` 补齐 issue-tracker 工作流。

**S01：窗口销毁契约与实际行为不一致。** [WindowManager.md:48](Documentation/Runtime/WindowManager.md#L48) 宣称 Destroy 会“销毁窗口实例及其 GameObject”，第 50 行说明“需要彻底释放实例时销毁”。历史基线中，销毁后静态容器仍返回原实例，直接销毁已打开窗口还保留 `IsOpen=true`；新增 [WindowExample.cs:35](Samples~/Basics/WindowExample.cs#L35) 使用 Close→Destroy，也会留下延时清理队列项，随后再次调用 OnDestroy。问题见 R14。当前项目本地已将容器迁入 WindowManager，并在 Destroy 中移除类型条目；直接销毁、关闭队列和重复释放尚未验收，S01/R14 仍未关闭。

Controller 帧分发、批量退出、初始化失败和宿主生命周期等限制已经在当前文档中明确说明，因此不重复记为新增规格偏差。

**本轴：1 项 API 契约偏差，最严重项为 S01；映射到 R14，不额外计入 26 项源码问题。**

## 历史问题清单：P1（当前 2 项已修复、10 项待处理）

### R01 [P1] [已修复] DLC 清理误删应用数据根目录文件

位置：[当前清理实现 DLCUpdater.cs:142](Runtime/Manager/DLCManager/DLCUpdater.cs#L142)、[EasyFrameworkSettings.cs:99](Runtime/Base/ProjectSettings/EasyFrameworkSettings.cs#L99)。验证：真实源码隔离诊断。

`DeleteUnversionedFiles()` 应清理 DLC 缓存，却枚举 `DataPath` 根目录；该路径在 Editor 和大多数 Player 上为 `Application.persistentDataPath`，真正的缓存是其 `DLC` 子目录。初始化成功到达清理时，根目录下不在资源清单中的存档等文件会被删除，DLC 内过期 Bundle 反而保留。临时目录实测：根目录 `player.sav` 被删，`DLC/old.ab` 仍在，`Config` 子目录文件未被遍历。

优化：限定缓存清理在规范化后的 `DLCPath` 内，仅删除框架拥有的旧资源；保护版本元数据，校验清单后再清理。验收：根目录和其他业务目录的哨兵文件保留，当前版本资源和元数据保留，过期 DLC 资源删除。

当前状态：已修复。保留项目原有 `DataPath → DLCPath` 修正，只删除合法版本清单下、DLC 根层的过期 MD5 文件，保护元数据、业务文件、子目录、链接和在途下载；真实 Unity 回归通过，详见顺序 1 执行记录。

### R02 [P1] ETask 异常没有传播给 await 调用方

位置：[ETask.TaskAwaiter.cs:67](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L67)、[异常完成:111](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L111)、[泛型结果:158](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L158)。验证：直接编译真实 ETask 源码诊断。

异常完成只把状态改为 Faulted，没有保存异常；两种 GetResult 都不根据 Faulted 抛错。实测 `async ETask<int>` 内抛出 `InvalidOperationException` 后状态为 Faulted，调用方 GetResult 却返回 `0`；非泛型也正常返回。业务 await 后继续执行，外层 catch 无法恢复，池复用时泛型结果还缺少清理保证。

优化：保存 `ExceptionDispatchInfo`，在 GetResult 传播原异常；明确取消语义和未观察异常策略，回池时清空结果/异常。验收：泛型/非泛型在同步失败、挂起后失败、取消及 Forget 路径上均符合约定，业务 catch 能收到原异常。

### R03 [P1] ETask 完成与 continuation 注册竞争导致永久等待

位置：[ETask.TaskAwaiter.cs:42](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L42)、[完成逻辑:99](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L99)、[泛型完成:166](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L166)。验证：真实源码、可控交错诊断。

OnCompleted 预期完成方会设置 sentinel，但 TrySetResult / TrySetException 只调用已有 continuation，从不设置 sentinel。实测顺序 `IsCompleted=false → TrySetResult → OnCompleted(callback)`，callback 不执行。这对应后台任务完成与 await 注册之间的竞争，调用链可能永久挂起。

优化：以同一原子状态机交接注册和完成，结果/异常先于完成状态发布，保证 continuation 恰好执行一次。验收：人为控制“先完成后注册”和“先注册后完成”，成功、失败及取消都能唤醒，再补并发压力验证。

### R04 [P1] ETask 在首次消费前就回池

位置：[ETask.TaskAwaiter.cs:122](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L122)、[失效状态查询:35](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L35)、[ETask.StateMachine.cs:19](Runtime/Core/ETask/ETask.StateMachine.cs#L19)。验证：真实源码隔离诊断。

Ticker 移除已完成节点时无条件清空 Token 并回池，不等待首次 GetResult。实测创建 `ETask.DelayFrame(0)`，推进一个 tick，再首次读取结果，Status 显示 Succeeded，GetResult 却抛出 recycled `ArgumentException`。这是首次消费，文档的“不要重复 await”不能规避“先启动、稍后首次 await”的正常用法。

优化：分开调度节点移除与结果消费；结果在首次消费前保持有效，定义未消费任务的释放策略，并统一 token 校验。验收：完成后隔若干 tick 首次 await 仍成功，池复用不影响旧任务，重复消费按明确规则拒绝。

### R05 [P1] 一个 tick 回调异常持续阻塞后续节点

位置：[Ticker.cs:51](Runtime/Core/ETask/Ticker/Ticker.cs#L51)。验证：真实 Ticker 源码诊断。

Tick 对整个节点列表执行循环，没有逐节点异常隔离；ETask 外层即使记录错误，失败节点仍留在集合。实测先注册持续抛异常的节点，再注册计数节点，连续两个 tick 后计数仍为 `0`。WaitUntil 条件抛错也可触发，影响后续 Delay、等待及队列推进。

优化：按节点隔离异常，让失败异步任务进入 Faulted，普通回调按约定移除或停用；保证移除和资源清理完成。验收：异常节点之后的节点在当次及后续 tick 都继续执行，失败等待不会永久 Pending。

### R06 [P1] Controller 批量退出跳过退出回调

位置：[ControllerManager.cs:54](Runtime/Manager/ControllerManager/ControllerManager.cs#L54)、[Controller.cs:117](Runtime/Manager/ControllerManager/Controller.cs#L117)。状态：上次问题仍在，本次源码复核。

ExitAllAsync 在调用 ExitAsync 前 RemoveAt，导致 Controller 的 IsEnter 为 false 并提前返回，跳过监听解绑、OnExit、OnExitAsync 和组件退出。ExitOtherBeforeEnter 复用同一路径。

优化：让 ExitAsync 负责移栈，批量退出使用快照或受控循环，并定义回调内重入的处理方式。验收：两个 Controller 的退出与解绑均各一次，栈最终为空；覆盖异步退出和回调中进入/退出其他 Controller。

### R07 [P1] Controller 帧更新列表始终为空

位置：[ControllerManager.cs:28](Runtime/Manager/ControllerManager/ControllerManager.cs#L28)、[更新分发:60](Runtime/Manager/ControllerManager/ControllerManager.cs#L60)。状态：上次问题仍在，本次源码复核。

`_refresh` 默认 false，Enter 和 Remove 从未置 true，OnUpdate 不会填充 `_updateList`。成功进入的 Controller 也收不到 OnUpdate / OnLateUpdate。

优化：栈变化时标脏，使用稳定快照分发，并保持激活状态一致。验收：进入后收到两种帧回调，退出后停止，帧回调内修改栈不导致漏调用或集合异常。

### R08 [P1] 批量异步实例化未等待完成就读取 Result

位置：[IResLoaderEx.cs:69](Runtime/Manager/ResLoader/IResLoaderEx.cs#L69)。状态：上次问题仍在，本次源码复核。

`UNITY_2022_3_OR_NEWER` 分支发起 InstantiateAsync 后，仅在 Editor 且使用 AssetBundleLoader 时 await。Player 和 Editor 资源桥接模式直接读取 Result，不能保证对象已创建，也可能留下调用方未获得的实例。

优化：所有分支统一 await 完成，再做特定 Editor 分支的 Shader 恢复。验收：Editor 桥接、Editor Bundle 和 Player 三种路径返回的数组长度及有效实例数均等于 count；另测失败和 count 边界。

### R09 [P1] UPM 清单缺少无条件 asmdef 依赖

位置：[EasyFramework.asmdef:5](Runtime/EasyFramework.asmdef#L5)、[Profiler.asmdef:4](Module/Profiler/Runtime/EasyFramework.Profiler.asmdef#L4)、[package.json:17](package.json#L17)。状态：上次问题仍在；本次重新对照本机 PackageCache 的原始 asmdef.meta 确认 GUID。

Runtime 无条件引用 TMP、HybridCLR 和 URP，Profiler 也无条件引用 URP；package.json 只声明 Newtonsoft.Json 和 Scriptable Build Pipeline。纯净工程仅添加本包可能缺程序集而无法编译。versionDefines 只控制宏，不会使 references 可选；安装指南说明依赖，并未修复包的自动解析能力。

优化：明确必须依赖并在清单声明；将真正可选的 HybridCLR / URP 集成拆到独立程序集并控制启用，避免核心程序集引用可选包。确定最低支持 Unity 版本。验收：纯净工程按公开安装步骤导入，核心及各可选集成组合均无缺失引用和编译错误。

### R10 [P1] RingBuffer 写满容量时把有效数据判为空

位置：[RingBuffer.cs:26](Runtime/Core/Network/RingBuffer.cs#L26)、[Write:61](Runtime/Core/Network/RingBuffer.cs#L61)、[Advance:204](Runtime/Core/Network/RingBuffer.cs#L204)、[扩容判断:261](Runtime/Core/Network/RingBuffer.cs#L261)。验证：真实 RingBuffer 源码诊断。

读写指针相同表示空，但 FreeLength 和扩容判断允许写满整个 Capacity；写满后写指针绕回读指针，Length 变为 0。实测 `Write(new byte[buffer.Capacity])` 后 Length 为 0、TryPeek 失败；GetMemory / Advance 写满也相同。Socket 使用该缓存，满容量边界会静默丢失数据。

优化：维护有效数据计数，或保留一个哨兵空位；统一 Write、Advance、可写区域和扩容的容量语义。验收：容量减一、容量、容量加一、绕回后恰好填满，以及两种写入 API 都保持字节完整和顺序。

### R11 [P1] [已修复] HTTP 失败留下半包，DLC 随后把它当作成功资源

位置：[当前 HTTP 下载:45](Runtime/Core/Extension/HttpClientExtensions.cs#L45)、[当前文件校验:185](Runtime/Manager/DLCManager/DLCUpdater.cs#L185)、[当前按文件合并下载:191](Runtime/Manager/DLCManager/DLCUpdater.cs#L191)、[当前 UnityWebRequest 下载:124](Runtime/Extension/ETask/ETask.UnityWebRequest.cs#L124)。验证：真实下载/DLC 源码隔离诊断，注入故障 HTTP 流。

HttpClient 下载直接写最终文件，流中断后返回 false，但不删除半包。DLC 批量与按需下载只用 File.Exists 判定已完成。实测声明 6 字节、写入 3 字节后中断，最终文件仍存在；后续 DownloadAndReturnFileAsync 直接返回该路径，网络调用次数为 0。

优化：下载到同卷临时文件，完成后校验长度及内容哈希，再提交最终文件；失败/取消清临时文件，保留原有可用文件。已有缓存也需校验，按文件合并并发下载。验收：中断、取消、错误长度/哈希后均不能出现被认可的半包，下一次请求能重新下载。

当前状态：已修复。HTTP 与 UnityWebRequest 使用目标同目录唯一临时文件，完成并校验后原子提交；DLC 验证清单长度和 MD5，并按文件合并批量与按需下载。失败、取消、坏内容或提交失败保留旧文件，后续请求可重试；真实 Unity 回归通过。

### R12 [P1] 资源构建失败仍继续生成并发布 DLC 版本

位置：[AssetBundleBuilderUtility.cs:23](Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilderUtility.cs#L23)、[DLCBuilderUtility.cs:124](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L124)、[版本发布:60](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L60)。验证：源码调用链，尚未运行 Unity 故障构建。

AB 构建返回 null 时只打印错误，外层资源构建链继续运行 DLCBuilder，可能把旧输出发布为新版本。BuildDLC 复制文件异常也被捕获后继续写资源索引，调用方仍写版本文件和 LatestVersion；该索引甚至可能包含复制失败的条目。

优化：各阶段显式返回失败或抛出异常并中断父链；全部产物验证成功后再提交索引、LatestVersion 和 StreamingAssets。验收：注入 AB 构建失败、源文件不可读和复制失败，版本不推进、可用底包不覆盖、后续 Player 构建不继续。

## 历史问题清单：P2（当前 2 项已修复、12 项待处理）

### R13 [P2] 初始化异常后，后续调用永久等待

位置：[F.cs:64](Runtime/F.cs#L64)、[后续等待:82](Runtime/F.cs#L82)。状态：上次问题仍在，本次源码复核。

状态在模块创建前置 Initializing，任何异常都没有失败状态或复位；后续调用只能等待 Initialized。加载器也可能仅记录失败后正常返回，再叠加 R02，导致 Initialized 不代表 Manifest 有效。

优化：先修复 ETask 异常传播，再以可共享的初始化操作承载结果与错误，定义失败状态和重试规则；不要直接复用当前只能单次消费的 ETask 值。验收：任一步注入失败，并发和后续调用都能在有限时间内得到同一失败；允许重试时能恢复。

### R14 [P2] [本地部分修复，未验收] 窗口销毁没有完成关闭、队列清理和容器失效

位置：[UIWindowBehaviour.cs:116](Runtime/Manager/WindowManager/Behaviour/UIWindowBehaviour.cs#L116)、[Destroy:149](Runtime/Manager/WindowManager/Behaviour/UIWindowBehaviour.cs#L149)、[Window.cs:93](Runtime/Manager/WindowManager/Window.cs#L93)、[当前 WindowManager.Destroy:180](Runtime/Manager/WindowManager/WindowManager.cs#L180)。验证：真实 Window / UIWindowBehaviour 源码隔离诊断。

历史基线中，直接 Destroy 已打开窗口不先 Close，IsOpen 和监听保留；静态类型容器也不移除旧实例。历史诊断实测 GameObject 已清空，IsOpen 仍为 true。即使按示例 Close→Destroy，`_closeList` 仍有条目，超过 KeepAliveTime 后同一窗口再次执行 OnDestroy：实测计数由 1 变为 2。重复 Close 也可制造重复队列项。

优化：销毁作为幂等终态，统一关闭、解绑、队列移除、状态失效、容器移除及持有资源释放；关闭队列去重。验收：直接 Destroy、Close→Destroy、重复调用和超时清理都只释放一次，旧实例不可查询；再次 Open 得到有效新实例。

当前本地进度：WindowContainer 已删除，容器迁入 WindowManager，Destroy 已移除类型条目。这只覆盖容器失效的一部分；Window / UIWindowBehaviour 的关闭、去重、队列移除和幂等销毁尚未完成验证，R14 仍未关闭。上述历史复现未针对本地变动重新运行。

### R15 [P2] 同类型并发 OpenAsync 创建多个窗口对象

位置：[Window.cs:64](Runtime/Manager/WindowManager/Window.cs#L64)、[UIWindowBehaviour.cs:103](Runtime/Manager/WindowManager/Behaviour/UIWindowBehaviour.cs#L103)、[当前窗口容器:208](Runtime/Manager/WindowManager/WindowManager.cs#L208)。验证：真实 Window.CreateAsync 源码隔离诊断。

容器复用一个 Window，但 CreateAsync 只检查已完成的 WindowObject，没有“创建中”状态。两次调用在第一次创建完成前进入，会分别创建 GameObject，后完成者覆盖引用。实测创建请求 2 次、OnCreate 2 次，第一个对象未被销毁，最终只跟踪第二个。

优化：每个窗口共享创建操作或串行化创建/打开，明确不同参数和目标层并发时的规则；销毁时处理进行中的创建结果。验收：同时打开两次仅一个 GameObject、OnCreate 一次，失败可重试，销毁期间完成的创建没有孤立对象。

### R16 [P2] [已修复] DLC 换版本后仍返回旧哈希路径

位置：[当前路径查询:66](Runtime/Manager/DLCManager/DLCUpdater.cs#L66)、[当前版本初始化:93](Runtime/Manager/DLCManager/DLCUpdater.cs#L93)。验证：真实源码隔离诊断。

`_fileDict` 按 resName 永久缓存路径，VersionInfo 更新时没有失效。实测 v1 查询 Cube 后切到 v2，GetFileName 已返回 `cube-v2.ab`，GetFilePath 仍返回 `cube-v1.ab`。旧文件存在时会继续读旧资源，旧文件不存在时也会使用错误目标路径。

优化：在新版本校验并提交时清空路径缓存，或将缓存键与版本 UID 绑定；失败更新保留完整旧快照。验收：同资源名跨两个版本后路径、下载目标及内容一致，更新失败不混用两版数据。

当前状态：已修复。移除路径缓存，查询由当前清单实时计算；新清单通过解析、合法性与保存校验后才提交版本快照和服务器地址，失败保留旧版，请求期间配置变化时拒绝提交；真实 Unity 回归通过。

### R17 [P2] [已修复] DLC.Exists 对未知资源返回 true

位置：[当前 Exists:72](Runtime/Manager/DLCManager/DLCUpdater.cs#L72)。验证：真实源码隔离诊断。

GetFilePath 对不存在的资源返回空字符串，Exists 却执行 `string.IsNullOrWhiteSpace(filePath) || File.Exists(filePath)`，因此未知资源被判为存在。清单尚未有效加载时也会出现相同假阳性。

优化：路径非空且目标有效才返回 true；下载入口遇到未知资源直接失败。验收：未知资源、无清单、缺失文件均为 false，只有实际可用资源为 true。

当前状态：已修复。Exists 仅在清单条目存在且文件长度、MD5 均有效时返回 true；未知资源下载直接失败且不发请求，真实 Unity 回归通过。

### R18 [P2] 事件派发中退订导致漏调用与越界

位置：[Event.cs:49](Runtime/Core/Event/Event.cs#L49)、[Action 分支:65](Runtime/Core/Event/Event.cs#L65)。验证：真实 Event 源码诊断。

派发保存的是原 List 引用与派发前数量。A、B 两个订阅中，A 回调内移除自己，B 前移后被跳过，下一轮索引越界。实测 B 调用次数为 0，记录一次派发错误；IEvent 与 Action 两个分支均有同类逻辑。

优化：使用派发快照或支持重入的延迟增删，明确订阅变化在当前/下一次派发生效的语义。验收：移除自己/他人、新增订阅及嵌套 Invoke 均按约定调用且不越界。

### R19 [P2] SocketChannel.Dispose 不释放未连接的 socket

位置：[SocketChannel.cs:78](Runtime/Core/Network/SocketChannel.cs#L78)、[Dispose:113](Runtime/Core/Network/SocketChannel.cs#L113)。验证：真实源码及真实 .NET Socket 诊断。

Dispose 只调用 Disconnect，后者在 IsConnected=false 时直接返回，socket 本身没有无条件 Dispose。实测 new 后立即 Dispose，native handle 的 IsClosed 仍为 false。连接失败或异步断开后也存在绕过释放的路径。

优化：Dispose 无条件释放 socket，协调在途异步操作和回调，清理保持幂等。验收：未连接、连接失败、已连接及异步断开后均关闭 native handle，重复 Dispose 安全。

### R20 [P2] 无效发布版本会先清空可用 StreamingAssets

位置：[DLCBuilderUtility.cs:139](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L139)。验证：源码路径。

CopyVersionToStreamingAssets 先 ClearDirectory，到后面才验证源版本目录。releaseVersion 填错或版本已被缓存裁剪时，可用底包资源已经清空，方法只记录错误并返回。

优化：先验证源版本和全部文件，复制到暂存目录后再替换目标；失败传播给父构建链。验收：选择不存在版本或注入复制失败，原目标内容完整保留且构建明确失败。

### R21 [P2] 构建设置缓存使修改目录后仍打旧资源

位置：[AssetBundleBuilderSettings.cs:15](Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilderSettings.cs#L15)、[ToolExtensionObjectPool.cs:24](Editor/Tools/ToolExtensionObjectPool.cs#L24)、[ToolBase.cs:72](Editor/Tools/ToolBase.cs#L72)。验证：源码缓存路径。

AssetBundleBuildSettingExtension 构造时快照 buildDirectories，普通扩展对象又永久缓存；ToolBase.Refresh 只刷新执行事件，不刷新资源设置扩展。首次读取设置或构建后，再修改目录仍用旧数组，新配置资产也可能未被发现。

优化：设置通过实时 getter 读取，每次构建前刷新需要重新发现的配置扩展；避免把配置值作为永久实例状态。验收：同一 Editor 会话先构建 A，再改为 B 并新增配置 C，下一次产物仅包含新配置要求的资源。

### R22 [P2] DLC 扩展 Before / After 每次执行两遍

位置：[DLCBuilderUtility.cs:36](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L36)、[After:78](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L78)、[ToolBase.cs:62](Editor/Tools/ToolBase.cs#L62)。验证：源码调用链。

ToolBase.Execute 已分发 Before/Execute/After，BuildBySettings 又分发同一组 Before/After。默认 StreamingAssets 复制因此运行两次，自定义生成、清理或发布扩展也可能重复执行，内层 After 还可能早于其他扩展的 Execute。

优化：生命周期只由 ToolBase 管理，具体 Utility 只负责构建工作。验收：直接执行及经 AssetBuilder 调用时，记录扩展的顺序均为 Before→Execute→After，各一次。

### R23 [P2] Profiler 的 autoInitialize 开关无效

位置：[ProfilerManager.cs:82](Module/Profiler/Runtime/Unity/ProfilerManager.cs#L82)、[ProfilerSettings.cs:15](Module/Profiler/Runtime/Unity/ProfilerSettings.cs#L15)。验证：源码路径；文档已公开该限制。

运行时初始化回调无条件 CreateInstance，未读取默认 false 的 autoInitialize。取消自动初始化仍会创建宿主、默认性能面板及日志订阅。

优化：自动入口检查开关，保留显式启动；发布配置明确是否启用诊断组件。验收：false 时无自动宿主/记录器，true 时自动启动，false 后显式启动仍正常。

### R24 [P2] 工作线程日志直接读写非线程安全集合

位置：[ProfilerManager.cs:48](Module/Profiler/Runtime/Unity/ProfilerManager.cs#L48)、[线程回调:60](Module/Profiler/Runtime/Unity/ProfilerManager.cs#L60)、[ErrorGUIBehaviour.cs:68](Module/Profiler/Runtime/Unity/Behaviour/ErrorGUIBehaviour.cs#L68)、[Push:107](Module/Profiler/Runtime/Unity/Behaviour/ErrorGUIBehaviour.cs#L107)。验证：源码线程访问路径，未运行并发 Unity 日志压力测试。

logMessageReceivedThreaded 直接读写普通 HashSet，并向错误面板的普通 Stack 写入；主线程 OnGUI 同时 Peek/Pop/Clear。并发错误日志可丢失或破坏集合，Shader 错误事件也直接在日志产生线程派发。

优化：线程回调只写并发队列，在主线程统一去重、更新面板和通知订阅者。验收：多线程输出错误时同时关闭/清空面板，没有集合异常，去重结果及通知线程符合约定。

### R25 [P2] 关闭已展开的 Profiler 面板后 UI 仍被禁用

位置：[ProfilerGUIBehaviour.cs:98](Module/Profiler/Runtime/Unity/Behaviour/ProfilerGUIBehaviour/ProfilerGUIBehaviour.cs#L98)、[OnDestroy:61](Module/Profiler/Runtime/Unity/Behaviour/ProfilerGUIBehaviour/ProfilerGUIBehaviour.cs#L61)。验证：源码生命周期路径。

展开详情会设置 EventSystemEnabled=false；展开时关闭 ProfilerGUIBehaviour 会销毁组件，但 OnDestroy 只释放 Recorder，没有恢复事件系统，面板消失后游戏 UI 仍无法点击。

优化：OnDisable / OnDestroy 释放面板持有的输入屏蔽，保留原状态并协调其他遮罩，避免简单强制置 true 覆盖业务禁用。验收：展开后关闭/销毁面板，原本可交互的 UI 恢复，业务自己的禁用状态保持有效。

### R26 [P2] Profiler 计数器归零后仍显示旧值

位置：[ProfilerRecorderEx.cs:192](Module/Profiler/Runtime/Unity/Behaviour/ProfilerGUIBehaviour/ProfilerRecorder/ProfilerRecorderEx.cs#L192)。验证：源码更新逻辑。

OnUpdate 用 `lastValue == 0` 跳过赋值，导致 DrawCalls、GC 分配等指标从非零归零后保留上次数据，可能误导性能判断。

优化：将 Recorder 的有效性与数值分开处理，有效的 0 也必须更新；不可用指标显示独立状态。验收：输入 N→0→M，字段和面板同步变化。

## 优化实施方案

下表同步当前实施进度；每包都以本节验收完成后再关闭对应问题。数据保护的小修复可先独立交付；依赖异步语义的初始化、窗口和下载改造，应在 ETask 的基础行为稳定后合并验证。

| 顺序 | 状态 | 工作包 | 对应问题 | 设计与交付范围 | 完成门槛 |
| --- | --- | --- | --- | --- | --- |
| 1 | 已完成 | 保护用户数据和下载文件 | R01、R11、R16、R17 | 约束 DLC 文件所有权与清理根目录；临时下载、完整性校验后提交；版本快照及路径缓存一起切换 | 根目录存档不变；断网/取消不留下可用半包；换版本路径与内容一致 |
| 2 | 待执行 | 稳定 ETask 与调度基础 | R02–R05 | 统一结果、异常、取消、continuation 和消费/回池协议；逐节点隔离 tick 异常；写清单次消费语义 | 真实 TaskAwaiter 回归覆盖异常、竞争、延后首次消费、回池复用及失败节点隔离 |
| 3 | 待执行 | 修复网络与事件边界 | R10、R18、R19 | 统一 RingBuffer 满/空不变量、写入与扩容规则；事件重入策略；socket 无条件幂等释放 | 满容量/绕回字节测试、订阅变更测试及 native handle 释放测试通过 |
| 4 | 待执行 | 统一初始化和业务生命周期 | R06–R08、R13–R15 | 初始化共享结果/失败策略；Controller 栈变更与回调配对；Window 创建中/已打开/已关闭/已销毁状态；统一最终清理入口 | Unity 场景验证退出/解绑各一次、帧分发正常、并发窗口只创建一次、失败能收敛 |
| 5 | 待执行 | 让构建失败可见且可恢复 | R12、R20–R22 | 构建阶段失败立即上抛；暂存全部产物并校验后发布；统一生命周期；每次执行读取当前配置 | 故障注入时 LatestVersion/底包保持原样；回调各一次；同会话修改目录立即生效 |
| 6 | 待执行 | 完成可安装性与诊断隔离 | R09、R23–R26 | 明确最低 Unity 与必选依赖；隔离可选集成程序集；Profiler 遵循开关、主线程消费日志、恢复输入屏蔽、正确记录零值 | 纯净工程安装及集成组合编译通过；Profiler 开关/日志/输入/计数器场景通过 |

测试组织建议：在现有 Tests/Editor 中补充真实 ETask / Ticker、RingBuffer、Event 和构建失败传播测试；增加 Play Mode 的 Controller / Window / 初始化测试；独立临时工程进行 UPM 安装及 Player 冒烟构建。现有 ETaskCoroutineTests、EVoidTests 的部分路径使用自制 ManualAwaiter，不能替代真实 TaskAwaiter 的异常、竞争和回池回归。

性能优化应在行为修复后度量：关闭 Profiler 的基线与开启后的开销分别采样，覆盖高频事件、批量创建、资源加载和 UI 打开。以 GC Alloc、帧耗时分位数、峰值内存及构建耗时为指标；本次未做性能基准，不据此承诺性能收益，也不优先进行大范围抽象重写。

## 历史全量审查的验证与限制

以下为 2026-11-01 历史全量审查的复现证据：诊断在原仓库外临时目录执行，当时没有修改或切换正在打开的 Unity 工程。修复后的当前 Unity 验证另见“顺序 1 执行记录”。

| 验证组 | 方式 | 历史结果 |
| --- | --- | --- |
| 远端与增量 | fetch、提交/状态核对、固定基准 diff | HEAD 与 origin/main 均为 d20f791；Runtime / Editor / Module / Tests 增量为空 |
| 异步、网络与事件 | .NET 10 直接编译真实 ETask、Ticker、ObjectPool、RingBuffer、Event、SocketChannel；替代 Unity 调度、时间及日志依赖；socket 使用真实 .NET 实现 | 复现异常不传播、丢唤醒、首次消费前回池、写满归零、事件退订漏调用、tick 阻塞及未连接 socket 未释放；程序退出码 0 |
| DLC 与 HTTP | 复制真实 DLCUpdater、DLCVersionInfo、HashFileInfo、HttpClientExtensions，仅将 ETask 映射为 Task；替代配置/Unity/网络依赖，在独立数据目录注入中断流 | 5/5 症状复现：根目录文件误删、未知资源 Exists=true、换版本旧路径、半包残留、半包跳过重新下载；程序退出码 0 |
| 窗口生命周期 | 复制真实 Window、UIWindowBehaviour、IWindow，仅将 ETask 映射为 Task；替代 Unity、反射和外部服务；.NET 10 以 major rollforward 执行隔离程序集 | Close→Destroy 后超时将 OnDestroy 由 1 次变为 2 次；直接 Destroy 后 IsOpen=true；并发创建 2 个对象且首个未销毁；程序退出码 0 |
| 包依赖 | 对照本机 Unity 2022.3.62f3 PackageCache 内原始 asmdef.meta | TMP / HybridCLR / URP 的 GUID 引用与缺失清单声明再次确认；未据此声称纯净导入已实测 |
| Review 文件 | 检查源码链接、问题编号、严重度计数及 git diff --check | 仅更新 REVIEW.md；诊断复现的是现存缺陷，不能视为修复后的测试通过 |

历史全量审查当时未执行 Unity Edit Mode / Play Mode 测试、纯净工程导入、目标平台 Player 构建、HybridCLR 热更新或实际 CDN 更新。当前顺序 1 已完成真实 Unity Edit Mode 验证：首次交互式 TestRunnerApi 67/67、virtual 可读性整理后原项目命令行 67/67，以及最终合并 FileHelper 后原项目命令行 67/67，均为 0 失败、0 跳过；Play Mode、纯净导入、目标平台 Player / WebGL 构建、HybridCLR 热更新和实际 CDN 更新仍未验证。隔离诊断验证状态/文件/缓冲区逻辑，不验证 Unity 主线程调度、原生对象或目标平台兼容性。Editor 和 Profiler 中标为源码路径的结论仍需要对应的 Unity 故障注入与场景验收。

## 历史记录

2026-10-31 的 Review 记录了本次保留的 5 项问题，以及修正文档接口、发布步骤、Roslyn 约束、FBehaviour 说明并新增基础示例等工作。此前记录的示例编译、Controller 隔离诊断及官网构建/Playwright 验证属于上次结果，本轮未重跑，不作为当前修复验收的证据。

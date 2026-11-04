# 源码审查记录与优化进度

历史全量审查日期：2026-11-01（Asia/Shanghai）。原独立仓库审查提交：`d20f7911738d24180313b0aaca0a21df22d3e458`（`同步文档`）。原仓库执行 `git fetch origin` 后确认 `HEAD == origin/main`，当时工作区干净。此处记录的是历史审查条件，不代表当前 Unity 项目包的工作区状态。

## 当前修复进度

当前文件同步自 `D:\WorkGIT\easyframework-unity\REVIEW.md` 的完整 26 项报告；原独立仓库报告保持原样。当前修复目标为 `D:\WorkCookie\easyframework-projects\EasyFramework.Unity2022\Packages\easyframework-unity`；下文顺序 1、2 保留当时的项目路径。

历史基线共 **26 项：12 项 P1、14 项 P2**。当前 **20 项已修复、6 项待处理**；待处理为 **1 项 P1、5 项 P2**。已关闭 R01–R08、R10–R13、R15–R22；R14 保留部分修正及两项暂缓事项。顺序 1–3、5 已完成，顺序 4 **已完成（当前约定范围）**，顺序 6 尚未执行。阶段完成与历史问题全量关闭分别统计。

上述数量仅统计历史 26 项。2026-11-04 顺序 4 全量复核另确认的 **P2：Controller 的 UpdateEnabled 求值未纳入帧异常隔离**现已修复，不混入历史问题计数。最新 Window Destroy 异常保护复测完整 **Edit Mode 218/218、Play Mode 103/103 通过**；窗口 Alive 防护、重复销毁、异常后的 GameObject 释放、资源请求按缓存和其他持有者条件自动回收均已验证，异步异常入口名称 P3 也已通过回归。按最新约定，Destroy 只结束 Alive 持有，不主动卸载资源。R14 的创建中销毁收敛及直接 IWindow.Destroy 的管理器登记继续明确暂缓，不作为当前阶段的关闭条件，顺序 4 标记为已完成（当前约定范围）。验证证据见“Window Destroy 异常保护与资源持有复测（2026-11-04）”，状态口径见“顺序 4 当前范围验收结论（2026-11-04）”。

## Ticker Node 重构（2026-11-02）

当前调度契约以本节及 `Documentation/Runtime/ETask.md` 为准；下文顺序 2 的 OnRemove、待清理节点禁止重加与 `_scheduled` 描述属于重构前记录。

Ticker 改为内部 Node 双向链表和按目标引用身份比较的 `Dictionary<ITickerNode, Node>`。每次 Add 都使旧注册过期并建立新注册，新增批次从下一次 Tick 生效；Update 只摘除捕获的旧 Node，并在字典仍指向该 Node 时删除映射。回调锁外执行，稳定节点不再逐个获取集合锁或查询 HashSet，也不再重建快照。Action / Func 直接保存在内部 Node，删除委托包装对象与额外锁；已回收 Node 清空目标和委托后进入 Ticker 本地池。

`ITickerNode` 删除 OnRemove。Remove 和 Dispose 只停止注册，不通知、取消或释放 Task 本体，这是本轮有意调整的契约。TaskAwaiter 删除 `_scheduled` 和 Releasing 状态，初始化完成后才发布到 Ticker；完成但未消费保留结果并退出调度，首次消费或续体结束请求新注册。最终在自身 OnTick 内移除本轮注册、重置基础状态，调用 OnTaskResult 清理并最后回池，然后直接返回 false。归还后旧调用不再读写实例，新租借注册不受旧返回值影响。结果发布、续体注册及消费仍保留必要同步。

同时补齐 ETaskQueue 的回收门槛：取消结果被消费后，队列和后台执行仍持有任务时禁止回池；执行结束后释放持有权，避免旧命令续体执行或完成复用后的新命令。Clear 仅处理捕获的待执行批次，并保留取消回调中新加入的命令。

验证：直接对原 `EasyFramework.Unity2022` 项目使用 Unity **2022.3.62f3** 命令行执行 Edit Mode 全套测试，**146/146 通过，0 失败、0 跳过**（ETaskAwaiter 47、Ticker 26、ETaskQueue 6、原有其他测试 67）。随后针对新增的“Tick 中 Dispose 释放已遍历目标”GC 回归，重新编译并执行最新 Ticker 测试，**27/27 通过，0 失败、0 跳过**，两次进程均正常退出。覆盖真实对象池立即复用、旧回调 false / true / 抛错、新旧注册隔离、并发移除、注册覆盖、延后首次消费、初始化发布顺序、取消后的执行持有和重入 Clear；未执行 Play Mode、Player 或 IL2CPP 构建。本轮没有提交或推送。

## Ticker 与 TaskAwaiter 精简（2026-11-02，已通过 Edit Mode 回归）

将 `Ticker.Action.cs`、`Ticker.Func.cs` 及其 `.meta` 移除，委托重载集中到 `Ticker.cs`，Ticker 不再声明 partial。删除 `LogException`、`LogError` 日志包装，相关异常分支直接调用 FDebug，不再捕获日志调用自身的异常。

TaskAwaiter 删除 `Lifetime`、`ContinuationPhase` 两个枚举和 continuation state 参数。空闲由 `Token == Guid.Empty` 表示，消费由一个布尔字段表示，续体用真实 Action 和运行中/已执行两个私有静态标记表达阶段；基类由八个实例字段减为六个（泛型源另有结果字段）。`IETaskAwaiter.OnCompleted` 及三个测试用手写 awaiter 统一为 `OnCompleted(Action, Guid)`，框架 await 路径不增加闭包。完成竞争、续体锁外执行、延后首次消费、CanRecycle 门槛和回池后不再访问实例的协议保留；续体运行期间的消费由续体退出安排回收，消除重复 Add。

实现阶段按要求只完成静态源码和差异检查，没有运行测试或编译；用户随后明确授权执行测试，最新回归结果见下节。

## TaskAwaiter 两分支完成协议（2026-11-02，已通过 Edit Mode 回归）

按最新职责约束移除 TaskAwaiter.SyncRoot 和 CanRecycle，Start 由子类独占初始化，不再加锁或防御重复启动。基类缩为五个实例字段：Token、Status、异常、续体、消费交接。TrySetResult / TrySetException 通过 CAS 取得一次完成权，写入结果后发布终态，续体通过原子引用交接；手写的完成/消费发布顺序保证旧调用最后一次访问结束前不会回池。静态续体标记只用于完成交接，不创建每任务的同步对象。

显式 ITickerNode.OnTick 只保留 IsCompleted 完成分支和 OnTaskTick 更新分支。完成分支先清理本轮基础数据，再调用 OnTaskResult 最终完成并回池；没有 CanRecycle 或独立释放事件。所有框架轮询子类在操作结果已就绪时转入基类等待逻辑，不再重复检查业务条件。完成但未消费退出调度并保留结果，首次消费后重新注册，生产者与续体完成交接后进入下一次完成分支，支持延后首次 await。

ETaskQueue 的 CommandTask 自己用专属锁保护真实的后台执行持有，扩展 IsCompleted 等待所有持有结束；最后释放持有前先发布一次注册，基类不再承担这部分同步。新增/调整测试源码描述完成未消费不执行业务 Tick、消费后的下一次 Tick 最终收尾。实现阶段未运行测试，后续在用户明确授权后执行以下验证。

验证：对原 Unity 2022.3.62f3 项目运行完整 Edit Mode 回归，首次 **147/148 通过**，唯一失败为 `DLCDownloader_InvalidHashFails_AndCanRetry`；单独运行该用例再次复现。原因是下载结果已消费但最终收尾推迟到下一次 Tick，可重复使用的下载器仍凭旧 Token 拒绝重试。在 DLCDownloader.StartAsync 入口仅对已满足 IsCompleted 的上一轮执行原完成分支，再启动下一轮，不更改 TaskAwaiter 的两分支结构，也不忽略错误日志。修正后完整回归 **148/148 通过，0 失败、0 跳过**，Unity 进程退出码 0（ETaskAwaiter 48、Ticker 27、ETaskQueue 6、其他测试 67）。

另用当前真实 ETask/Ticker/ObjectPool 源码执行 .NET 10 Release 隔离验证 **3/3 通过**：完成/注册竞争且持续 Tick 256 轮、结果发布与延后消费/生产者退出交错 256 轮，以及真实对象池在旧 OnTick 未返回前立即复用。该验证只替代主循环、时间和日志，不代表 IL2CPP 或全部交错证明。本次未执行 Play Mode 或 Player 构建；提交状态以 Git 历史为准。

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

## 顺序 2 执行记录（2026-11-01）

R02–R05 已完成实现与验收，四项问题已关闭。TaskAwaiter 在同一同步边界内发布结果、异常和完成状态，锁外通知续体；消费时重新抛出保留的异常，并区分取消状态。完成后摘除调度但保留结果，首次消费且回调退出后才安全清理回池；无消费者且无引用时可由 GC 回收，不长期留在 Ticker。

Ticker 逐节点隔离 OnTick / OnRemove 错误，移除幂等，延后清理以保护同一 tick 中的回调和池复用；等待条件抛错会令相应任务失败，后续节点继续执行。ETask 对 Timer.Tick 和 Ticker.Tick 分别保护，Timer 错误不会跳过 Ticker；尚未扩展为 Timer 内部所有节点逐一隔离。内部无需结果的队列/下载入口改用 Forget 完成消费。本轮改动覆盖 ETask 核心，顺序 1 记录中的“核心没有改动”仅指当时工作包。

验证：修复前在原项目 Unity 命令行测试中复现 26 项失败（21 项 ETask、5 项 Ticker），原有 67 项通过。最终对同一 `EasyFramework.Unity2022` 项目使用 Unity **2022.3.62f3** 命令行 `-batchmode -nographics -runTests -testPlatform EditMode -testFilter EasyFramework.Tests` 执行 **115/115 通过、0 失败、0 跳过**：35 项 ETaskAwaiter、13 项 Ticker 与原有 67 项。没有创建临时 Unity 项目，没有执行 Play Mode 或 Player 构建；本轮尚未提交或推送。

后续可读性整理：Set / TrySet 共用完成路径，锁内辅助方法以 Locked 标明调用约束；保留原子 token 校验、结果先于状态发布、锁外续体及安全回池。Ticker 将重复的移除入队收敛到 QueueRemovalLocked；在串行派发、延后清理和待清理集合禁止提前重加的约束下，去掉冗余版本号及元组快照。未新增接口、Func 或上下文类，外部行为保持。此前一轮 TaskAwaiter 由 307 行精简到 283 行，Ticker 由 166 行精简到 158 行；该轮整理后对同一原项目执行 Unity 命令行 Edit Mode 回归，**125/125 通过、0 失败、0 跳过**（42 项 ETaskAwaiter、16 项 Ticker 与原有 67 项，其中本轮新增 7 项完成 API 与 3 项 Ticker 回归）。进程已退出，未创建临时 Unity 项目或启动交互式 Editor。 随后将 TaskAwaiter 的 5 个布尔字段改为两个正交小枚举 Lifetime、ContinuationPhase，并保留表示 Ticker 注册事实的 _scheduled；枚举直接表达消费/清理和续体阶段，未用 Flags 隐藏原位状态。SyncRoot 继续保护线程池/Socket 等后台完成与调度线程共享的状态，Set/TrySet 薄入口共用加锁核心，Locked 辅助不重复加锁，外部续体仍锁外执行。此轮枚举整理后再次直接对同一原项目执行 Unity 2022.3.62f3 命令行 Edit Mode 测试，**128/128 通过、0 失败、0 跳过**（45 项 ETaskAwaiter、16 项 Ticker 与原有 67 项，本轮新增 3 项状态转换回归）；未创建临时 Unity 项目或启动交互式 Editor。

## 顺序 3 执行记录（2026-11-02）

执行项目：`D:\WorkCookie\easyframework-projects\EasyFramework.Unity2022`，包子模块为 `Packages/easyframework-unity` 的 `dev` 分支。本轮完成“修复网络与事件边界”工作包，关闭 R10、R18、R19。

| 问题 | 本轮修正 | 验收结果 |
| --- | --- | --- |
| R10：满容量被判为空 | 显式记录有效字节数；统一普通写入、连续/分段写区间、Advance、扩容和 Compact；连续区间满足 sizeHint 且不暴露未读数据，Advance 不得超出本次借用 | 30 项回归覆盖容量减一/容量/容量加一、绕回填满、三种连续写入 API、分段写入、借用失效及 1000 步混合操作对照 |
| R18：回调退订污染派发 | Invoke 在回调前捕获对象与委托两组快照；首轮写时复制已在后续调整为订阅 List 与分段复用快照，当前派发和嵌套语义不变 | 首轮 14 项，后续扩展为 18 项同步事件回归；覆盖订阅变化、深层嵌套扩容、异常引用清理及预热后分配 |
| R19：未连接 Socket 未释放 | Dispose 无条件关闭原生 Socket，重复/并发调用幂等；提交与释放同步，所有同步/异步完成统一释放操作持有；最后一个回调退出后才释放事件参数与池化缓冲区 | 12 项真实 loopback/句柄回归覆盖未连接、连接失败、发送/接收在途、异步断开后、连接等待期间、回调内及回调执行期间 Dispose |

修复前，直接编译真实源码与正式 NUnit 用例的 Roslyn/Mono 隔离验证中，RingBuffer 基线为 **9/30 通过、21 失败**，事件初版为 **4/12 通过、8 失败**。Socket 原生句柄回归在原项目 Unity 命令行中明确复现 Dispose 后 `IsClosed=false`。修复后的 RingBuffer 与事件隔离验证分别为 **30/30**、**14/14** 通过。

直接对当前原项目使用 Unity **2022.3.62f3** 命令行 `-batchmode -nographics -runTests -testPlatform EditMode -testFilter EasyFramework.Tests` 执行完整 Edit Mode 回归，**204/204 通过，0 失败、0 跳过**：原有 148 项及新增 RingBuffer 30、Event 14、SocketChannel 12 项。随后补充在途操作/回调结束后实际归还底层池缓冲区的断言，Socket 聚焦回归 **12/12 通过，退出码 0**；最终完整回归仍为 **204/204 通过，0 失败、0 跳过，退出码 0**。两次最终进程均已退出，报告统一保存在项目内 `Library/Review3Validation/EditMode.xml`、`EditMode.log`，聚焦报告为同目录 `SocketChannel.xml`、`SocketChannel.log`。按用户要求使用命令行测试，没有创建临时 Unity 工程或启动交互式 Editor。

事件 API 仍要求同线程订阅/派发；RingBuffer 的借用内存、扩容和释放仍由调用者协调同步，相关契约已分别写入 Event 文档和 IRingBuffer 注释。本轮未执行 Play Mode、Player 或 IL2CPP 构建，没有提交或推送。

## Event List 快照与异步事件（2026-11-02）

按用户要求恢复同步事件的订阅 List，并新增两条复用快照 List。每次 Invoke 在回调前追加两组订阅，保存各自起点和数量，按本次区间派发；同类型嵌套仅尾部追加，finally 清除自己的尾段和引用，保留容量。有效增删不再每次分配新数组；容量不足仍会扩容，每次派发增加 O(n) 复制和清引用开销。

新增 4 项正式同步回归：五层嵌套期间两组快照扩容与完整顺序、日志器再抛异常后的同类型恢复及退订目标 GC、预热后有效对象/委托增删 1024 轮零分配、稳定及五层嵌套派发各 1024 轮零分配。对原项目执行 Unity 2022.3.62f3 命令行完整 Edit Mode 回归，**208/208 通过，0 失败、0 跳过，退出码 0**；其中 EventTests **18/18**。报告位于 `Library/EventSnapshotValidation/EditMode.xml` 和 `EditMode.log`。零分配断言仅覆盖测试中的预建处理器与预热容量，不包括业务分配、首次使用及扩容。

随后新增独立异步事件通道，统一从 `F.Event` 使用 `AddAsync/RemoveAsync/InvokeAsync`，支持 `IAsyncEvent<T>.ExecuteAsync(T)` 和 `Func<T, ETask>`；原同步 Add/Remove/Invoke 保持不变。同步和异步订阅、快照完全分离，即使参数类型相同也不相互触发或解绑。每次异步派发先捕获对象与委托两组快照，先对象后委托按注册顺序逐个 await；处理器异常与取消记录后继续，日志器自身异常仍传播给调用方。

异步派发使用每次独占的池化 Snapshot，完成后在 finally 中清引用归池，支持同类型嵌套和多个挂起派发按任意顺序完成；不共用同步派发的尾段存储。事件调用、订阅变更及续体仍遵循同线程契约；返回的 ETask 只能由调用方 await 或 Forget 一次。不承诺异步调用整体零 GC，也不新增并行广播、Task 重载或统一取消入口。

新增 **10 项 AsyncEventTests**，使用真实 ETask.TaskAwaiter 控制挂起，覆盖同步/异步隔离及挂起期间同步事件操作、逐个等待与参数顺序、订阅变化、同类型嵌套、非 LIFO 完成与重租隔离、同步/挂起后异常及取消、空/重复订阅，以及正常和日志异常路径的引用清理。最新 Unity **2022.3.62f3** 原项目命令行完整 Edit Mode 回归 **218/218 通过，0 失败、0 跳过，退出码 0**，进程正常退出；其中同步事件 **18/18**、异步事件 **10/10**。报告位于 `Library/EventSnapshotValidation/AsyncEditMode.xml` 和 `AsyncEditMode.log`。

已同步 Event/AsyncEvent 文档及入口索引；未创建临时 Unity 工程、未启动交互式 Editor，未执行 Play Mode、Player 或 IL2CPP 构建，未提交或推送。

## 顺序 4 执行记录（2026-11-02，历史完整实现）

执行项目：`D:\WorkCookie\easyframework-projects\EasyFramework.Unity2022`，包分支 `dev`。当时完成 R06–R08、R13–R15：初始化状态及失败重试；Controller 快照退出、失败清理和稳定帧分发；批量原生实例化统一等待；Window 共享创建、失败重试、销毁中创建结果收敛、关闭队列去重及统一幂等最终清理。相关主线程调用、并发参数和目标层规则同步至 Runtime 文档。窗口后续按用户要求还原，当前范围及 R14 未完成部分见下方窗口调整记录。

当时版本验证：直接在原项目使用 Unity **2022.3.62f3** 命令行执行完整 Edit Mode 回归 **218/218 通过，0 失败、0 跳过**；新增真实 Play Mode 生命周期回归 **14/14 通过，0 失败、0 跳过**。覆盖两实例退出/解绑各一次、退出异常后继续清理、进入失败回滚重试、异步退出、退出中进入其他实例、帧回调变更栈、共享初始化异常及真实 Editor 模式重试、并发创建失败重试、并发 OpenAsync 单对象和最终目标层、创建中销毁、直接/重复/关闭后销毁、旧实例不误删新实例、超时回收、原生批量实例化与 count 边界。初始化失败通过注入共享操作验证通知，未逐阶段注入真实 CDN/Manifest 故障。结果与日志位于 `Library/Order4Validation/`，两个 Unity 测试进程均已正常退出；测试产生的 ShaderVariant 与 ProjectSettings 自动改动已恢复。

未执行 Player / IL2CPP 构建或实际 CDN 初始化；Editor Bundle Shader 恢复与 Player 分支依据共同等待路径完成源码检查，未单独进行目标模式运行验收。本轮没有提交或推送。

## F 初始化精简（2026-11-02，移除 Initialization）

按用户本人修改后的 F.cs 同步文档：基于原版 switch 只使用静态 EState（None / Initializing / Initialized / Exception），不保留 Initialization 类、异常字段或等待者计数。首调用在 try/catch 中初始化；catch 记录原异常、置 Exception 并重新抛出。进入 Initializing 的其他调用分别等待状态变化，恢复时若状态为 Exception，则抛出新的通用异常 `Exception("F.InitializeAsync Error!")`。成功后重复调用直接完成。

保留自动重试：Exception 状态下的新调用立即启动初始化。当前等待者读取全局状态，没有轮次隔离；重试可能先于旧等待者恢复，因此不保证旧等待者收到先前那轮失败，也不保证与首调用收到相同异常实例。原始原因通过初始化日志查看。只使用 ETask，不引入 Task / TaskCompletionSource、轮次对象或额外初始化方法；入口仍要求 Unity 主线程。本次仅按当前源码核对并更新 F 相关文档，没有修改 F 源码。

此前带 ExceptionDispatchInfo 和等待者计数的版本在原项目 Unity **2022.3.62f3** 命令行 Play Mode 生命周期回归中 **15/15 通过，0 失败、0 跳过**，进程正常退出；覆盖真实本地无效清单、旧等待者原异常、失败续体中排队重试、并发成功等待、成功后重复调用及计数归零。该结果与 `Library/Order4Validation/FStateOnly/` 的日志属于旧版本，不能作为当前仅 EState 实现的验证结果。本次文档同步仅核对源码；新版本回归另行记录。顺序 4 的 218/218 同样为初始化精简前结果，未据此声称当前版本通过完整 Edit Mode 或 Player 验证。

## 生命周期与资源加载精简（2026-11-02）

F 文档按用户当前源码更新，未修改 F.cs。Controller.ExitAsync 删除 Run 局部包装、逐步骤 try/catch 和异常聚合，只保留一层 try/finally：依次移栈、解绑、执行退出和事件；任一步失败直接传播并停止后续步骤，finally 始终复位 _exiting。ControllerManager.ExitAllAsync 按当前源码逐实例捕获并记录异常，继续退出其他实例；文档和旧异常测试同步该契约。

AssetBundleLoader.InitializeAsync 移除临时 bundles 字典，直接 Clear 并填充 _abDict，全部条目成功后才设置 Manifest。没有额外字典分配和二次复制；中途失败可能留下部分条目，下次初始化重新 Clear，因此不再承诺资源字典的事务提交或失败回滚。

直接在原项目通过 Unity **2022.3.62f3** 命令行执行 Play Mode 生命周期回归：首次 **16/17 通过**，唯一失败是旧测试仍要求批量退出向外抛异常，与当前源码记录错误并继续的行为不符；更新断言后重新编译执行，最终 **17/17 通过，0 失败、0 跳过**，进程正常退出。覆盖 Controller 同步/异步退出异常立即传播、后续阶段停止、标记复位后重新进入退出、批量错误记录，以及当前 F 实现的首调用原异常、等待者通用异常和失败后重试。AssetBundleLoader 直接填充路径完成编译及差异检查，未单独构建 Bundle 故障场景。

结果与日志位于 `Library/Order4Validation/LifecycleSimplification/PlayMode-final.xml` 和 `PlayMode-final.log`。F.cs 与 Startup.cs 和测试前备份哈希一致；测试产生的 ShaderVariant / ProjectSettings 自动改动已恢复。本轮未重复执行完整 Edit Mode 或 Player 构建，未提交或推送。

## Window 还原与创建状态（2026-11-02）

按用户要求将 Window 恢复到原有生命周期实现，只增加 CreateAsync 的创建中标记和 try/finally 复位；异步创建中再次同步或异步创建均以 InvalidOperationException 失败，已有对象仍直接复用。Destroy 按追加要求先同步调用 Close，再执行原有 OnDestroy、组件 Destroy 和 Object.Destroy。移除共享 Task / TaskCompletionSource、已销毁标记及逐步骤清理包装。

保留 UIWindowBehaviour 的关闭队列去重、重新打开和销毁时移除队列、批量操作快照，以及超时经 WindowManager 销毁；保留管理器按实例身份在回调前移除类型容器。重复 Destroy 幂等、创建中销毁后的迟到对象收敛仍未实现，R14 恢复为部分待处理。R15 当前以拒绝重叠创建的契约关闭，不再承诺多调用共享创建结果；参数在管理器入口写入，同类型打开仍应顺序等待。

本次窗口文档按该范围同步。上方顺序 4 的 14/14，以及后续 F / 生命周期精简的 15/15、17/17 均属于窗口还原前版本。当前直接在原项目使用 Unity **2022.3.62f3** 命令行重新编译并执行 Play Mode 生命周期回归，**19/19 通过，0 失败、0 跳过**，进程正常退出。覆盖异步创建中的重复同步/异步调用被拒绝且不清除首调用状态、异常和空对象返回后标记复位、成功后对象复用、重叠 Open 拒绝及顺序重开，以及直接 Destroy / Close 后 Destroy 的解绑→关闭→销毁顺序。

结果与日志位于 `Library/Order4Validation/WindowCreationGuard/`。F.cs 与 Startup.cs 和测试前备份哈希一致；测试产生的 ShaderVariant 与 ProjectSettings 自动改动已恢复。本轮未重复执行完整 Edit Mode 或 Player 构建，未提交或推送。

## UIWindowBehaviour 统一更新（2026-11-02）

移除 _closeList 及全部加入、去重、移除和独立遍历逻辑。_windowLayerDict 跟踪已打开和已关闭的窗口，_uiLayerWindowDict 仅保留已打开窗口；Close 只从层级列表移除，继续保留超时检查资格。_updateList 按需从跟踪字典建立稳定快照，同时负责打开窗口 Update 和关闭窗口 NeedDestroy 检查。

AddWindow / RemoveWindow 统一标记刷新，Destroy 在回调前移除跟踪映射，下一帧才重建快照；当前遍历和 RefreshAllWindow 检查映射成员资格，跳过本帧其他回调已经销毁的实例。解决审查中“超时回收的销毁回调又销毁另一个关闭窗口”导致的关闭队列越界，回调中新开窗口下一帧开始更新。

直接在原项目通过 Unity **2022.3.62f3** 命令行重新编译并执行 Play Mode 回归，**23/23 通过，0 失败、0 跳过**，进程正常退出（原有生命周期 19 项、新增统一更新 4 项）。覆盖超时销毁互相重入且每实例一次、更新回调销毁同快照实例后跳过、快照延后刷新、关闭窗口继续跟踪且查询排除、重复关闭/重新打开、以及销毁回调新窗口下帧更新。结果与日志位于 `Library/Order4Validation/UnifiedWindowUpdate/PlayMode-final.xml` 和 `PlayMode-final.log`。

本轮只完成这一项优化，不据此关闭顺序 4 的全部问题；此前审查中的 Controller 同实例重入退出及 Window.OnCreate 失败重试缺口未在本轮修改。没有重复执行完整 Edit Mode 或 Player 构建，未提交或推送。

## WindowManager 合并（2026-11-02）

移除 UIWindowBehaviour 脚本及其 meta，将窗口跟踪、打开/关闭、层级排序、查询、刷新和统一快照更新合并到 WindowManager。所有泛型和 Type 入口直接调用本类逻辑，IWindowManager 接口不变。WindowManager 在 OnCreate 初始化层列表后注册 IFrameworkTick，由 FBehaviour 分发更新；销毁时移除注册。保留帧入口统一异常记录，窗口回调失败不会跳过宿主中的后续模块。

沿用上一节的跟踪字典与稳定 _updateList：关闭窗口仍参与超时检查，销毁先移字典并标记下一帧刷新，当前快照跳过已移除实例，回调中新开的窗口下一帧参与更新。帧循环现由 FBehaviour 驱动，禁用 UI Root 不再停止管理器更新。删除旧组件专用 Inspector，将运行时层级/窗口调试显示迁到现有 UIRootBehaviourInspector；项目序列化资源未发现旧组件脚本 GUID 引用。

直接在原项目使用 Unity **2022.3.62f3** 命令行重新编译并执行 Play Mode 回归，**25/25 通过，0 失败、0 跳过**，进程正常退出（生命周期 19 项、WindowManager 更新 6 项）。保留四项快照/重入/超时回归，并新增真实 FBehaviour 注册分发、移除后停止，以及窗口 Update 抛错后后续订阅者仍执行的验证。Runtime 与 Editor 程序集编译通过；Inspector 未进行视觉验收。

结果与日志位于 `Library/Order4Validation/WindowManagerMerge/`。未重复执行完整 Edit Mode 或 Player 构建；本轮不修改此前剩余 Controller 重入退出及 Window 初始化失败重试问题，不将顺序 4 标为全部完成。未提交或推送。

## ControllerManager 统一生命周期（2026-11-04，初版）

按本次达成共识的设计，将实例缓存、组件发现与注册、参数写入、进入退出、激活、帧分发和销毁集中到 ControllerManager。Controller<T> 保留 protected 业务回调、事件、只读查询和接口转发；Instance 与无参实例 EnterAsync / ExitAsync 只转发到同一管理器，不保留第二份缓存。移除 Controller.TParams 和同步 DestroyInstance；带参数通过管理器 0–3 个强类型 ITParams 重载进入，销毁统一使用可等待的 DestroyAsync。

刷新进入先占用本实例操作、写入最新参数，重新执行 PreEnter / Enter / AfterEnter；PreEnter 成功后即入栈激活，已有实例移到栈顶，监听仅在首次加入当前进入生命周期时绑定一次。进入、退出或销毁进行中，同一实例的其他过渡直接抛异常；不同实例可以嵌套调用。异常首错中断并向上传播，finally 只释放操作保护，不自动补偿退出、不设置 Faulted。PreEnter 前后是否已提交栈关系由管理器记录，不以业务回调是否成功代替入栈状态；退出先移栈再清理，失败不会重新入栈。

进入轮次同时用于 ExitAll 批次与同帧 Update / LateUpdate 快照。B 的退出回调退出并重新进入 A，或仅刷新 A，旧批次均跳过新轮次；帧回调中的新轮次从下一帧参与分发，组件分发也验证轮次。每个实例的激活版本使切换回调中“切走又切回”的旧通知失效，避免重复激活或误丢其他实例的停用通知。ExitAll 尝试入口批次中的所有有效轮次，最后传播单个原异常或 AggregateException；这取代历史版本“只记录错误并继续、不向调用方报告”的行为。

同步 ControllerManager / F 文档、快速开始及基础示例，并将旧生命周期测试的失败回滚、批量退出吞错断言调整到当前契约。新增 25 项 ControllerManager 测试，覆盖 0–3 参数及默认值、刷新进入与监听去重、各类过渡冲突、参数 setter 重入、提前入栈、异常传播及操作复位、完成事件重入、批次和帧轮次、激活回调重入、创建失败及管理器销毁。窗口运行时未改动，但按此前通过后同步文档的约定，补齐其当前接口、打开/关闭缓存、IsLoading、实例身份保护及 WindowLayerInfo 即时重排说明。

验证：原 EasyFramework.Unity2022 工程 Unity **2022.3.62f3** 命令行完整 Edit Mode **218/218 通过**，完整 Play Mode **76/76 通过**，均 0 失败、0 跳过、进程正常退出。Play Mode 包含 ControllerManager 25、原生命周期 20、窗口组件 2、窗口诊断 18、窗口更新 11。结果与日志位于 `Library/Order4Validation/ControllerManagerCentralization-20261104/`（`EditMode.xml`、`PlayMode-final.xml`）。

本轮未修改业务 Startup / MainController / MainWindow，也未改动窗口运行时；这些业务文件、ShaderVariant 和 ProjectSettings 与测试前哈希相同。未执行 Player / IL2CPP 构建，未提交或推送。R06 的同实例重新进入误退已修正，R07 的帧分发增加轮次保护；R14 明确暂缓的窗口销毁终态范围仍保持待处理，不据此将顺序 4 标为全部完成。

## Controller 组件封装边界（2026-11-04）

按追加要求修正初版的职责边界：ControllerManager 只持有和调用 IController，删除 Entry 的组件数组以及全部组件发现、owner 注册和分发循环。IController / IControllerManager 均不暴露组件接口；直接实现 CustomController : IController 的类型使用同一套管理器入口、操作保护和轮次快照，不需要继承 Controller<T> 或引用 IControllerComponent。

组件支持收敛到 Controller<T> 内部：IController.OnCreate 的实现负责发现组件、为全部组件 Register owner，并按业务回调在前、组件在后的顺序转发各生命周期阶段。基类在新进入的 PreEnter 开始、移除监听和销毁时更新本地分发版本，旧组件帧工作即使遇到刷新提交后激活回调失败也会停止；组件激活通知则独立检查本实例激活版本和当前激活结果，不因无关栈变化或失败的准备阶段而遗漏有效停用通知。管理器仍只决定 IController 的进入、退出、激活、帧更新和销毁时机，不感知这些内部转发。

新增 6 项不继承基类且不引用组件接口的 CustomController 回归，覆盖强类型参数、刷新进入与监听次数、异步冲突、异常后的管理器状态、ExitAll 轮次、帧快照和销毁保护。原 ControllerManager 测试增至 27 项，补充失败激活后旧组件帧调用中止和失败准备不丢失停用通知两条路径。

验证：原工程 Unity **2022.3.62f3** 命令行完整 Edit Mode **218/218 通过**、完整 Play Mode **84/84 通过**，均 0 失败、0 跳过、进程正常退出。Play Mode 包含 ControllerManager 27、CustomController 6、原生命周期 20、窗口组件 2、窗口诊断 18、窗口更新 11。结果位于 `Library/Order4Validation/ControllerComponentEncapsulation-20261104/`（`EditMode.xml`、`PlayMode-final.xml`）。ControllerManager 文档同步此边界；业务 MainController / Startup / MainWindow、ShaderVariant 和 ProjectSettings 与测试前哈希一致。未执行 Player / IL2CPP 构建，未提交或推送。

## Controller 实例入口与监听封装（2026-11-04）

按最新要求收敛公开入口：从 IControllerManager 删除 DestroyAsync，具体管理器仅保留内部清理，不作为业务接口。业务结束本轮流程使用 ExitAsync，实例留在管理器会话缓存；测试通过仅存在于测试程序集中的清理扩展释放隔离实例。示例、快速开始和运行时文档改用 X.Instance.EnterAsync(...) / ExitAsync()。

Controller<T> 增加 1–3 参数 EnterAsync 转发，加上已有无参入口可直接进行实例跳转。参数仍由管理器在验证实例归属并占用 Entering 后写入；不支持对应 ITParams 形状时明确抛 ArgumentException，冲突请求不改参数或默认值。管理器的有参泛型重载继续保留编译期 ITParams 约束，CustomController : IController 仍可直接使用管理器入口。

OnAddListeners / OnRemoveListeners 从 IController 移除，管理器 Entry 不再保存监听标记或调度监听阶段。Controller<T> 在 IController.OnEnterAsync 内部首次绑定监听，再执行业务进入；刷新进入不重绑。IController.OnExitAsync 内部先按需解绑，再执行业务退出。组件监听转发同样由基类完成，自定义 IController 自行实现内部监听，不受基类约定影响。

新增 4 项实例入口及公开接口回归：0–3 参数和默认值、错误参数类型/数量不改写、过渡期间拒绝且不改参数、公开接口不含销毁或监听调度。CustomController 测试改为在自身进入退出中维护监听。原工程 Unity **2022.3.62f3** 命令行完整 Edit Mode **218/218 通过**、完整 Play Mode **88/88 通过**，均 0 失败、0 跳过、进程正常退出。Play Mode 包含 ControllerManager 31、CustomController 6、原生命周期 20、窗口组件 2、窗口诊断 18、窗口更新 11。结果位于 `Library/Order4Validation/ControllerPublicSurface-20261104/`（`EditMode.xml`、`PlayMode.xml`）。业务脚本、ShaderVariant 和 ProjectSettings 与测试前哈希一致，未执行 Player / IL2CPP 构建，未提交或推送。

## Controller 内部阶段与实例事件（2026-11-04）

按最新职责约定进一步精简 IController：删除 OnAfterEnterAsync、OnEntered、OnExited。AfterEnter 与组件 AfterEnter 收回 Controller<T> 的 IController.OnEnterAsync 实现末尾，管理器只等待该进入接口，不再单独编排后置阶段；自定义 IController 不必实现这些扩展约定。

完成事件是 Controller 的自定义行为，不属于管理器契约。X.Instance.EnterAsync 的无参和 1–3 参数入口分别等待管理器完成后，自行发送 OnEnterEvent；实例 ExitAsync 在调用管理器前记录当前入栈状态，成功退出后按需发送 OnExitEvent，稳定的重复退出不重复通知。事件发送时管理器已释放操作保护，事件内可以再次进入或退出同一实例，旧流程不会再覆盖新轮次；事件订阅者抛错由实例入口向调用方传播。直接管理器进入、退出及 ExitAll 只执行 IController 生命周期，不发送基类实例事件。此前将事件放入受保护生命周期内的尝试已撤回，不改变原先实例事件可重入的目标。

实例无参进入也按当前实例身份校验，避免自行 new 出来的 Controller 进入管理器中的另一实例，却向错误实例发送事件。补充 AfterEnter 异常中断、不发送失败阶段的事件、实例事件异常传播、管理器直调与批量退出不发送事件、0–3 参数事件顺序及未登记实例拒绝的回归；原完成事件重入测试改为显式通过实例入口发起。

验证：原工程 Unity **2022.3.62f3** 命令行完整 Edit Mode **218/218 通过**、完整 Play Mode **93/93 通过**，均 0 失败、0 跳过、进程正常退出。Play Mode 包含 ControllerManager 36、CustomController 6、原生命周期 20、窗口组件 2、窗口诊断 18、窗口更新 11。结果位于 `Library/Order4Validation/ControllerInstanceEvents-20261104/`（`EditMode.xml`、`PlayMode.xml`）。同步 ControllerManager 文档；业务脚本、ShaderVariant 和 ProjectSettings 与测试前哈希一致。未执行 Player / IL2CPP 构建，未提交或推送。

## 顺序 4 全量复核（2026-11-04）

本轮直接复核原 EasyFramework.Unity2022 工程中的包，分支 dev，固定基准为 `805375e477c7dc3e460be9c1d21e83b6a897ecfc` 到当前工作区，包含未跟踪的 WindowLayerInfo 和测试文件。需求来源为 R06–R08、R13–R15 及本次对话确认的最新约定；不将初始化旧等待者失败隔离、窗口直接重复销毁终态或创建中销毁收敛重新列为新增需求。

### Standards

依据 AGENTS.md 及对应 Runtime 文档复核，未发现新增明确规范违规或必须整改的结构性气味。Runtime / Editor 边界、UI Root Inspector 的 WindowList 适配、ControllerManager 仅调度 IController 的职责边界保持正确。参数、监听、AfterEnter、实例事件和自定义 IController 的最新说明已匹配源码；本轮补充资源字典直接填充、宿主异常隔离范围及窗口复用措辞。

### Spec

| 任务 | 当前设计与复核结果 | 验证或限制 |
| --- | --- | --- |
| R06：批量退出 | 管理器统一调度，退出先移栈；批次捕获实例及进入轮次，旧批次跳过退出后重入或刷新进入的同一实例。单实例首错中断，批量尝试有效项后报告单个原异常或 AggregateException | ControllerManager 和直接实现 IController 的批次回归通过 |
| R07：帧分发 | 入栈和新轮次标脏，Update / LateUpdate 共享稳定快照；管理器检查实例、轮次和 UpdateEnabled，Controller 内部保护组件分发。复核时另发现资格 getter 异常隔离缺口，现已修复 | 首次复核的两项故障注入失败记录保留下文；修复后两项及完整 Play Mode 95/95 通过，见修复记录 |
| R08：批量原生实例化 | 所有原生分支统一等待 InstantiateAsync 完成后读取 Result；0 不加载并返回空数组，负数在加载前抛错 | 原工程 Play Mode 真实创建 5 个实例及父节点检查通过；Editor Bundle Shader 恢复及 Player / IL2CPP 未单独构建运行 |
| R13：全局初始化 | 仅 ETask 和静态 EState，等待调用各自独立；首调用原异常向上传播，其他等待者在观察到 Exception 时得到通用异常，后续新调用自动重试 | 独立等待、真实本地无效 DLC 清单和失败后重试通过；旧等待者不隔离轮次为已接受限制 |
| R14：窗口销毁与缓存 | 管理器核验实例身份，先删除匹配类型缓存再移除层关系并调用 Window.Destroy；Window 先 Close，打开 / 关闭快照按需分类，加载过滤及旧缓存保护保持有效 | 普通、超时互相销毁及旧缓存不误删新实例通过；直接 IWindow 重复销毁终态、创建中销毁迟到结果和清理首错后的完整释放仍暂缓 |
| R15：重叠窗口加载 | IsLoading 在 finally 复位，管理器在修改层登记前拒绝重叠请求并返回 null；资源为空或初始化失败清理，后续可重建 | 同步 / 异步重叠、跨层请求、失败清理和成功后复用通过；窗口参数仍在入口写入，同类型应顺序等待 |
| 关联优化：AssetBundleLoader | 直接 Clear 并填充 _abDict，不额外分配 bundles；全部条目处理成功后设置 Manifest | 源码和编译检查通过；中途部分条目保留、下次重试重新清空符合当前约定，不声称事务回滚或真实 CDN 故障验收 |

Controller 当前公开使用方式为 X.Instance.EnterAsync 的 0–3 参数入口及 ExitAsync；管理器保留强类型泛型入口供 CustomController : IController 使用，不公开 DestroyAsync。组件发现、监听去重、AfterEnter、内部回调转发都在 Controller 内；Manager 不引用这些实现。完成事件仅由实例入口成功等待管理器后发送，支持事件内再次进入或退出；管理器直调和 ExitAll 不发送基类实例事件。异常只传播，复位管理器操作保护，不自动补偿 Exit、不引入 Faulted；已提交的结构不因业务失败回滚。

WindowLayerInfo 保持即时重排设计：普通 Add / Remove 不主动排序，排序中变更立即嵌套重排，旧循环在当前窗口调用返回后经 UID 检查退出，finally 复位。窗口及组件逐次读取 Canvas.sortingOrder，删除后允许数值空洞，只要求相对顺序正确。OnLoadObject、组件 owner 注册、首次打开后排序、加载过滤和身份清理均由现有回归覆盖。

**本次新增 P2（已修复）：UpdateEnabled getter 抛错会中断后续帧分发。** 位置：[ControllerManager.DispatchFrame](Runtime/Manager/ControllerManager/ControllerManager.cs#L312)。首次复核时资格检查在逐实例 try 之外，CustomController 或基类重写的 UpdateEnabled 抛错会逃出整个 DispatchFrame。最小复现：先进入 A、B，帧快照按 A、B 保存；A 的 UpdateEnabled getter 抛错，B 的 Update / LateUpdate 均未被调用，异常还可能中断 FBehaviour 的后续订阅者。新增 `CustomControllerTests.FrameEligibilityFailure_DoesNotSkipTheNextController(false/true)` 当时均复现：后续实例调用次数预期 1，实际 0。

首次复核建议把资格检查移入已经存在的逐实例 try，保持原有回调隔离，不增加捕获层；当时只更新复核与文档，没有修改运行时。随后按用户要求完成该最小修正，资格 getter 与帧回调共用原有捕获范围，修复后的验证见下节。R07 原始空帧列表问题和这一追加边界均已修复。

### 本轮验证

原工程 Unity **2022.3.62f3** 命令行完整 Edit Mode **218/218 通过**；完整 Play Mode **93/95 通过，2 项失败、0 跳过**，失败均为上述新增 P2 的 Update / LateUpdate 复现。其余 93 项均通过：ControllerManager 36、CustomController 既有 6、生命周期 20、窗口组件 2、窗口诊断 18、窗口更新 11。测试和日志位于 `Library/Order4Validation/Order4Acceptance-20261104/`（`EditMode.xml`、`PlayMode.xml` 及同名 log）；Edit Mode 进程退出码 0，Play Mode 因诊断失败退出码 2。

本轮更新 REVIEW 和对应模块文档，新增两条有意义的帧资格求值异常回归；未修改 Runtime、业务 Startup / MainController / MainWindow。测试前后 Runtime、业务脚本、ShaderVariant 和 ProjectSettings 哈希核对一致。未执行 Player / IL2CPP 构建、纯净安装、真实 CDN 初始化或 Inspector 视觉验收，未进行性能基准，未提交或推送。

首次复核结论：Standards 无新增问题；Spec 新增 1 项 P2，R14 保留明确暂缓范围。该新增 P2 随后修复并通过验证，当前结论见下节；上述首次失败结果保留为修复前证据。

## Controller 帧资格异常隔离修复（2026-11-04）

按用户要求修复上一节确认的 P2。唯一运行时改动是将 DispatchFrame 中 IsCurrent / UpdateEnabled 资格判断及本地 Entry 获取移入已有的逐实例 try；不新增异常处理层，也不修改进入退出、组件、监听、事件或快照时机。自定义 UpdateEnabled getter 抛错时记录原异常，并继续本次 Update / LateUpdate 的后续 Controller，不向 FBehaviour 传播该实例异常。

沿用前轮两条故障注入回归，现均通过：后续实例各执行 1 次，异常没有逃出分发，日志保留原异常实例。原工程 Unity **2022.3.62f3** 命令行完整 Play Mode **95/95 通过**、完整 Edit Mode **218/218 通过**，均 0 失败、0 跳过、进程退出码 0。Play Mode 包含 ControllerManager 36、CustomController 8、生命周期 20、窗口组件 2、窗口诊断 18、窗口更新 11。结果位于 `Library/Order4Validation/ControllerEligibilityFix-20261104/`（`PlayMode.xml`、`EditMode.xml` 及同名 log）。

ControllerManager / FBehaviour 文档和当前任务状态已同步；修复前 93/95 的诊断记录仍保留在上节。业务 MainController / Startup / MainWindow、ShaderVariant、ProjectSettings，以及本轮开始时已存在的 PlayerBuilder 源码和文档改动均与快照哈希一致，未覆盖其他工作。未执行 Player / IL2CPP 构建，未提交或推送。

当前约定的顺序 4 范围已通过复核及回归，新增 P2 关闭；R14 直接重复 IWindow.Destroy 终态、创建中销毁迟到对象和清理异常后的完整释放仍为明确暂缓范围，因此顺序 4 历史工作包继续标为部分完成。

## Window Alive 增量复核与回归（2026-11-04）

复核原 EasyFramework.Unity2022 工程包的 dev 分支，固定基准为 `4f9034e2a8dac4bdca8034491082bf79d39a55c1` 到本轮开始时的工作区，命令为 `git diff 4f9034e2a8dac4bdca8034491082bf79d39a55c1 --`。运行时增量仅 IWindow.cs 和 Window.cs。按最新实际源码，Alive 初始为 true，Create 不赋值，Destroy 在清理前置为 false；已销毁实例的 Open / OpenAsync 抛出 InvalidOperationException，由管理器入口捕获并记录异常、返回 null。直接 IWindow 调用只传播异常，日志由调用方负责。

**Standards：**依据 AGENTS.md 与对应 Runtime 文档，未发现新增明确规范违规或需要整改的结构性气味。短入口判断保留原有风格，不引入新抽象。同步窗口文档中仍称“没有销毁标记、重复 Destroy 会重复回调”的旧说明，明确当前入口防护和剩余限制。

**Spec：**当前约定的 Alive 初值、关闭后保持存活、Destroy 防重复及递归、已销毁实例拒绝打开、帧/刷新/激活入口跳过均通过。复核发现的 **P3：Window.OpenAsync 已销毁异常入口名称错误，现已修复**；`Runtime/Manager/WindowManager/Window.cs:119` 已改为 `Window.OpenAsync({Type.Name}): window has been destroyed.`，同步入口继续显示 Window.Open。该修正只调整异常文本，完成静态核对，未重复执行 Unity 测试。创建中销毁的迟到对象、清理首错后继续释放，以及直接 IWindow.Destroy 主动撤销管理器缓存，仍属此前明确暂缓的 R14 范围，不重新计为新增问题。

新增 6 项真实 Play Mode 回归：同步/异步拒绝重开且不重新加载（2）、关闭后存活及递归/重复销毁只清理一次（1）、销毁后不再执行帧/刷新/激活回调（1）、同步/异步管理器入口记录原异常并返回 null、移除旧缓存（2）。直接在原工程使用 Unity **2022.3.62f3** 命令行执行完整 **Edit Mode 218/218、Play Mode 101/101 通过**，均 0 失败、0 跳过、进程退出码 0。Play Mode 包含 ControllerManager 36、CustomController 8、生命周期 24、窗口组件 2、窗口诊断 20、窗口更新 11。

报告与日志位于 `Library/Order4Validation/WindowAliveReview-20261104-173742/`（`EditMode.xml`、`PlayMode.xml` 及同名 log）。上述回归执行前后全部 208 个 Runtime C# 文件哈希一致；业务 MainController / Startup / MainWindow、ShaderVariant 与 ProjectSettings 哈希也一致。随后按用户要求修正 OpenAsync 异常文本并同步文档；218/218、101/101 是该文本修正前的回归结果。未执行 Player / IL2CPP、真实 AssetBundle 在途卸载或设备验收，未提交或推送。R14 仍为部分修正，顺序 4 仍为部分完成。

## Window Destroy 异常保护与资源持有复测（2026-11-04）

按用户最新源码和约定复测：Window.Destroy 在 Alive=false 后，用一层 try/catch 执行 Close、窗口 OnDestroy 及组件 OnDestroy；普通回调失败时记录原异常并停止后续业务回调，随后继续清空组件列表、销毁 GameObject 和清空 WindowObject。当前约定不要求每个失败阶段之后继续调用全部业务回调。Destroy 不主动调用资源卸载，只需窗口不再以 Alive=true 长期持有资源。

新增 2 项 Play Mode 回归：一项对窗口解绑、窗口关闭、窗口销毁和组件销毁分别注入异常，验证不向外传播、日志保留原异常、Alive 持续为 false、重复销毁不重试、WindowObject 清空，并在下一帧确认原生 GameObject 已销毁；另一项使用真实 AssetBundleRequest、隔离 AssetBundleLoader Tick 和 Bundle=null 的 AssetBundleInfo，验证活跃窗口保留请求、Destroy 不立即移除资源请求、缓存期限和其他活跃持有者仍有效，最后一个窗口销毁且缓存到期后由加载器移除请求、将 RefCount 降为零并清空持有者引用。该测试没有读取真实 AssetBundle 文件，不代表原生 Bundle 卸载或设备验收。既有同步/异步拒绝重开测试同时补充入口名称断言，确认 OpenAsync 文本修正。

直接在原 EasyFramework.Unity2022 工程使用 Unity **2022.3.62f3** 命令行执行完整 **Edit Mode 218/218、Play Mode 103/103 通过**，均 0 失败、0 跳过、进程退出码 0。Play Mode 包含 ControllerManager 36、CustomController 8、生命周期 26、窗口组件 2、窗口诊断 20、窗口更新 11。报告与日志位于 `Library/Order4Validation/WindowDestroyCatchRetest-20261104-175443/`（`EditMode.xml`、`PlayMode.xml` 及同名 log）。

测试前后全部 208 个 Runtime C# 文件哈希一致；业务 MainController / Startup / MainWindow、ShaderVariant 和 ProjectSettings 哈希也一致。保留用户新增的 try/catch，本轮只补充测试和文档。未执行 Player / IL2CPP、真实 AssetBundle 在途卸载或设备验收，未提交或推送。R14 仍有创建中销毁的迟到对象及直接 IWindow.Destroy 不主动撤销管理器登记两项待处理；不再把“Destroy 主动卸载资源”或“逐回调继续全部业务清理”作为当前验收要求。

## 顺序 4 当前范围验收结论（2026-11-04）

按当前对话确认的实现范围及最新复测结果，顺序 4 标记为 **已完成（当前约定范围）**。R06、R07、R08、R13、R15 和当前选定的 R14 处理方式均已有实现及相应验证；Controller 帧资格异常隔离 P2、OpenAsync 异常入口文本 P3 均已修复。最新完整回归为 Edit Mode 218/218、Play Mode 103/103，0 失败、0 跳过、进程退出码 0。

R14 的历史问题仍保持“部分修正”，仅保留两项明确暂缓事项：创建中销毁的迟到 GameObject 收敛，以及直接 IWindow.Destroy 主动撤销管理器登记。这两项不计入当前阶段完成门槛，不能把阶段完成解读为已实现全部历史销毁保护。Destroy 不主动卸载资源、生命周期回调首错后停止而继续原生对象销毁，属于当前选定契约。业务通过 F.WindowManager 管理窗口，并等待打开完成后再销毁。

前文按历史全量问题范围写下的“顺序 4 部分完成”保留为当时执行记录；当前状态以本节、顶部汇总和实施方案表为准。Player / IL2CPP、真实 AssetBundle 在途卸载及设备验收未执行，继续保留验证限制。本次仅同步完成状态，没有修改运行时代码或重新执行测试。

## 顺序 5 执行记录（2026-11-04）

执行项目为 `D:\WorkCookie\easyframework-projects\EasyFramework.Unity2022`，框架包分支为 `dev`。本轮完成 R12、R20、R21、R22。保留原有顺序 4 文档改动及项目业务、ShaderVariant、PlayerSettings 改动，没有提交或推送。

| 问题 | 实现与验收 |
| --- | --- |
| R12：构建失败继续发布 | AB 管线 null Manifest 直接抛错；真实 Bundle、JSON Manifest 校验后才替换目录。DLC 只将复制并通过长度/MD5 校验的资源加入清单；源缺失、不可读、名称冲突、复制或提交失败直接上抛。Player 前置资源失败不进入主体；另补齐 Player 缺少设置、null/失败/取消报告的失败传播。 |
| R20：无效版本清空底包 | 先验证版本、清单 UID 和全部资源，再平铺复制到暂存目录、复核并替换 StreamingAssets。DLC Utility 显式管理 LatestVersion、历史列表、版本目录、历史裁剪与自动底包复制，本次替换失败恢复本组旧产物。选定历史 releaseVersion 会额外保留，避免被本次裁剪删除。 |
| R21：配置缓存 | 默认目录改实时 getter；每次收集 Bundle 配置重新发现 ScriptableObject。覆盖同会话 A→B、配置资产新增/修改/禁用/删除及 null；真实 SBP 两次构建确认旧 Bundle 被移除、新 Bundle 可加载。 |
| R22：生命周期重复 | 删除 DLC Utility 内部重复 Before/After；ToolBase 每次捕获同一扩展数组，按 Before→Execute→After 各一次。直接 Utility 不派发事件；直接工具和 AssetBuilder 所用的父接口入口均已验证。 |

按用户确认的 Tool 设计调整，删除初版 `BuildOutputTransaction` 及 ThreadStatic 父子事务。ToolBase 保持生命周期编排，只管理自己的版本文件：执行中提供本次编号，本工具全部阶段成功后才保存，失败恢复本工具的旧编号和时间。AB、DLC、HybridCLR 各 BuilderUtility 自行暂存、校验、显式替换和清理本次产物，复用 FileHelper 的纯静态文件操作；禁用 HybridCLR 时 DLC 不读取其旧目录。已成功完成的工具不因后续工具或 After 回调失败而隐式撤销。

设计调整后，对原项目使用 Unity **2022.3.62f3** 命令行 `-batchmode -nographics -runTests -testPlatform EditMode -testFilter EasyFramework.Tests`。最新完整回归 **265/265 通过、0 失败、0 跳过，进程退出码 0**。其中顺序 5 新增 47 项：AB 5、构建链 4、FileHelper 文件替换 12、发布/复制 20、配置刷新 6；已删除初版面向内部事务类的 10 项测试，改为公开文件操作和独立调用行为回归。报告：[EditMode.xml](../../Library/Order5Validation/ToolDesignFinal/EditMode.xml)，日志：[EditMode.log](../../Library/Order5Validation/ToolDesignFinal/EditMode.log)。

AB 测试包含真实 SBP 回调返回 Canceled 后得到 null Manifest、实际构建/重建并加载 TextAsset，以及 BuildManifestOnly 保留已有资产 GUID。发布测试包含 Windows 独占文件锁、LatestVersion/历史列表提交故障、无效版本/文件校验失败保护，以及直接工具和父接口入口回调各一次。FileHelper 测试通过公开方法验证本组替换失败恢复、历史裁剪保护和独立调用没有隐式回滚。DLC After 失败测试明确验证异常传播、本工具版本不推进，而此前已经完成的 DLC/底包发布保持有效。未使用独立临时 Unity 工程。

测试前后 MainController、Startup、MainWindow、Mcxin.shadervariants、ProjectSettings.asset 的 SHA256 全部一致。未执行 Play Mode、完整 Player / IL2CPP、实际 HybridCLR GenerateAll 或设备验收；Player 失败报告分支完成编译和源码核对，未注入真实 Player 打包失败。SBP 当前兼容入口不真正跳过 DryRun 的 Bundle 生成，因此 BuildManifestOnly 将其隔离到临时目录，正式发布配置含 DryRunBuild 时明确失败。

恢复范围限于各 Utility 当前这一次文件替换，不覆盖后续工具或回调的失败。DLC Utility 返回后，After 失败会令 ToolVersion 保留旧值，但已发布资源保持新版本；重试仍由该 Utility 负责重新生成和替换。Player 生成目录、PlayerSettings、自定义扩展副作用与 HybridCLR GenerateAll 的工程写入不自动恢复。文件系统阻止恢复时抛聚合异常并保留备份；进程崩溃/断电恢复未实现。AppendHashToAssetBundleName 与运行时逻辑名的既有兼容性未纳入本轮。

## 历史全量审查范围与结论

沿用上次 Review 的 `5a6c4e4361d98b1c7541a4f76a64c20bb3f70ca1` 为增量基准，差异命令为 `git diff 5a6c4e4361d98b1c7541a4f76a64c20bb3f70ca1...HEAD`；期间只有 `d20f791 同步文档` 一个提交。该提交修改文档、示例、AGENTS 和包的 Samples 注册，未修改 Runtime、Editor、Module 或 Tests 实现。

本次除检查增量外，重点复核当前 ETask / Ticker、事件、网络缓存、Controller、Window、初始化、资源下载、DLC、Editor 构建、UPM 依赖及 Profiler。历史审查记录 **26 项源码问题：12 项 P1、14 项 P2**。在该审查基线中，上次记录的 5 项问题均仍存在，对应 R06、R07、R08、R09、R13；其余 21 项为该次新识别的历史问题，不能归因于文档提交。当前修复状态以上节和各项标记为准。

P1 表示应优先修复的数据丢失、核心流程失效或发布可靠性问题；P2 表示需排期处理的条件性错误与生命周期问题。编号用于跟踪；Standards、Spec 两轴分别报告，后面的源码清单另按修复优先级组织。历史全量审查阶段仅更新报告；当前已完成顺序 1–3，顺序 4 部分完成。下文保留原故障描述、诊断证据及验收标准，已修复项的历史症状不表示当前仍可复现。

## 历史审查：Standards

依据 [AGENTS.md](AGENTS.md)、[ToolBase.md](Documentation/Editor.Tools/ToolBase.md) 和相关模块文档核对最新差异。新示例遵循 `F.xxx` 访问、等待初始化、Editor 程序集隔离及标准工具入口要求。未发现可行动的规范违规，也未发现有充分证据、且值得独立整改的新增结构性异味。

**本轴：0 项。** 下文历史源码缺陷不计作本次增量的规范违规。

## 历史审查：Spec

仓库没有 `docs/agents/issue-tracker.md`，提交也没有原始 issue 引用，无法进行工单需求覆盖或 scope creep 判断。本次以公开文档与示例作为 API 契约；后续需要按工单审查时，可用 `/setup-matt-pocock-skills` 补齐 issue-tracker 工作流。

**S01：窗口销毁契约与实际行为不一致。** [WindowManager.md:48](Documentation/Runtime/WindowManager.md#L48) 宣称 Destroy 会“销毁窗口实例及其 GameObject”，第 50 行说明“需要彻底释放实例时销毁”。历史基线中，销毁后静态容器仍返回原实例，直接销毁已打开窗口还保留 `IsOpen=true`；新增 [WindowExample.cs:35](Samples~/Basics/WindowExample.cs#L35) 使用 Close→Destroy，也会留下延时清理队列项，随后再次调用 OnDestroy。问题见 R14。当前项目已保留容器和关闭队列修正，Window.Destroy 先置 Alive=false，再调用 Close；重复销毁防护、普通清理异常后的 GameObject 释放及资源 Alive 持有结束已经通过回归，创建中销毁收敛及直接 IWindow 销毁登记仍待处理，S01/R14 保持部分待处理。

Controller 帧分发、批量退出、初始化失败和宿主生命周期等限制已经在当前文档中明确说明，因此不重复记为新增规格偏差。

**本轴：1 项 API 契约偏差，最严重项为 S01；映射到 R14，不额外计入 26 项源码问题。**

## 历史问题清单：P1（当前 11 项已修复、1 项待处理）

### R01 [P1] [已修复] DLC 清理误删应用数据根目录文件

位置：[当前清理实现 DLCUpdater.cs:142](Runtime/Manager/DLCManager/DLCUpdater.cs#L142)、[EasyFrameworkSettings.cs:99](Runtime/Base/ProjectSettings/EasyFrameworkSettings.cs#L99)。验证：真实源码隔离诊断。

`DeleteUnversionedFiles()` 应清理 DLC 缓存，却枚举 `DataPath` 根目录；该路径在 Editor 和大多数 Player 上为 `Application.persistentDataPath`，真正的缓存是其 `DLC` 子目录。初始化成功到达清理时，根目录下不在资源清单中的存档等文件会被删除，DLC 内过期 Bundle 反而保留。临时目录实测：根目录 `player.sav` 被删，`DLC/old.ab` 仍在，`Config` 子目录文件未被遍历。

优化：限定缓存清理在规范化后的 `DLCPath` 内，仅删除框架拥有的旧资源；保护版本元数据，校验清单后再清理。验收：根目录和其他业务目录的哨兵文件保留，当前版本资源和元数据保留，过期 DLC 资源删除。

当前状态：已修复。保留项目原有 `DataPath → DLCPath` 修正，只删除合法版本清单下、DLC 根层的过期 MD5 文件，保护元数据、业务文件、子目录、链接和在途下载；真实 Unity 回归通过，详见顺序 1 执行记录。

### R02 [P1] [已修复] ETask 异常没有传播给 await 调用方

位置：[当前异常保存:120](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L120)、[当前消费与抛出:80](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L80)、[当前泛型结果:212](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L212)。验证：原项目 Unity Edit Mode 回归与独立交错验证。

异常完成只把状态改为 Faulted，没有保存异常；两种 GetResult 都不根据 Faulted 抛错。实测 `async ETask<int>` 内抛出 `InvalidOperationException` 后状态为 Faulted，调用方 GetResult 却返回 `0`；非泛型也正常返回。业务 await 后继续执行，外层 catch 无法恢复，池复用时泛型结果还缺少清理保证。

优化：保存 `ExceptionDispatchInfo`，在 GetResult 传播原异常；明确取消语义和未观察异常策略，回池时清空结果/异常。验收：泛型/非泛型在同步失败、挂起后失败、取消及 Forget 路径上均符合约定，业务 catch 能收到原异常。

当前状态：已修复。以 ExceptionDispatchInfo 保存异常，泛型和非泛型消费时重新抛出；取消映射为 Canceled，Forget 按 EVoidScheduler 规则报告。覆盖真实 async 方法在同步/挂起后失败、取消及 Forget 场景，原项目 Unity 命令行回归通过。

### R03 [P1] [已修复] ETask 完成与 continuation 注册竞争导致永久等待

位置：[当前续体注册:61](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L61)、[当前完成发布:118](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L118)、[当前泛型完成:222](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L222)。验证：真实源码、可控交错诊断。

OnCompleted 预期完成方会设置 sentinel，但 TrySetResult / TrySetException 只调用已有 continuation，从不设置 sentinel。实测顺序 `IsCompleted=false → TrySetResult → OnCompleted(callback)`，callback 不执行。这对应后台任务完成与 await 注册之间的竞争，调用链可能永久挂起。

优化：以同一原子状态机交接注册和完成，结果/异常先于完成状态发布，保证 continuation 恰好执行一次。验收：人为控制“先完成后注册”和“先注册后完成”，成功、失败及取消都能唤醒，再补并发压力验证。

当前状态：已修复。完成方以 CAS 取得唯一完成权，结果/异常先于终态发布，续体通过原子标记交接，回调不持有调度锁；先完成后注册也会唤醒，重复注册被拒绝。可控交错、并发完成与泛型结果发布测试通过。

### R04 [P1] [已修复] ETask 在首次消费前就回池

位置：[当前消费:159](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L159)、[当前状态读取:31](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L31)、[当前最终摘除:183](Runtime/Core/ETask/ETask.TaskAwaiter.cs#L183)。验证：真实源码隔离诊断。

Ticker 移除已完成节点时无条件清空 Token 并回池，不等待首次 GetResult。实测创建 `ETask.DelayFrame(0)`，推进一个 tick，再首次读取结果，Status 显示 Succeeded，GetResult 却抛出 recycled `ArgumentException`。这是首次消费，文档的“不要重复 await”不能规避“先启动、稍后首次 await”的正常用法。

优化：分开调度节点移除与结果消费；结果在首次消费前保持有效，定义未消费任务的释放策略，并统一 token 校验。验收：完成后隔若干 tick 首次 await 仍成功，池复用不影响旧任务，重复消费按明确规则拒绝。

当前状态：已修复。完成后不使结果失效，延后首次 await 仍可消费；每轮由子类独占初始化，最终回池后旧调用不再访问实例。真实池复用、同 tick 重入、旧 token 和单次消费测试通过。

### R05 [P1] [已修复] 一个 tick 回调异常持续阻塞后续节点

位置：[当前逐节点异常隔离:98](Runtime/Core/ETask/Ticker/Ticker.cs#L98)、[当前异常处理:125](Runtime/Core/ETask/Ticker/Ticker.cs#L125)、[当前 Timer/Ticker 分开调度:34](Runtime/Core/ETask/ETask.cs#L34)。验证：真实 Ticker 源码诊断。

Tick 对整个节点列表执行循环，没有逐节点异常隔离；ETask 外层即使记录错误，失败节点仍留在集合。实测先注册持续抛异常的节点，再注册计数节点，连续两个 tick 后计数仍为 `0`。WaitUntil 条件抛错也可触发，影响后续 Delay、等待及队列推进。

优化：按节点隔离异常，让失败异步任务进入 Faulted，普通回调按约定移除或停用；保证移除和资源清理完成。验收：异常节点之后的节点在当次及后续 tick 都继续执行，失败等待不会永久 Pending。

当前状态：已修复。Ticker 逐 Node 隔离 OnTick 异常，任务 tick 异常令对应任务失败，后续节点继续；移除只清理内部 Node，不通知 Target；Timer.Tick 错误不跳过 Ticker.Tick。未扩展 Timer 内部各节点隔离。等待条件抛错、普通节点、同 tick 增删、回收重入及 Timer 错误回归通过。

### R06 [P1] [已修复] Controller 批量退出跳过退出回调

位置：[当前批量退出](Runtime/Manager/ControllerManager/ControllerManager.cs#L228)、[当前退出阶段](Runtime/Manager/ControllerManager/ControllerManager.cs#L218)。下述症状属于历史基线，当前实现见本项状态及 2026-11-04 统一生命周期记录。

ExitAllAsync 在调用 ExitAsync 前 RemoveAt，导致 Controller 的 IsEnter 为 false 并提前返回，跳过监听解绑、OnExit、OnExitAsync 和组件退出。ExitOtherBeforeEnter 复用同一路径。

优化：让 ExitAsync 负责移栈，批量退出使用快照或受控循环，并定义回调内重入的处理方式。验收：两个 Controller 的退出与解绑均各一次，栈最终为空；覆盖异步退出和回调中进入/退出其他 Controller。

当前状态：已修复。ControllerManager 统一移栈和调度退出；批量退出捕获实例与进入轮次，回调中新进入、退出后重入或刷新进入的轮次不被旧批次再次退出。单实例退出首错中断，finally 释放操作保护；批量尝试所有有效快照项后，单个错误保留原异常，多个错误以 AggregateException 向调用方报告。旧的“只记录错误”行为已由 2026-11-04 统一生命周期实现替代，相关真实 Play Mode 回归通过。

### R07 [P1] [已修复] Controller 帧更新列表始终为空

位置：[当前进入提交](Runtime/Manager/ControllerManager/ControllerManager.cs#L180)、[当前帧快照与分发](Runtime/Manager/ControllerManager/ControllerManager.cs#L297)。下述症状属于历史基线，当前实现见本项状态及 2026-11-04 统一生命周期记录。

`_refresh` 默认 false，Enter 和 Remove 从未置 true，OnUpdate 不会填充 `_updateList`。成功进入的 Controller 也收不到 OnUpdate / OnLateUpdate。

优化：栈变化时标脏，使用稳定快照分发，并保持激活状态一致。验收：进入后收到两种帧回调，退出后停止，帧回调内修改栈不导致漏调用或集合异常。

当前状态：已修复。管理器在入栈、刷新进入及移栈后标脏，Update 捕获实例与进入轮次，LateUpdate 复用同帧快照；调用 IController 前检查成员资格、轮次与 UpdateEnabled。已退出的轮次停止分发，帧中新进入或刷新进入的轮次下一帧生效；资格求值及 Update / LateUpdate 回调体的异常均被逐实例记录。组件帧调用及其重入保护由 Controller<T> 自己实现，管理器不依赖组件接口。2026-11-04 追加的 UpdateEnabled getter 隔离缺口已修复，两项故障注入及完整 Play Mode 95/95 通过。

### R08 [P1] [已修复] 批量异步实例化未等待完成就读取 Result

位置：[原生实例化发起及等待](Runtime/Manager/ResLoader/IResLoaderEx.cs#L73)、[结果返回](Runtime/Manager/ResLoader/IResLoaderEx.cs#L83)。下述症状属于历史基线，当前等待路径经本轮源码及真实 Play Mode 复核。

`UNITY_2022_3_OR_NEWER` 分支发起 InstantiateAsync 后，仅在 Editor 且使用 AssetBundleLoader 时 await。Player 和 Editor 资源桥接模式直接读取 Result，不能保证对象已创建，也可能留下调用方未获得的实例。

优化：所有分支统一 await 完成，再做特定 Editor 分支的 Shader 恢复。验收：Editor 桥接、Editor Bundle 和 Player 三种路径返回的数组长度及有效实例数均等于 count；另测失败和 count 边界。

当前状态：已修复。原生 InstantiateAsync 在所有编译分支统一等待，再执行 Editor Bundle Shader 恢复；0 返回空数组，负数在加载前失败。Play Mode 使用真实原生操作验证 5 个实例及父节点；Editor Bundle 和 Player 路径未单独构建运行。

### R09 [P1] UPM 清单缺少无条件 asmdef 依赖

位置：[EasyFramework.asmdef:5](Runtime/EasyFramework.asmdef#L5)、[Profiler.asmdef:4](Module/Profiler/Runtime/EasyFramework.Profiler.asmdef#L4)、[package.json:17](package.json#L17)。状态：上次问题仍在；本次重新对照本机 PackageCache 的原始 asmdef.meta 确认 GUID。

Runtime 无条件引用 TMP、HybridCLR 和 URP，Profiler 也无条件引用 URP；package.json 只声明 Newtonsoft.Json 和 Scriptable Build Pipeline。纯净工程仅添加本包可能缺程序集而无法编译。versionDefines 只控制宏，不会使 references 可选；安装指南说明依赖，并未修复包的自动解析能力。

优化：明确必须依赖并在清单声明；将真正可选的 HybridCLR / URP 集成拆到独立程序集并控制启用，避免核心程序集引用可选包。确定最低支持 Unity 版本。验收：纯净工程按公开安装步骤导入，核心及各可选集成组合均无缺失引用和编译错误。

### R10 [P1] [已修复] RingBuffer 写满容量时把有效数据判为空

位置：[RingBuffer.cs:26](Runtime/Core/Network/RingBuffer.cs#L26)、[Write:61](Runtime/Core/Network/RingBuffer.cs#L61)、[Advance:204](Runtime/Core/Network/RingBuffer.cs#L204)、[扩容判断:261](Runtime/Core/Network/RingBuffer.cs#L261)。验证：真实 RingBuffer 源码诊断。

读写指针相同表示空，但 FreeLength 和扩容判断允许写满整个 Capacity；写满后写指针绕回读指针，Length 变为 0。实测 `Write(new byte[buffer.Capacity])` 后 Length 为 0、TryPeek 失败；GetMemory / Advance 写满也相同。Socket 使用该缓存，满容量边界会静默丢失数据。

优化：维护有效数据计数，或保留一个哨兵空位；统一 Write、Advance、可写区域和扩容的容量语义。验收：容量减一、容量、容量加一、绕回后恰好填满，以及两种写入 API 都保持字节完整和顺序。

当前状态：已修复。有效字节计数区分满/空，全部 Capacity 可用；普通/连续/分段写入、Advance、线性化和扩容遵循统一不变量。30 项真实源码回归及原项目 Unity Edit Mode 完整回归通过，详见顺序 3 执行记录。

### R11 [P1] [已修复] HTTP 失败留下半包，DLC 随后把它当作成功资源

位置：[当前 HTTP 下载:45](Runtime/Core/Extension/HttpClientExtensions.cs#L45)、[当前文件校验:185](Runtime/Manager/DLCManager/DLCUpdater.cs#L185)、[当前按文件合并下载:191](Runtime/Manager/DLCManager/DLCUpdater.cs#L191)、[当前 UnityWebRequest 下载:124](Runtime/Extension/ETask/ETask.UnityWebRequest.cs#L124)。验证：真实下载/DLC 源码隔离诊断，注入故障 HTTP 流。

HttpClient 下载直接写最终文件，流中断后返回 false，但不删除半包。DLC 批量与按需下载只用 File.Exists 判定已完成。实测声明 6 字节、写入 3 字节后中断，最终文件仍存在；后续 DownloadAndReturnFileAsync 直接返回该路径，网络调用次数为 0。

优化：下载到同卷临时文件，完成后校验长度及内容哈希，再提交最终文件；失败/取消清临时文件，保留原有可用文件。已有缓存也需校验，按文件合并并发下载。验收：中断、取消、错误长度/哈希后均不能出现被认可的半包，下一次请求能重新下载。

当前状态：已修复。HTTP 与 UnityWebRequest 使用目标同目录唯一临时文件，完成并校验后原子提交；DLC 验证清单长度和 MD5，并按文件合并批量与按需下载。失败、取消、坏内容或提交失败保留旧文件，后续请求可重试；真实 Unity 回归通过。

### R12 [P1] [已修复] 资源构建失败仍继续生成并发布 DLC 版本

位置：[AssetBundleBuilderUtility.cs:23](Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilderUtility.cs#L23)、[DLCBuilderUtility.cs:124](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L124)、[版本发布:60](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L60)。验证：源码调用链，尚未运行 Unity 故障构建。

AB 构建返回 null 时只打印错误，外层资源构建链继续运行 DLCBuilder，可能把旧输出发布为新版本。BuildDLC 复制文件异常也被捕获后继续写资源索引，调用方仍写版本文件和 LatestVersion；该索引甚至可能包含复制失败的条目。

优化：各阶段显式返回失败或抛出异常并中断父链；全部产物验证成功后再提交索引、LatestVersion 和 StreamingAssets。验收：注入 AB 构建失败、源文件不可读和复制失败，版本不推进、可用底包不覆盖、后续 Player 构建不继续。

当前状态：已修复。AB / DLC 阶段失败直接上抛；各 Utility 独立保护自己的输出，DLC 本次版本元数据、历史裁剪及自动底包复制作为同组文件替换，失败恢复本组旧产物。真实 SBP null Manifest、不可读源、文件锁发布失败及重试回归通过。后续工具或 After 失败中断父链，不隐式撤销之前已完成的工具。Player 报告失败分支为源码和编译验证，完整 Player / IL2CPP 未执行。

## 历史问题清单：P2（当前 9 项已修复、5 项待处理）

### R13 [P2] [已修复] 初始化异常后，后续调用永久等待

位置：[初始化入口](Runtime/F.cs#L61)、[后续调用等待](Runtime/F.cs#L90)。下述症状属于历史基线，当前状态与已接受限制见本项状态及最新顺序 4 复核记录。

状态在模块创建前置 Initializing，任何异常都没有失败状态或复位；后续调用只能等待 Initialized。历史基线中，加载器也可能仅记录失败后正常返回，再叠加当时的 R02，导致 Initialized 不代表 Manifest 有效。

当前状态：原始异常后永久停留 Initializing 的路径已修复。初始化只使用 ETask 和静态 EState；首调用的 try/catch 记录并重新抛出原异常，Initializing 的调用独立等待，恢复时若为 Exception 则抛出新的通用异常。Exception 入口立即自动重试，不保存原异常、没有等待者计数或轮次隔离，不保证重试前的等待者收到旧失败。DLC 失败及无效 Manifest 向上传播；资源字典直接填充，Manifest 只在全部条目成功后设置。当前契约及旧版本验证的适用范围见 F 初始化精简记录。

历史优化与验收目标：用共享状态和原异常表达结果，每位调用者独立等待，不复用只能单次消费的 ETask；同轮调用者收到相同成功或失败，重试不覆盖旧失败。当前用户选定的简化实现以全局状态和等待者通用异常为准，不再承诺原异常共享或旧轮失败隔离。

### R14 [P2] [部分修正，待处理] 窗口销毁没有完成关闭、队列清理和容器失效

位置：[WindowManager.Close](Runtime/Manager/WindowManager/WindowManager.cs#L189)、[管理器身份校验与销毁](Runtime/Manager/WindowManager/WindowManager.cs#L350)、[Window.Destroy](Runtime/Manager/WindowManager/Window.cs#L70)。原 UIWindowBehaviour 逻辑现已合并到 WindowManager；历史诊断使用真实 Window / UIWindowBehaviour 源码。

历史基线中，直接 Destroy 已打开窗口不先 Close，IsOpen 和监听保留；静态类型容器也不移除旧实例。历史诊断实测 GameObject 已清空，IsOpen 仍为 true。即使按示例 Close→Destroy，`_closeList` 仍有条目，超过 KeepAliveTime 后同一窗口再次执行 OnDestroy：实测计数由 1 变为 2。重复 Close 也可制造重复队列项。

优化：销毁作为幂等终态，统一关闭、解绑、队列移除、状态失效、容器移除及持有资源释放；关闭队列去重。验收：直接 Destroy、Close→Destroy、重复调用和超时清理都只释放一次，旧实例不可查询；再次 Open 得到有效新实例。

当前状态：部分修正，仍待处理。Window.Destroy 在入口通过 Alive 防止重复及递归调用，置为 false 后同步调用 Close，再执行窗口和组件 OnDestroy；这些业务回调共用一层 try/catch，首错记录后仍继续清空组件列表及销毁 GameObject。按最新用户约定，不要求逐回调继续，也不主动卸载资源；Alive=false 后，加载器在缓存期限到期且无其他活跃持有者时自动回收资源请求。WindowManager 从类型缓存按层登记分类建立 _updateList / _closedList；关闭移除层登记但保留类型实例，超时经管理器销毁。管理器销毁入口校验实例身份并先删除匹配类型缓存，再移除层关系、调用窗口销毁；旧关闭缓存不能误删同类型新实例。已销毁实例的 Open / OpenAsync 抛出异常，SetActive、Refresh、RefreshOrder 和 Update 直接返回。剩余范围为创建中销毁没有迟到对象回收，以及直接 IWindow.Destroy 不主动撤销管理器登记。最新 Edit Mode 218/218、Play Mode 103/103 通过，已覆盖普通回调异常后的原生对象释放及真实加载器请求回收调度，不代表 R14 已全部关闭。

### R15 [P2] [已修复] 同类型并发 OpenAsync 创建多个窗口对象

位置：[当前 Window.OpenAsync](Runtime/Manager/WindowManager/Window.cs#L113)、[WindowManager.OpenWindowAsync](Runtime/Manager/WindowManager/WindowManager.cs#L286)、[窗口容器](Runtime/Manager/WindowManager/WindowManager.cs#L363)。历史诊断使用当时的 Window.CreateAsync 源码。

容器复用一个 Window，但 CreateAsync 只检查已完成的 WindowObject，没有“创建中”状态。两次调用在第一次创建完成前进入，会分别创建 GameObject，后完成者覆盖引用。实测创建请求 2 次、OnCreate 2 次，第一个对象未被销毁，最终只跟踪第二个。

优化：每个窗口共享创建操作或串行化创建/打开，明确不同参数和目标层并发时的规则；销毁时处理进行中的创建结果。验收：同时打开两次仅一个 GameObject、OnCreate 一次，失败可重试，销毁期间完成的创建没有孤立对象。

当前状态：重复创建问题按用户选定契约已修复。Window.OpenAsync 通过 IsLoading 保护对象创建；直接 IWindow 重叠打开抛出 InvalidOperationException，管理器入口则在修改层登记前记录错误并返回 null，保留原请求。finally 保证加载结束或失败后复位标记；空对象或初始化回调失败由管理器清理，后续打开可新建，成功对象直接复用，不使用共享 Task。窗口参数仍在入口写入，重叠请求可能已修改参数，因此同类型打开应顺序 await。创建中销毁后的收敛仍见 R14。

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

### R18 [P2] [已修复] 事件派发中退订导致漏调用与越界

位置：[Event.cs:49](Runtime/Core/Event/Event.cs#L49)、[Action 分支:65](Runtime/Core/Event/Event.cs#L65)。验证：真实 Event 源码诊断。

派发保存的是原 List 引用与派发前数量。A、B 两个订阅中，A 回调内移除自己，B 前移后被跳过，下一轮索引越界。实测 B 调用次数为 0，记录一次派发错误；IEvent 与 Action 两个分支均有同类逻辑。

优化：使用派发快照或支持重入的延迟增删，明确订阅变化在当前/下一次派发生效的语义。验收：移除自己/他人、新增订阅及嵌套 Invoke 均按约定调用且不越界。

当前状态：已修复。两组订阅表均在回调前捕获；订阅变更不影响当前派发，嵌套和后续 Invoke 使用最新订阅。后续按用户要求改为订阅 List 加分段复用快照，容量预热后避免每次订阅变更与派发的框架内部分配；18 项同步事件回归及 Unity Edit Mode 完整回归通过，详见 Event 后续执行记录。

### R19 [P2] [已修复] SocketChannel.Dispose 不释放未连接的 socket

位置：[SocketChannel.cs:78](Runtime/Core/Network/SocketChannel.cs#L78)、[Dispose:113](Runtime/Core/Network/SocketChannel.cs#L113)。验证：真实源码及真实 .NET Socket 诊断。

Dispose 只调用 Disconnect，后者在 IsConnected=false 时直接返回，socket 本身没有无条件 Dispose。实测 new 后立即 Dispose，native handle 的 IsClosed 仍为 false。连接失败或异步断开后也存在绕过释放的路径。

优化：Dispose 无条件释放 socket，协调在途异步操作和回调，清理保持幂等。验收：未连接、连接失败、已连接及异步断开后均关闭 native handle，重复 Dispose 安全。

当前状态：已修复。Dispose 无条件关闭句柄并终止连接等待；同步与异步完成共同持有事件参数及池化缓冲区，回调退出后统一幂等清理。12 项真实 Socket/loopback 回归及 Unity Edit Mode 完整回归通过。

### R20 [P2] [已修复] 无效发布版本会先清空可用 StreamingAssets

位置：[DLCBuilderUtility.cs:139](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L139)。验证：源码路径。

CopyVersionToStreamingAssets 先 ClearDirectory，到后面才验证源版本目录。releaseVersion 填错或版本已被缓存裁剪时，可用底包资源已经清空，方法只记录错误并返回。

优化：先验证源版本和全部文件，复制到暂存目录后再替换目标；失败传播给父构建链。验收：选择不存在版本或注入复制失败，原目标内容完整保留且构建明确失败。

当前状态：已修复。先校验元数据、UID、文件长度与 MD5，暂存复制并复核后替换目标。无效版本、路径越界、资源缺失/损坏/不可读及发布提交失败均保留原底包；真实文件系统及 Unity 回归通过。

### R21 [P2] [已修复] 构建设置缓存使修改目录后仍打旧资源

位置：[AssetBundleBuilderSettings.cs:15](Editor/Tools/AssetBuilder/AssetBundleBuilder/AssetBundleBuilderSettings.cs#L15)、[ToolExtensionObjectPool.cs:24](Editor/Tools/ToolExtensionObjectPool.cs#L24)、[ToolBase.cs:72](Editor/Tools/ToolBase.cs#L72)。验证：源码缓存路径。

AssetBundleBuildSettingExtension 构造时快照 buildDirectories，普通扩展对象又永久缓存；ToolBase.Refresh 只刷新执行事件，不刷新资源设置扩展。首次读取设置或构建后，再修改目录仍用旧数组，新配置资产也可能未被发现。

优化：设置通过实时 getter 读取，每次构建前刷新需要重新发现的配置扩展；避免把配置值作为永久实例状态。验收：同一 Editor 会话先构建 A，再改为 B 并新增配置 C，下一次产物仅包含新配置要求的资源。

当前状态：已修复。默认扩展通过 getter 读取当前目录，每次配置收集强制重扫配置资产；同会话新增/修改/禁用/删除均生效。6 项配置测试及真实 SBP A→B 产物重建/加载回归通过。

### R22 [P2] [已修复] DLC 扩展 Before / After 每次执行两遍

位置：[DLCBuilderUtility.cs:36](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L36)、[After:78](Editor/Tools/AssetBuilder/DLCBuilder/DLCBuilderUtility.cs#L78)、[ToolBase.cs:62](Editor/Tools/ToolBase.cs#L62)。验证：源码调用链。

ToolBase.Execute 已分发 Before/Execute/After，BuildBySettings 又分发同一组 Before/After。默认 StreamingAssets 复制因此运行两次，自定义生成、清理或发布扩展也可能重复执行，内层 After 还可能早于其他扩展的 Execute。

优化：生命周期只由 ToolBase 管理，具体 Utility 只负责构建工作。验收：直接执行及经 AssetBuilder 调用时，记录扩展的顺序均为 Before→Execute→After，各一次。

当前状态：已修复。移除 DLC Utility 内部生命周期派发和工具 After 的重复底包复制入口，改为 Utility 内一次显式发布。直接工具与 AssetBuilder 使用的同一父接口入口各一次，Utility 零次；事件失败中断后续，本工具版本保持原值，已完成发布的产物不自动撤销。

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
| 2 | 已完成 | 稳定 ETask 与调度基础 | R02–R05 | 统一结果、异常、取消、continuation 和消费/回池协议；逐节点隔离 tick 异常；写清单次消费语义 | 真实 TaskAwaiter 回归覆盖异常、竞争、延后首次消费、回池复用及失败节点隔离 |
| 3 | 已完成 | 修复网络与事件边界 | R10、R18、R19 | 统一 RingBuffer 满/空不变量、写入与扩容规则；事件重入策略；socket 无条件幂等释放 | 满容量/绕回字节测试、订阅变更测试及 native handle 释放测试通过 |
| 4 | 已完成（当前约定范围） | 统一初始化和业务生命周期 | R06–R08、R13–R15 | 初始化、Controller、窗口加载与即时排序约定已通过；Alive、重复销毁、异常后的原生对象释放及资源持有结束已验证；追加 P2/P3 已修复；R14 迟到对象及直接销毁登记明确暂缓，不阻塞本阶段关闭 | 最新 Edit Mode 218/218、Play Mode 103/103 通过；真实 AssetBundle 在途卸载、R14 暂缓范围及 Player / IL2CPP 未验收 |
| 5 | 已完成 | 让构建失败可见且可恢复 | R12、R20–R22 | 各 BuilderUtility 独立暂存、校验、发布及本次替换失败恢复；ToolBase 单次生命周期与自身版本；配置实时读取和重扫 | 最新完整 Edit Mode 265/265；真实 SBP 构建/失败、文件锁、元数据发布保护和同会话配置更新通过；工具调用之间无隐式事务；完整 Player / IL2CPP 未执行 |
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

历史全量审查当时未执行 Unity Edit Mode / Play Mode 测试、纯净工程导入、目标平台 Player 构建、HybridCLR 热更新或实际 CDN 更新。当前顺序 1 已完成真实 Unity Edit Mode 验证：首次交互式 TestRunnerApi 67/67、virtual 可读性整理后原项目命令行 67/67，以及最终合并 FileHelper 后原项目命令行 67/67；顺序 2 修复后原项目命令行 115/115，可读性精简后对同一原项目命令行 125/125，枚举状态整理后同一原项目命令行 128/128，以上均为 0 失败、0 跳过；Play Mode、纯净导入、目标平台 Player / WebGL 构建、HybridCLR 热更新和实际 CDN 更新仍未验证。隔离诊断验证状态/文件/缓冲区逻辑，不验证 Unity 主线程调度、原生对象或目标平台兼容性。Editor 和 Profiler 中标为源码路径的结论仍需要对应的 Unity 故障注入与场景验收。

## 历史记录

2026-10-31 的 Review 记录了本次保留的 5 项问题，以及修正文档接口、发布步骤、Roslyn 约束、FBehaviour 说明并新增基础示例等工作。此前记录的示例编译、Controller 隔离诊断及官网构建/Playwright 验证属于上次结果，本轮未重跑，不作为当前修复验收的证据。

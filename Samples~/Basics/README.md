# 基础示例

在 Unity Package Manager 中选择 Easy Framework，在 Samples 中导入“基础示例”。也可以将本目录复制到业务工程 Assets 下。

1. 在空场景创建一个 GameObject，每种示例组件最多挂载一份。
2. 挂载 EventExample、ControllerExample，运行后查看事件、进入与退出日志。
3. 挂载 WindowExample，运行后看到半透明绿色面板；它通过重写创建方法演示窗口，不需要预制体或 Roslyn 生成代码。
4. 资源示例需要先将名为 Cube 的预制体保存到 AssetBundleBuilder 的构建目录（默认 Assets/Res_DLC），再挂载 ResourceExample。
5. Editor 默认使用 resLoaderEditorMode，无须先打包。Player 运行前按 Documentation/GettingStarted.md 配置 AppSettings、DLC 与资源构建。

ResourceExample 用 IResRequest 持有资源，处理异步加载中销毁，并在不再使用时销毁实例、释放同一个请求。事件组件在 OnDisable 解除订阅。Controller 示例通过实例进入和退出，不依赖当前有问题的管理器帧分发或 ExitAllAsync。

Editor/BuildExample 提供菜单 EasyFramework/Samples/Build Player。执行前检查构建目标、启用场景、AppSettings 和 DLCBuilder 的 StreamingAssets 复制设置；默认 PlayerBuilder 会先构建资源。

示例运行需要已解析框架 asmdef 引用的 TMP、URP、HybridCLR，以及 Unity UI（包名 com.unity.ugui，程序集 UnityEngine.UI），详见 Documentation/GettingStarted.md 的安装前提。本包未声明最低 Unity 版本，需在目标版本导入验证。

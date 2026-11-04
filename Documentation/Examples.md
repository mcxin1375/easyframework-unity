# 基础示例

包内提供可导入的 Samples~/Basics，代码同时用于官网示例。先阅读 [快速开始](GettingStarted.md)，再按以下步骤运行。

## 导入和运行

通过 Package Manager 的 Samples 导入“基础示例”，或复制 Samples~/Basics 到 Assets。创建空场景和 GameObject；每种组件最多挂载一份。EventExample、ControllerExample、WindowExample 无需业务预制体；ResourceExample 需要构建目录中的 Cube.prefab。窗口示例重写创建方法显示面板，正常 Window 的默认规则仍是按类型名加载预制体。

示例 asmdef 引用 EasyFramework、UnityEngine.UI；Editor 程序集单独引用 EasyFramework.Editor。示例不使用 UI / 参数代码生成，方便检查完整实现。

## 资源持有与异步清理

源码：[ResourceExample.cs](../Samples~/Basics/ResourceExample.cs)。

```csharp
using EasyFramework;
using UnityEngine;

namespace EasyFramework.Samples.Basics
{
    // Prepare Cube.prefab in an AssetBundleBuilder build directory.
    public sealed class ResourceExample : MonoBehaviour, IResRequest
    {
        public bool Alive => !_destroyed;
        private GameObject _cube;
        private bool _destroyed;
        private bool _loading;
        private bool _requested;

        private void Start() => LoadAsync().Forget();

        private async ETask LoadAsync()
        {
            await F.InitializeAsync();
            if (_destroyed) return;

            _loading = true;
            _requested = true;
            try
            {
                var instance = await F.ResLoader.CreateObjAsync("Cube", request: this);
                if (_destroyed)
                {
                    if (instance != null) Destroy(instance);
                }
                else
                {
                    _cube = instance;
                }
            }
            finally
            {
                _loading = false;
                if (_destroyed) Release();
            }
        }

        private void OnDestroy()
        {
            _destroyed = true;
            if (!_loading) Release();
        }

        private void Release()
        {
            if (_cube != null) Destroy(_cube);
            _cube = null;
            if (!_requested) return;
            _requested = false;
            F.ResLoader.Unload("Cube", this);
        }
    }
}
```

## 窗口创建与关闭

源码：[WindowExample.cs](../Samples~/Basics/WindowExample.cs)。

```csharp
using EasyFramework;
using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework.Samples.Basics
{
    public sealed class WindowExample : MonoBehaviour
    {
        private ExampleWindow _window;
        private bool _destroyed;

        private void Start() => OpenAsync().Forget();

        private async ETask OpenAsync()
        {
            await F.InitializeAsync();
            if (_destroyed) return;

            var window = await F.WindowManager.OpenAsync<ExampleWindow>(UILayer.HUD);
            if (_destroyed)
            {
                Release(window);
                return;
            }
            _window = window;
        }

        private void OnDestroy()
        {
            _destroyed = true;
            if (_window != null) Release(_window);
            _window = null;
        }

        private static void Release(ExampleWindow window)
        {
            F.WindowManager.Close(window);
            F.WindowManager.Destroy(window);
        }
    }

    // This demo creates its own panel. A normal Window loads a prefab by type name.
    public sealed class ExampleWindow : Window
    {
        protected override GameObject CreateWindowObject(Transform parent)
        {
            var panel = new GameObject("ExampleWindow", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.15f, 0.55f, 0.4f, 0.4f);
            return panel;
        }

        protected override ETask<GameObject> CreateWindowObjectAsync(Transform parent)
            => ETask.FromResult(CreateWindowObject(parent));

        protected override void OnOpen() => Debug.Log("ExampleWindow opened");
        protected override void OnClose() => Debug.Log("ExampleWindow closed");
    }
}
```

## 事件订阅与解绑

源码：[EventExample.cs](../Samples~/Basics/EventExample.cs)。

```csharp
using EasyFramework;
using UnityEngine;

namespace EasyFramework.Samples.Basics
{
    public readonly struct LoginCompleted { }

    public sealed class EventExample : MonoBehaviour
    {
        private void OnEnable() => F.Event.Add<LoginCompleted>(OnLogin);
        private void OnDisable() => F.Event.Remove<LoginCompleted>(OnLogin);
        private void Start() => F.Event.Invoke(new LoginCompleted());
        private void OnLogin(LoginCompleted args) => Debug.Log("LoginCompleted received");
    }
}
```

## Controller 进入与退出

源码：[ControllerExample.cs](../Samples~/Basics/ControllerExample.cs)。

```csharp
using EasyFramework;
using UnityEngine;

namespace EasyFramework.Samples.Basics
{
    public sealed class ExampleController : Controller<ExampleController>
    {
        protected override void OnEnter() => Debug.Log("ExampleController entered");
        protected override void OnExit() => Debug.Log("ExampleController exited");
    }

    public sealed class ControllerExample : MonoBehaviour
    {
        private bool _destroyed;

        private void Start() => RunAsync().Forget();
        private void OnDestroy() => _destroyed = true;

        private async ETask RunAsync()
        {
            await F.InitializeAsync();
            if (_destroyed) return;

            var controller = ExampleController.Instance;
            try
            {
                await controller.EnterAsync();
                Debug.Log($"Controller active: {F.ControllerManager.HasActive(controller)}");
            }
            finally
            {
                await controller.ExitAsync();
            }
        }
    }
}
```

## Editor 发布入口

源码：[Editor/BuildExample.cs](../Samples~/Basics/Editor/BuildExample.cs)。

```csharp
using EasyFramework.Editor;
using UnityEditor;

namespace EasyFramework.Samples.Basics.Editor
{
    public static class BuildExample
    {
        [MenuItem("EasyFramework/Samples/Build Player")]
        public static void Execute()
        {
            // Default PlayerBuilder.assetBuilder = true runs the resource chain first.
            // Enable DLCBuilder.copyVersionToStreamingAssets for an offline Player.
            PlayerBuilder.Instance.Execute();
        }
    }
}
```

## 使用范围

基础示例演示 API 与清理模式；发布前仍需配置 AppSettings、目标场景与 DLC 复制，检查资源构建与平台输出。Controller 示例通过 Instance.EnterAsync 进入，并在 finally 中等待 ExitAsync 退出；基类只转发到管理器，退出后实例继续缓存在会话中，不提供公开 DestroyAsync。生命周期清理首错中断，业务应处理自身回调异常。MonoBehaviour 的销毁不会自动取消 ETask，资源与窗口示例显式处理 await 后已销毁的情况。

using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework.Samples.Modules
{
    // 在资源构建目录准备以 RectTransform 为根的 TemplateWindow.prefab。
    // 主线程顺序等待 OpenAsync；Close 默认缓存 60 秒，Release 必须在打开结束后调用以销毁实例。
    public sealed class TemplateWindow : Window
    {
        protected override void OnOpen() => Debug.Log("窗口已打开");
        protected override void OnClose() => Debug.Log("窗口已关闭");
    }

    public static class WindowExample
    {
        public static async ETask<TemplateWindow> OpenAsync()
        {
            await F.InitializeAsync();
            return await F.WindowManager.OpenAsync<TemplateWindow>(UILayer.HUD);
        }

        public static void Close() => F.WindowManager.Close<TemplateWindow>();

        public static void Release()
        {
            F.WindowManager.Close<TemplateWindow>();
            F.WindowManager.Destroy<TemplateWindow>();
        }
    }

    // 自动绑定与组件：WindowUIExampleWindow.prefab 子节点名须与字段完全一致，且在 UI 树中唯一。
    public sealed class WindowUIExampleComponent : WindowComponent<WindowUIExampleWindow>
    {
        public Text StatusText;
        public Button ConfirmButton;
        public Image StatusImage;
        public int UpdateCount { get; private set; }

        protected override void OnLoadObject()
        {
            if (StatusText != null) StatusText.text = "Ready";
            if (StatusImage != null) StatusImage.color = Color.green;
        }

        protected override void OnOpen() => FDebug.Log("Window component open");
        protected override void OnClose() => FDebug.Log("Window component close");
        protected override void OnSetActive(bool value) => FDebug.Log($"Window component active={value}");
        protected override void OnRefreshOrderInLayer(int orderInLayer) => FDebug.Log($"Window sort order={orderInLayer}");
        protected override void OnUpdate() => UpdateCount++;
        protected override void OnDestroy()
        {
            StatusText = null;
            ConfirmButton = null;
            StatusImage = null;
        }

        protected override void OnAddListeners()
        {
            if (ConfirmButton != null) ConfirmButton.onClick.AddListener(OnConfirm);
        }

        protected override void OnRemoveListeners()
        {
            if (ConfirmButton != null) ConfirmButton.onClick.RemoveListener(OnConfirm);
        }

        private void OnConfirm()
        {
            if (StatusText != null) StatusText.text = "Confirmed";
            FDebug.Log("Window component button clicked.");
        }
    }

    // IWindowUI<T> 由框架 Roslyn 生成 UI 属性及同名组件自动绑定入口。
    public partial class WindowUIExampleWindow : Window, IWindowUI<WindowUIExampleComponent>
    {
        protected override void OnOpen()
        {
            if (UI.StatusText != null) UI.StatusText.text = "Opened";
        }

        protected override void OnButtonClick(Button button)
        {
            // Window 已统一绑定子节点 Button；通用路由无需再手动注册 onClick。
            if (button.name == "CloseButton") Close();
        }

        protected override void OnRefresh()
        {
            if (UI.StatusImage != null) UI.StatusImage.enabled = IsOpen;
        }
    }

    public static class WindowUIExample
    {
        public static async ETask<WindowUIExampleWindow> OpenAsync()
        {
            await F.InitializeAsync();
            return await F.WindowManager.OpenAsync<WindowUIExampleWindow>(UILayer.Popup);
        }

        public static void Release()
        {
            F.WindowManager.Close<WindowUIExampleWindow>();
            F.WindowManager.Destroy<WindowUIExampleWindow>();
        }
    }

    // 常见组合：同一窗口既有自动绑定 UI，也通过一个业务参数结构体接收数据。
    public readonly struct UiWindowArgs
    {
        public readonly string Title;
        public readonly int Score;
        public UiWindowArgs(string title, int score) { Title = title; Score = score; }
    }

    public sealed class ParameterizedWindowUI : WindowComponent<ParameterizedUIWindowExample>
    {
        public Text TitleText;
        public Button CloseButton;
    }

    public partial class ParameterizedUIWindowExample : Window, IWindowUI<ParameterizedWindowUI>, ITParams<UiWindowArgs>
    {
        protected override void OnRefresh()
        {
            if (UI.TitleText != null) UI.TitleText.text = $"{T1.Title}: {T1.Score}";
        }

        protected override void OnButtonClick(Button button)
        {
            if (button == UI.CloseButton) Close();
        }
    }

    public static class CombinedWindowExample
    {
        public static async ETask<ParameterizedUIWindowExample> OpenAsync(string title, int score)
        {
            await F.InitializeAsync();
            return await F.WindowManager.OpenAsync<ParameterizedUIWindowExample, UiWindowArgs>(
                UILayer.Popup, new UiWindowArgs(title, score));
        }
    }

    // 一至三个参数：泛型同步/异步与 Type 入口使用同样的参数顺序。
    public readonly struct WindowOneArgs
    {
        public readonly int Value;
        public WindowOneArgs(int value) => Value = value;
    }

    public partial class WindowOneParamExample : Window, ITParams<WindowOneArgs>
    {
        protected override void OnOpen() => FDebug.Log($"Window arg: {T1.Value}");
    }

    public partial class WindowTwoParamExample : Window, ITParams<WindowOneArgs, string>
    {
        protected override void OnOpen() => FDebug.Log($"Window args: {T1.Value}, {T2}");
    }

    public partial class WindowThreeParamExample : Window, ITParams<WindowOneArgs, string, bool>
    {
        protected override void OnOpen() => FDebug.Log($"Window args: {T1.Value}, {T2}, {T3}");
    }

    // ITParams<T> 成员由包内 Roslyn 生成器补齐；每种参数类型准备同名窗口 prefab。
    // 所有方法都在 Unity 主线程、await F.InitializeAsync() 之后调用；同类型打开应顺序等待。
    public static class WindowParamsExample
    {
        public static WindowOneParamExample OpenOneSync(int value)
            => F.WindowManager.Open<WindowOneParamExample, WindowOneArgs>(UILayer.HUD, new WindowOneArgs(value));

        public static WindowTwoParamExample OpenTwoSync(int value, string text)
            => F.WindowManager.Open<WindowTwoParamExample, WindowOneArgs, string>(UILayer.HUD, new WindowOneArgs(value), text);

        public static WindowThreeParamExample OpenThreeSync(int value, string text, bool active)
            => F.WindowManager.Open<WindowThreeParamExample, WindowOneArgs, string, bool>(UILayer.HUD, new WindowOneArgs(value), text, active);

        public static ETask<WindowOneParamExample> OpenOneAsync(int value)
            => F.WindowManager.OpenAsync<WindowOneParamExample, WindowOneArgs>(UILayer.HUD, new WindowOneArgs(value));

        public static ETask<WindowTwoParamExample> OpenTwoAsync(int value, string text)
            => F.WindowManager.OpenAsync<WindowTwoParamExample, WindowOneArgs, string>(UILayer.HUD, new WindowOneArgs(value), text);

        public static ETask<WindowThreeParamExample> OpenThreeAsync(int value, string text, bool active)
            => F.WindowManager.OpenAsync<WindowThreeParamExample, WindowOneArgs, string, bool>(UILayer.HUD, new WindowOneArgs(value), text, active);

        public static WindowOneParamExample OpenByTypeSync(int value)
            => F.WindowManager.Open(typeof(WindowOneParamExample), UILayer.Popup,
                new object[] { new WindowOneArgs(value) }) as WindowOneParamExample;

        public static async ETask<WindowThreeParamExample> OpenByTypeAsync(int value, string text, bool active)
        {
            var window = await F.WindowManager.OpenAsync(typeof(WindowThreeParamExample), UILayer.Popup,
                new object[] { new WindowOneArgs(value), text, active });
            return window as WindowThreeParamExample;
        }
    }

    // Resources 默认按类型名加载：Resources/DefaultResourcesWindowExample.prefab。
    public sealed class DefaultResourcesWindowExample : WindowResources { }

    // ResourcePath 指定 Resources 下的相对路径。
    public sealed class PreferencesWindowExample : WindowResources
    {
        protected override string ResourcePath => "Windows/PreferencesWindow";
        protected override void OnOpen() => FDebug.Log("Preferences window opened.");
    }

    public static class WindowResourcesExample
    {
        public static async ETask<DefaultResourcesWindowExample> OpenDefaultAsync()
        {
            await F.InitializeAsync();
            return await F.WindowManager.OpenAsync<DefaultResourcesWindowExample>(UILayer.Popup);
        }

        public static async ETask<PreferencesWindowExample> OpenAsync()
        {
            await F.InitializeAsync();
            return await F.WindowManager.OpenAsync<PreferencesWindowExample>(UILayer.Popup);
        }
    }

    public static class WindowManagerExample
    {
        // 除 OpenAsync 外，其余入口要求业务已等待 F.InitializeAsync()。
        public static TemplateWindow OpenSync() => F.WindowManager.Open<TemplateWindow>(UILayer.HUD);
        public static async ETask<TemplateWindow> OpenAsync()
        {
            await F.InitializeAsync();
            return await F.WindowManager.OpenAsync<TemplateWindow>(UILayer.HUD);
        }

        public static void Inspect()
        {
            var manager = F.WindowManager;
            var root = manager.UIRoot;
            var eventSystem = manager.EventSystem;
            var resolution = manager.Resolution;
            bool inputEnabled = manager.EventSystemEnabled;
            var current = manager.GetWindow<TemplateWindow>();
            var byType = manager.GetWindow(typeof(TemplateWindow));
            bool isOpen = manager.IsOpen<TemplateWindow>();
            var top = manager.GetTopWindow(UILayer.HUD);
            var windows = manager.GetWindows(UILayer.HUD);
            var layer = manager.GetLayerData(UILayer.HUD);
            var camera = manager.GetCamera(UILayer.HUD);
            var canvas = manager.GetCanvas(UILayer.HUD);
            Debug.Log($"Root={root}, EventSystem={eventSystem}, Resolution={resolution}, Input={inputEnabled}");
            Debug.Log($"Window={current}, ByType={byType}, Open={isOpen}, Top={top}, Count={windows.Length}, Layer={layer}, Camera={camera}, Canvas={canvas}");
        }

        // 调用方应保存并在流程结束后恢复共享 UI 设置。
        public static void ConfigureUI(Vector2 resolution, bool eventSystemEnabled)
        {
            F.WindowManager.Resolution = resolution;
            F.WindowManager.EventSystemEnabled = eventSystemEnabled;
        }

        public static void RefreshAndSetActive(TemplateWindow window, bool visible)
        {
            if (window == null || !window.Alive) return;
            window.Refresh();
            window.SetActive(visible);
            // SetActive 只切换显示状态，IsOpen 的管理器登记保留；结束业务仍用 Close/Destroy。
        }

        public static void CloseInstance(IWindow window) => F.WindowManager.Close(window);
        public static void CloseByType() => F.WindowManager.Close<TemplateWindow>();
        // 层和全局操作只在业务明确需要影响该范围内所有窗口时调用。
        public static void CloseLayer() => F.WindowManager.CloseLayer(UILayer.Popup);
        public static void CloseAll() => F.WindowManager.CloseAll();
        public static void RefreshAll() => F.WindowManager.RefreshAllWindow();
        public static void DestroyInstance(IWindow window) => F.WindowManager.Destroy(window);
        public static void DestroyByType() => F.WindowManager.Destroy<TemplateWindow>();
    }
}

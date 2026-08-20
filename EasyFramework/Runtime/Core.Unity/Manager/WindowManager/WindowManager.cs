/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:UI管理类
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyFramework
{
    internal class WindowManager : Singleton<WindowManager>, IWindowManager
    {
        public GameObject UIRoot => UIRootBehaviour.gameObject;
        public EventSystem EventSystem => UIRootBehaviour.EventSystem;
        public Vector2 Resolution
        {
            get => UIRootBehaviour.Resolution;
            set => UIRootBehaviour.Resolution = value;
        }
        public bool EventSystemEnabled
        {
            get => EventSystem.enabled;
            set => EventSystem.enabled = value;
        }

        internal readonly UIWindowBehaviour UIWindowBehaviour;
        private UIRootBehaviour UIRootBehaviour => UIRootBehaviour.Instance;

        public WindowManager()
        {
            UIWindowBehaviour = UIRootBehaviour.gameObject.AddComponentEx<UIWindowBehaviour>();
        }

        public T Open<T>(UILayer uiLayer = UILayer.HUD) where T : class, IWindow, new()
        {
            FDebug.Log($"F.UISystem.Open(type: {typeof(T).Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow<T>();
            if (window is ITParams t) t.SetParamsDefault();

            UIWindowBehaviour.OpenWindow(window, uiLayer);
            return window;
        }
        public T Open<T, TK1>(UILayer uiLayer, in TK1 tk1) where T : class, IWindow, ITParams<TK1>, new()
        {
            FDebug.Log($"F.UISystem.Open(type: {typeof(T).Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow<T>();
            window.SetParams(in tk1);

            UIWindowBehaviour.OpenWindow(window, uiLayer);
            return window;
        }
        public T Open<T, TK1, TK2>(UILayer uiLayer, in TK1 tk1, in TK2 tk2) where T : class, IWindow, ITParams<TK1, TK2>, new()
        {
            FDebug.Log($"F.UISystem.Open(type: {typeof(T).Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow<T>();
            window.SetParams(in tk1, in tk2);

            UIWindowBehaviour.OpenWindow(window, uiLayer);
            return window;
        }
        public T Open<T, TK1, TK2, TK3>(UILayer uiLayer, in TK1 tk1, in TK2 tk2, in TK3 tk3) where T : class, IWindow, ITParams<TK1, TK2, TK3>, new()
        {
            FDebug.Log($"F.UISystem.Open(type: {typeof(T).Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow<T>();
            window.SetParams(in tk1, in tk2, in tk3);

            UIWindowBehaviour.OpenWindow(window, uiLayer);
            return window;
        }
        public IWindow Open(Type type, UILayer uiLayer, object[] tParams = null)
        {
            FDebug.Log($"F.UISystem.Open(type: {type.Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow(type);
            if (window is ITParams t) t.SetParams(tParams);

            UIWindowBehaviour.OpenWindow(window, uiLayer);
            return window;
        }

        public ETask<T> OpenAsync<T>(UILayer uiLayer = UILayer.HUD) where T : class, IWindow, new()
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {typeof(T).Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow<T>();
            if (window is ITParams t) t.SetParamsDefault();
            
            return UIWindowBehaviour.OpenWindowAsync(window, uiLayer);
        }
        public ETask<T> OpenAsync<T, TK1>(UILayer uiLayer, in TK1 tk1) where T : class, IWindow, ITParams<TK1>, new()
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {typeof(T).Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow<T>();
            window.SetParams(in tk1);
            
            return UIWindowBehaviour.OpenWindowAsync(window, uiLayer);
        }
        public ETask<T> OpenAsync<T, TK1, TK2>(UILayer uiLayer, in TK1 tk1, in TK2 tk2) where T : class, IWindow, ITParams<TK1, TK2>, new()
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {typeof(T).Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow<T>();
            window.SetParams(in tk1, in tk2);
            
            return UIWindowBehaviour.OpenWindowAsync(window, uiLayer);
        }
        public ETask<T> OpenAsync<T, TK1, TK2, TK3>(UILayer uiLayer, in TK1 tk1, in TK2 tk2, in TK3 tk3) where T : class, IWindow, ITParams<TK1, TK2, TK3>, new()
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {typeof(T).Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow<T>();
            window.SetParams(in tk1, in tk2, in tk3);

            return UIWindowBehaviour.OpenWindowAsync(window, uiLayer);
        }

        public ETask<IWindow> OpenAsync(Type type, UILayer uiLayer, object[] tParams = null)
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {type.Name}), uiLayer: {uiLayer}");
            
            var window = WindowContainer.GetOrCreateWindow(type);
            if (window is ITParams t) t.SetParams(tParams);

            return UIWindowBehaviour.OpenWindowAsync(window, uiLayer);
        }

        public void Close(IWindow window)
        {
            Debug.Log($"F.WindowManager.Close(type:{window.GetType().Name})");
            UIWindowBehaviour.Close(window);
        }
        public async ETask CloseAsync(IWindow window)
        {
            await window.BeforeCloseAsync();
            Close(window);
        }

        public void Close<T>() where T : class, IWindow, new()
        {
            var window = WindowContainer.GetWindow<T>();
            if (window == null) return;
            
            Close(window);
        }

        public async ETask CloseAsync<T>() where T : class, IWindow, new()
        {
            var window = WindowContainer.GetWindow<T>();
            if (window == null) return;
            
            await window.BeforeCloseAsync();
            Close(window);
        }
        
        public void CloseLayer(UILayer uiLayer)
        {
            FDebug.Log($"F.WindowManager.CloseLayer(uiLayer: {uiLayer})");
            UIWindowBehaviour.CloseLayer(uiLayer);
        }

        public void CloseAll()
        {
            FDebug.Log($"F.WindowManager.CloseAll()");
            UIWindowBehaviour.CloseAll();
        }

        public void Destroy(IWindow window)
        {
            Debug.Log($"F.WindowManager.Destroy(type:{window.GetType().Name})");
            UIWindowBehaviour.Destroy(window);
        }
        public void Destroy<T>() where T : class, IWindow, new()
        {
            var window = WindowContainer.GetWindow<T>();
            if (window == null) return;
            
            Destroy(window);
        }
        
        public void RefreshAllWindow() => UIWindowBehaviour.RefreshAllWindow();
        public T GetWindow<T>() where T : class, IWindow => WindowContainer.GetWindow<T>();
        public IWindow GetWindow(Type type) => WindowContainer.GetWindow(type);
        public IWindow[] GetWindows(UILayer uiLayer) => UIWindowBehaviour.GetWindows(uiLayer);
        
        public UILayerBehaviour GetLayerData(UILayer uiLayer) => UIRootBehaviour.GetLayerData(uiLayer);
        public Camera GetCamera(UILayer uiLayer) => UIRootBehaviour.GetCamera(uiLayer);
        public Canvas GetCanvas(UILayer uiLayer) => UIRootBehaviour.GetCanvas(uiLayer);
        
        public bool IsOpen<T>() where T : class, IWindow => GetWindow(typeof(T))?.IsOpen ?? false;
        public IWindow GetTopWindow(UILayer uiLayer)
        {
            var windows = GetWindows(uiLayer);
            return windows.Length > 0 ? windows[^1] : null;
        }
    }
}
/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:UI管理类
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyFramework
{
    /*
     * 同一时间只支持打开同一个Window
     * 已经打开的Window多次调用Open，返回的是同一实例
     * Window异步打开的情况下，需要await完成时机，在Loading期间再次调用Open，将会报错
     */
    internal class WindowManager : Singleton<WindowManager>, IWindowManager, IFrameworkTick
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

        private UIRootBehaviour UIRootBehaviour => UIRootBehaviour.Instance;
        private readonly Dictionary<Type, IWindow> _windowDict = new();
        private readonly Dictionary<IWindow, UILayer> _windowLayerDict = new();
        private readonly Dictionary<UILayer, WindowLayerInfo> _uiLayerWindowDict = new();
        private readonly List<IWindow> _updateList = new();
        private readonly List<IWindow> _windowTempList = new();
        private readonly List<IWindow> _closedList = new();
        private bool _needRefresh;
        public IReadOnlyDictionary<UILayer, WindowLayerInfo> UILayerWindowDict => _uiLayerWindowDict;

        protected override void OnCreate()
        {
            foreach (var layer in UIRootBehaviour.UILayers)
                _uiLayerWindowDict.Add(layer.Layer, new WindowLayerInfo(layer));
            FBehaviour.Instance.Add(this);
        }

        protected override void OnDestroy() => FBehaviour.Instance?.Remove(this);

        void IFrameworkTick.OnUpdate()
        {
            try
            {
                if (_needRefresh)
                {
                    _needRefresh = false;
                    _updateList.Clear();
                    _closedList.Clear();
                    foreach (var window in _windowDict.Values)
                    {
                        if (_windowLayerDict.ContainsKey(window))
                        {
                            _updateList.Add(window);
                        }
                        else
                        {
                            _closedList.Add(window);
                        }
                    }
                }
                foreach (var window in _updateList)
                {
                    if (!_windowLayerDict.ContainsKey(window) || window.IsLoading) continue;
                    window.Update();
                }

                for (int i = _closedList.Count - 1; i >= 0; i--)
                {
                    var window = _closedList[i];
                    if (!window.KeepAlive)
                    {
                        _closedList.RemoveAt(i);
                        DestroyWindow(window);
                    }
                }
            }
            catch (Exception exception) { FDebug.LogException(exception); }
        }

        void IFrameworkTick.OnLateUpdate() { }
        void IFrameworkTick.OnDestroy() => OnDestroy();

        public T Open<T>(UILayer uiLayer = UILayer.HUD) where T : class, IWindow, new()
        {
            FDebug.Log($"F.UISystem.Open(type: {typeof(T).Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow<T>();
            if (window is ITParams t) t.SetParamsDefault();

            return OpenWindow(window, uiLayer) ? window : null;
        }
        public T Open<T, TK1>(UILayer uiLayer, in TK1 tk1) where T : class, IWindow, ITParams<TK1>, new()
        {
            FDebug.Log($"F.UISystem.Open(type: {typeof(T).Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow<T>();
            window.SetParams(in tk1);

            return OpenWindow(window, uiLayer) ? window : null;
        }
        public T Open<T, TK1, TK2>(UILayer uiLayer, in TK1 tk1, in TK2 tk2) where T : class, IWindow, ITParams<TK1, TK2>, new()
        {
            FDebug.Log($"F.UISystem.Open(type: {typeof(T).Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow<T>();
            window.SetParams(in tk1, in tk2);

            return OpenWindow(window, uiLayer) ? window : null;
        }
        public T Open<T, TK1, TK2, TK3>(UILayer uiLayer, in TK1 tk1, in TK2 tk2, in TK3 tk3) where T : class, IWindow, ITParams<TK1, TK2, TK3>, new()
        {
            FDebug.Log($"F.UISystem.Open(type: {typeof(T).Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow<T>();
            window.SetParams(in tk1, in tk2, in tk3);

            return OpenWindow(window, uiLayer) ? window : null;
        }
        public IWindow Open(Type type, UILayer uiLayer, object[] tParams = null)
        {
            FDebug.Log($"F.UISystem.Open(type: {type.Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow(type);
            if (window is ITParams t) t.SetParams(tParams);

            return OpenWindow(window, uiLayer) ? window : null;
        }

        public ETask<T> OpenAsync<T>(UILayer uiLayer = UILayer.HUD) where T : class, IWindow, new()
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {typeof(T).Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow<T>();
            if (window is ITParams t) t.SetParamsDefault();

            return OpenWindowAsync(window, uiLayer);
        }
        public ETask<T> OpenAsync<T, TK1>(UILayer uiLayer, in TK1 tk1) where T : class, IWindow, ITParams<TK1>, new()
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {typeof(T).Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow<T>();
            window.SetParams(in tk1);

            return OpenWindowAsync(window, uiLayer);
        }
        public ETask<T> OpenAsync<T, TK1, TK2>(UILayer uiLayer, in TK1 tk1, in TK2 tk2) where T : class, IWindow, ITParams<TK1, TK2>, new()
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {typeof(T).Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow<T>();
            window.SetParams(in tk1, in tk2);

            return OpenWindowAsync(window, uiLayer);
        }
        public ETask<T> OpenAsync<T, TK1, TK2, TK3>(UILayer uiLayer, in TK1 tk1, in TK2 tk2, in TK3 tk3) where T : class, IWindow, ITParams<TK1, TK2, TK3>, new()
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {typeof(T).Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow<T>();
            window.SetParams(in tk1, in tk2, in tk3);

            return OpenWindowAsync(window, uiLayer);
        }

        public ETask<IWindow> OpenAsync(Type type, UILayer uiLayer, object[] tParams = null)
        {
            FDebug.Log($"F.UISystem.OpenAsync(type: {type.Name}), uiLayer: {uiLayer}");

            var window = GetOrCreateWindow(type);
            if (window is ITParams t) t.SetParams(tParams);

            return OpenWindowAsync(window, uiLayer);
        }

        public void Close(IWindow window) => CloseWindow(window, true);
        public void Close<T>() where T : class, IWindow, new() => CloseWindow(GetWindow<T>(), true);

        public void CloseLayer(UILayer uiLayer)
        {
            _windowTempList.Clear();
            foreach (var kv in _uiLayerWindowDict)
            {
                if ((kv.Key & uiLayer) > 0) _windowTempList.AddRange(kv.Value.WindowList);
            }
            foreach (var window in _windowTempList.ToArray()) CloseWindow(window, false);
            _windowTempList.Clear();
        }

        public void CloseAll()
        {
            _windowTempList.Clear();
            foreach (var kv in _uiLayerWindowDict) _windowTempList.AddRange(kv.Value.WindowList);
            foreach (var window in _windowTempList.ToArray()) CloseWindow(window, false);
            _windowTempList.Clear();
        }

        public void Destroy(IWindow window) => DestroyWindow(window);
        public void Destroy<T>() where T : class, IWindow, new() => DestroyWindow(GetWindow<T>());

        public void RefreshAllWindow()
        {
            foreach (var window in _updateList)
            {
                if (!_windowLayerDict.ContainsKey(window) || window.IsLoading) continue;
                window.Refresh();
            }
        }
        public T GetWindow<T>() where T : class, IWindow => GetWindow(typeof(T)) as T;
        public IWindow GetWindow(Type type) => _windowDict.GetValueOrDefault(type);
        public IWindow[] GetWindows(UILayer uiLayer)
        {
            _windowTempList.Clear();
            foreach (var kv in _uiLayerWindowDict)
            {
                if ((kv.Key & uiLayer) > 0) _windowTempList.AddRange(kv.Value.WindowList);
            }
            return _windowTempList.ToArray();
        }

        public UILayerBehaviour GetLayerData(UILayer uiLayer) => UIRootBehaviour.GetLayerData(uiLayer);
        public Camera GetCamera(UILayer uiLayer) => UIRootBehaviour.GetCamera(uiLayer);
        public Canvas GetCanvas(UILayer uiLayer) => UIRootBehaviour.GetCanvas(uiLayer);

        public bool IsOpen<T>() where T : class, IWindow
        {
            var window = GetWindow(typeof(T));
            return window != null && _windowLayerDict.ContainsKey(window);
        }

        public IWindow GetTopWindow(UILayer uiLayer)
        {
            if (_uiLayerWindowDict.TryGetValue(uiLayer, out var windowList))
                return windowList.TopWindow;
            return null;
        }

        private bool OpenWindow(IWindow window, UILayer uiLayer)
        {
            if (window.IsLoading)
            {
                FDebug.LogError($"F.UISystem.OpenWindow({window.GetType().Name}): window is loading.");
                return false;
            }

            if (_windowLayerDict.TryGetValue(window, out var curLayer) && curLayer != uiLayer)
                RemoveWindow(window);

            if (!_uiLayerWindowDict.TryGetValue(uiLayer, out var windowLayerInfo)) return false;
            
            _windowLayerDict[window] = uiLayer;
            _needRefresh = true;

            try
            {
                windowLayerInfo.Add(window);
                window.Open(windowLayerInfo.LayerBehaviour);
                return true;
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
            finally
            {
                windowLayerInfo.RefreshOrder();
            }
            
            DestroyWindow(window);
            return false;
        }

        private async ETask<T> OpenWindowAsync<T>(T window, UILayer uiLayer) where T : class, IWindow
        {
            if (window.IsLoading)
            {
                FDebug.LogError($"F.UISystem.OpenWindow({window.GetType().Name}): window is loading.");
                return null;
            }

            if (_windowLayerDict.TryGetValue(window, out var curLayer) && curLayer != uiLayer)
                RemoveWindow(window);

            if (!_uiLayerWindowDict.TryGetValue(uiLayer, out var windowLayerInfo)) return null;
            
            _windowLayerDict[window] = uiLayer;
            _needRefresh = true;

            try
            {
                windowLayerInfo.Add(window);
                await window.OpenAsync(windowLayerInfo.LayerBehaviour);
                return window;
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
            finally
            {
                windowLayerInfo.RefreshOrder();
            }

            DestroyWindow(window);
            return null;
        }

        private void CloseWindow(IWindow window, bool refreshAll)
        {
            if (window == null) return;
            Debug.Log($"F.WindowManager.Close(type:{window.GetType().Name})");

            RemoveWindow(window);

            try
            {
                window.Close();

                if (refreshAll) RefreshAllWindow();
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
                DestroyWindow(window);
            }
        }

        private void RemoveWindow(IWindow window)
        {
            if (_windowLayerDict.Remove(window, out var uiLayer) && _uiLayerWindowDict.TryGetValue(uiLayer, out var windowLayerInfo))
            {
                windowLayerInfo.Remove(window);
                _needRefresh = true;
            }
        }

        private void DestroyWindow(IWindow window)
        {
            if (window == null) return;
            if (!_windowDict.TryGetValue(window.GetType(), out var current) || !ReferenceEquals(current, window)) return;
            _windowDict.Remove(window.GetType());

            RemoveWindow(window);
            
            Debug.Log($"F.WindowManager.Destroy(type:{window.GetType().Name})");
            try { window.Destroy(); }
            catch (Exception exception) { FDebug.LogException(exception); }
        }

        private T GetOrCreateWindow<T>() where T : class, IWindow, new()
        {
            try
            {
                var type = typeof(T);
                if (!_windowDict.TryGetValue(type, out var window))
                {
                    window = new T();
                    window.Create();
                    _windowDict[type] = window;
                }
                return window as T;
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
                throw;
            }
        }
        private IWindow GetOrCreateWindow(Type type)
        {
            try
            {
                if (!_windowDict.TryGetValue(type, out var window))
                {
                    window = Activator.CreateInstance(type) as IWindow;
                    window.Create();
                    _windowDict[type] = window;
                }
                return window;
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
                throw;
            }
        }
    }
}

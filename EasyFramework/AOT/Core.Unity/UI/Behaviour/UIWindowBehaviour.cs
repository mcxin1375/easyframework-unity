/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyFramework
{
    internal class UIWindowBehaviour : UIBaseBehaviour
    {
        public IReadOnlyDictionary<UILayer, List<IWindow>> UILayerWindowDict => _uiLayerWindowDict;
        
        private readonly Dictionary<IWindow, UILayer> _windowLayerDict = new();
        private readonly Dictionary<UILayer, List<IWindow>> _uiLayerWindowDict = new();
        private readonly List<IWindow> _updateList = new();
        private readonly List<IWindow> _closeList = new();
        private readonly List<IWindow> _windowTempList = new();
        private UIRootBehaviour _uiRootBehaviour;
        private bool _needRefresh;
        
        private void Awake()
        {
            _uiRootBehaviour = UIRootBehaviour.Instance;
            foreach (var uiLayerBehaviour in _uiRootBehaviour.UILayers)
            {
                _uiLayerWindowDict.Add(uiLayerBehaviour.Layer, new List<IWindow>());
            }
        }

        private void Update()
        {
            if (_needRefresh)
            {
                _needRefresh = false;
                _updateList.Clear();
                foreach (var windowList in _uiLayerWindowDict.Values) _updateList.AddRange(windowList);
            }
            foreach (var window in _updateList)
            {
                if (window.IsOpen) window.Update();
            }
            for (int i = _closeList.Count - 1; i >= 0; i--)
            {
                var window = _closeList[i];
                if (window.IsOpen) _closeList.RemoveAt(i);
                else if (!window.Alive)
                {
                    _closeList.RemoveAt(i);
                    Destroy(window);
                }
            }
        }
        
        public UILayerBehaviour GetLayerData(UILayer uiLayer) => _uiRootBehaviour.GetLayerData(uiLayer);
        public IWindow[] GetWindows(UILayer uiLayer)
        {
            _windowTempList.Clear();
            foreach (var kv in _uiLayerWindowDict)
            {
                if ((kv.Key & uiLayer) > 0) _windowTempList.AddRange(kv.Value);
            }
            return _windowTempList.ToArray();
        }
        public void RefreshAllWindow()
        {
            foreach (var window in _updateList)
            {
                if (window.IsOpen) window.Refresh();
            }
        }
        
        public void OpenWindow(IWindow window, UILayer uiLayer)
        {
            if (_windowLayerDict.TryGetValue(window, out var curLayer) && curLayer != uiLayer)
            {
                RemoveWindow(window);
            }
            
            var uiLayerData = GetLayerData(uiLayer);
            window.Create(uiLayerData);

            AddWindow(uiLayer, window);
            _needRefresh = true;
                
            try
            {
                window.Open();
            }
            catch (Exception e) { FDebug.LogException(e); }
        }
        public async ETask<T> OpenWindowAsync<T>(T window, UILayer uiLayer) where T : class, IWindow
        {
            if (window.WindowObject == null)
            {
                var uiLayerData = GetLayerData(uiLayer);
                await window.CreateAsync(uiLayerData);
            }

            OpenWindow(window, uiLayer);
            await window.AfterOpenAsync();
            return window;
        }
        
        public void Close(IWindow window, bool refreshAll = true)
        {
            RemoveWindow(window);
            _closeList.Add(window);
            try
            {
                window.Close();
                _needRefresh = true;

                if (refreshAll) RefreshAllWindow();
            }
            catch (Exception e) { FDebug.LogException(e); }
        }
        
        public void CloseLayer(UILayer uiLayer)
        {
            _windowTempList.Clear();
            foreach (var kv in _uiLayerWindowDict)
            {
                if ((kv.Key & uiLayer) > 0) _windowTempList.AddRange(kv.Value);
            }
            foreach (var window in _windowTempList) Close(window, false);
            _windowTempList.Clear();
        }
        
        public void CloseAll()
        {
            _windowTempList.Clear();
            foreach (var kv in _uiLayerWindowDict) _windowTempList.AddRange(kv.Value);
            foreach (var window in _windowTempList) Close(window, false);
            _windowTempList.Clear();
        }
        
        public void Destroy(IWindow window)
        {
            RemoveWindow(window);
            try
            {
                window.Destroy();
                _needRefresh = true;
            }
            catch (Exception e) { FDebug.LogException(e); }
        }
        

        private void AddWindow(UILayer uiLayer, IWindow window)
        {
            if (!_uiLayerWindowDict.TryGetValue(uiLayer, out var windowList)) return;
            _windowLayerDict[window] = uiLayer;
            
            if (windowList.Contains(window)) windowList.Remove(window);

            var attribute = window.GetType().GetCustomAttribute<WindowOpenBeforeAttribute>();
            bool added = false;
            if (attribute != null)
            {
                for (int i = 0; i < windowList.Count; i++)
                {
                    var v = windowList[i];
                    if (v.GetType() == attribute.WindowType)
                    {
                        windowList.Insert(i, window);
                        added = true;
                        break;
                    }
                }
            }
            if (!added) windowList.Add(window);
            
            RefreshOrder(uiLayer);
        }
        private void RemoveWindow(IWindow window)
        {
            if (!_windowLayerDict.Remove(window, out var curLayer)) return;
            if (_uiLayerWindowDict.TryGetValue(curLayer, out var windowList))
            {
                if (windowList.Contains(window)) windowList.Remove(window);
            }
        }
        private void RefreshOrder(UILayer uiLayer)
        {
            if (!_uiLayerWindowDict.TryGetValue(uiLayer, out var windowList)) return;
            var uiLayerBehaviour = GetLayerData(uiLayer);
            
            int sortingOrder = uiLayerBehaviour.Canvas.sortingOrder;
            for (int i = 0; i < windowList.Count; i++)
            {
                var v = windowList[i];
                
                v.RefreshOrder(uiLayerBehaviour, sortingOrder, i);
                sortingOrder += uiLayerBehaviour.OrderInterval;
            }
        }
    }
}
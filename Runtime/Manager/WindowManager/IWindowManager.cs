/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:UI管理类
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    public interface IWindowManager
    {
        GameObject UIRoot { get; }
        UnityEngine.EventSystems.EventSystem EventSystem { get; }
        Vector2 Resolution { get; set; }
        bool EventSystemEnabled { get; set; }

        T Open<T>(UILayer uiLayer = UILayer.HUD) where T : class, IWindow, new();
        T Open<T, TK1>(UILayer uiLayer, in TK1 tk1) where T : class, IWindow, ITParams<TK1>, new();
        T Open<T, TK1, TK2>(UILayer uiLayer, in TK1 tk1, in TK2 tk2) where T : class, IWindow, ITParams<TK1, TK2>, new();
        T Open<T, TK1, TK2, TK3>(UILayer uiLayer, in TK1 tk1, in TK2 tk2, in TK3 tk3) where T : class, IWindow, ITParams<TK1, TK2, TK3>, new();
        IWindow Open(Type type, UILayer uiLayer, object[] tParams = null);
        
        ETask<T> OpenAsync<T>(UILayer uiLayer = UILayer.HUD) where T : class, IWindow, new();
        ETask<T> OpenAsync<T, TK1>(UILayer uiLayer, in TK1 tk1) where T : class, IWindow, ITParams<TK1>, new();
        ETask<T> OpenAsync<T, TK1, TK2>(UILayer uiLayer, in TK1 tk1, in TK2 tk2) where T : class, IWindow, ITParams<TK1, TK2>, new();
        ETask<T> OpenAsync<T, TK1, TK2, TK3>(UILayer uiLayer, in TK1 tk1, in TK2 tk2, in TK3 tk3) where T : class, IWindow, ITParams<TK1, TK2, TK3>, new();
        ETask<IWindow> OpenAsync(Type type, UILayer uiLayer, object[] tParams = null);

        void Close(IWindow window);
        ETask CloseAsync(IWindow window);
        void Close<T>() where T : class, IWindow, new();
        ETask CloseAsync<T>() where T : class, IWindow, new();
        void CloseLayer(UILayer uiLayer);
        void CloseAll();

        void Destroy(IWindow window);
        void Destroy<T>() where T : class, IWindow, new();

        void RefreshAllWindow();

        bool IsOpen<T>() where T : class, IWindow;
        T GetWindow<T>() where T : class, IWindow;
        IWindow GetWindow(Type type);
        IWindow GetTopWindow(UILayer uiLayer);
        IWindow[] GetWindows(UILayer uiLayer);
        Camera GetCamera(UILayer uiLayer);
        Canvas GetCanvas(UILayer uiLayer);
        UILayerBehaviour GetLayerData(UILayer uiLayer);
    }
}
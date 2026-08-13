/*----------------------------------------------------------------
// author:meng cheng xin
// date:2024/8/13
// describe:
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    public interface IWindow
    {
        bool IsOpen { get; }
        bool Alive { get; }
        UILayerBehaviour LayerBehaviour { get; }
        GameObject WindowObject { get; }
        ETask CreateAsync(UILayerBehaviour uiLayer);
        void Create(UILayerBehaviour uiLayer);
        void Destroy();
        void Open();
        ETask AfterOpenAsync();
        ETask BeforeCloseAsync();
        void Close();
        void SetActive(bool enabled);
        void Refresh();
        void RefreshOrder(UILayerBehaviour uiLayer, int orderInLayer, int orderIndex);
        void Update();
    }
    public interface IWindowComponent
    {
        void Create(IWindow window);
        void Destroy();
        void AddListeners();
        void RemoveListeners();
        void Open();
        ETask AfterOpenAsync();
        ETask BeforeCloseAsync();
        void Close();
        void SetActive(bool value) { }
        void Refresh();
        void RefreshOrderInLayer(int orderInLayer);
        void ButtonClick(Button btn);
        void Update();
    }
    public interface IWindowUI
    {
        void InitializeUI(GameObject uiObject);
    }
    public interface IWindowUI<out T> : IWindowUI where T : class, new()
    {
        public T UI { get; }
    }
}
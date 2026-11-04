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
        bool Alive { get; }
        bool IsLoading { get; }
        bool NeedDestroy { get; }
        UILayerBehaviour LayerBehaviour { get; }
        GameObject WindowObject { get; }
        void Create();
        void Destroy();
        void Open(UILayerBehaviour uiLayer);
        ETask OpenAsync(UILayerBehaviour uiLayer);
        void Close();
        void SetActive(bool enabled);
        void Refresh();
        void RefreshOrder(UILayerBehaviour uiLayer, int orderInLayer, int orderIndex);
        void Update();
    }
    public interface IWindowComponent
    {
        void Register(IWindow window);
        void OnLoadObject();
        void OnDestroy();
        void OnAddListeners();
        void OnRemoveListeners();
        void OnOpen();
        void OnClose();
        void OnSetActive(bool value) { }
        void OnRefresh();
        void OnRefreshOrderInLayer(int orderInLayer);
        void OnButtonClick(Button btn);
        void OnUpdate();
    }
    public interface IWindowUI
    {
        void InitializeUI(GameObject uiObject);
    }
    public interface IWindowUI<out T> : IWindowUI where T : class, IWindowComponent, new()
    {
        public T UI { get; }
    }
}
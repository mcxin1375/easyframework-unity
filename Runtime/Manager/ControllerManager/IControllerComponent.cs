/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/5
// describe??
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IControllerComponent
    {
        void Create(IController controller);
        ETask OnPreEnterAsync();
        ETask OnEnterAsync();
        ETask OnAfterEnterAsync();
        ETask OnExitAsync();
        void OnSetActive(bool isActive);
        void OnAddListeners();
        void OnRemoveListeners();
        void OnUpdate();
        void OnLateUpdate();
        void OnDestroy();
    }
    public interface IControllerLoading : IObjectTask
    {
        bool IObjectTask.IsCompleted => !IsLoading;

        bool IsLoading { get; }
        float Progress { get; }
        int Weight => 100;

        void OnStartLoading();
    }
}

namespace EasyFramework
{
    public interface IController
    {
        bool IsEnter { get; }
        bool IsActive { get; }
        bool UpdateEnabled { get; }

        void OnCreate();
        ETask OnPreEnterAsync();
        ETask OnEnterAsync();
        ETask OnExitAsync();
        void OnSetActive(bool active);
        void OnUpdate();
        void OnLateUpdate();
        void OnDestroy();
    }
}

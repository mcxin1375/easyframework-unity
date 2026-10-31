/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/5
// describe??
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IController
    {
        bool IsEnter { get; }
        bool IsActive { get; }
        void OnSetActive(bool active);
        ETask ExitAsync();

        void OnUpdate();
        void OnLateUpdate();
    }
}

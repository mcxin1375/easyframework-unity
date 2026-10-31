/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IFrameworkTick
    {
        void OnUpdate();
        void OnLateUpdate();
        void OnDestroy();
    }
}
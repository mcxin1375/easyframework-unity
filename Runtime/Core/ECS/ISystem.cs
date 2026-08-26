/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe: 
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface ISystem
    {
        int Order { get; }
        void Create();
        void Destroy();
        void Update();
        void LateUpdate();
    }
}
/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/5
// describe??
//----------------------------------------------------------------*/

using System.Collections.Generic;

namespace EasyFramework
{
    public interface IControllerManager
    {
        IController TopController { get; }
        IReadOnlyList<IController> EnterList { get; }
        
        bool HasEnter(IController controller);
        bool HasActive(IController controller);
        
        void Enter(IController controller);
        void Remove(IController controller);
        ETask ExitAllAsync();

    }
}

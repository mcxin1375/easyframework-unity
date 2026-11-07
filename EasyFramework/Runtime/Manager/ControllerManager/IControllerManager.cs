using System.Collections.Generic;

namespace EasyFramework
{
    public interface IControllerManager
    {
        IController TopController { get; }
        IReadOnlyList<IController> EnterList { get; }
        bool HasEnter(IController controller);
        bool HasActive(IController controller);
        T GetController<T>() where T : class, IController;

        ETask<T> EnterAsync<T>() where T : class, IController, new();
        ETask<T> EnterAsync<T, T1>(in T1 p1) where T : class, IController, ITParams<T1>, new();
        ETask<T> EnterAsync<T, T1, T2>(in T1 p1, in T2 p2) where T : class, IController, ITParams<T1, T2>, new();
        ETask<T> EnterAsync<T, T1, T2, T3>(in T1 p1, in T2 p2, in T3 p3) where T : class, IController, ITParams<T1, T2, T3>, new();
        ETask ExitAsync<T>() where T : class, IController;
        ETask ExitAsync(IController controller);
        ETask ExitAllAsync();
    }
}

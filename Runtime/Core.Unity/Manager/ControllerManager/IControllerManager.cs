/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/5
// describe??
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyFramework
{
    public interface IControllerManager
    {
        event Action<IController> OnEnter; 
        event Action<IController> OnExit;

        IController Current { get; }
        IReadOnlyList<IController> EnterList { get; }

        ETask EnterAsync<T>(EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, new();
        ETask EnterAsync<T, TK1>(in TK1 tk1, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1>, new();
        ETask EnterAsync<T, TK1, TK2>(in TK1 tk1, in TK2 tk2, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1, TK2>, new();
        ETask EnterAsync<T, TK1, TK2, TK3>(in TK1 tk1, in TK2 tk2, in TK3 tk3, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1, TK2, TK3>, new();
        ETask EnterAsync(Type type, EControllerEnter mode = EControllerEnter.Additive);
        ETask EnterAsync(IController controller, EControllerEnter mode = EControllerEnter.Additive);

        ETask SetActiveAsync<T>(bool isActive) where T : class, IController, new();
        
        ETask ExitAsync<T>() where T : class, IController, new();
        ETask ExitAsync(Type type);
        ETask ExitAllAsync();

        T Get<T>() where T : class, IController;
        IController Get(Type type);
        T GetOrCreate<T>() where T : class, IController, new();
        IController GetOrCreate(Type type);
        bool HasEnter<T>() where T : class, IController, new();
        bool HasActive<T>() where T : class, IController, new();
        bool HasEnter(Type type);
        void Create(Assembly assembly);
    }
    public interface IController
    {
        bool IsEnter { get; }
        bool IsActive { get; }
        Type Type { get; }
        void Create();
        ETask BeforeEnterAsync(EControllerEnter enter);
        ETask EnterAsync(EControllerEnter enter);
        ETask ExitAsync();
        ETask SetActiveAsync(bool isActive);
        void Update();
        void LateUpdate();
        void Destroy();
    }
    public interface IControllerComponent
    {
        void Create(IController controller);
        ETask OnBeforeEnterAsync(EControllerEnter enter);
        ETask OnEnterAsync();
        ETask OnAfterEnterAsync();
        ETask OnExitAsync();
        ETask OnSetActiveAsync(bool isActive);
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
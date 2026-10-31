/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/5
// describe??
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public interface IControllerManager
    {
        IController TopController { get; }
        IReadOnlyList<IController> EnterList { get; }

        // ETask EnterAsync<T>(EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, new();
        // ETask EnterAsync<T, TK1>(in TK1 tk1, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1>, new();
        // ETask EnterAsync<T, TK1, TK2>(in TK1 tk1, in TK2 tk2, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1, TK2>, new();
        // ETask EnterAsync<T, TK1, TK2, TK3>(in TK1 tk1, in TK2 tk2, in TK3 tk3, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1, TK2, TK3>, new();
        // ETask EnterAsync<TK1>(IController controller, in TK1 tk1, EControllerEnter mode = EControllerEnter.Additive);
        // ETask EnterAsync<TK1, TK2>(IController controller, in TK1 tk1, in TK2 tk2, EControllerEnter mode = EControllerEnter.Additive);
        // ETask EnterAsync<TK1, TK2, TK3>(IController controller, in TK1 tk1, in TK2 tk2, in TK3 tk3, EControllerEnter mode = EControllerEnter.Additive);
        // ETask EnterAsync(Type type, EControllerEnter mode = EControllerEnter.Additive);
        // ETask EnterAsync(IController controller, EControllerEnter mode = EControllerEnter.Additive);

        // ETask SetActiveAsync<T>(bool isActive) where T : class, IController, new();
        
        // ETask ExitAsync<T>() where T : class, IController, new();
        // ETask ExitAsync(Type type);
        // ETask ExitAllAsync();
        
        void Enter(IController controller);
        void Remove(IController controller);
        
        // bool HasEnter<T>() where T : class, IController, new();
        bool HasEnter(IController controller);
        // bool HasActive<T>() where T : class, IController, new();
        bool HasActive(IController controller);
        
        // void Create(Assembly assembly);
        // T Get<T>() where T : class, IController;
        // IController Get(Type type);
        // T GetOrCreate<T>() where T : class, IController, new();
        // IController GetOrCreate(Type type);
    }
    public interface IController
    {
        bool IsEnter { get; }
        bool IsActive { get; }
        void SetActive(bool active);
    }
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

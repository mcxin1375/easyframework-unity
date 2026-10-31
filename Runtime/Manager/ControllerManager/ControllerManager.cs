/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/5
// describe??
//----------------------------------------------------------------*/

using System.Collections.Generic;

namespace EasyFramework
{
    internal class ControllerManager : Singleton<ControllerManager>, IControllerManager
    {
        public IController TopController => _enterList.Count > 0 ? _enterList[^1] : null;
        public IReadOnlyList<IController> EnterList => _enterList;
        
        private readonly List<IController> _enterList = new();

        public void Enter(IController controller)
        {
            if (TopController == controller) return;
            if (_enterList.Contains(controller)) _enterList.Remove(controller);
            _enterList.Add(controller);

            for (int i = 0; i < _enterList.Count; i++)
            {
                _enterList[i].SetActive(i == _enterList.Count - 1);
            }
        }

        public void Remove(IController controller)
        {
            if (_enterList.Contains(controller)) _enterList.Remove(controller);
            
            for (int i = 0; i < _enterList.Count; i++)
            {
                _enterList[i].SetActive(i == _enterList.Count - 1);
            }
        }

        // private async ETask DoCommandAsync(Command command)
        // {
        //     var result = await _taskQueue.ExecuteAsync(command);
        //     if (result == ECommandResult.Failed)
        //     {
        //         FDebug.Log($"ControllerSystem.DoCommandAsync failed");
        //     }
        // }
        //
        // public ETask EnterAsync<TK1>(IController controller, in TK1 tk1, EControllerEnter mode = EControllerEnter.Additive)
        // {
        //     if (controller == null) return ETask.CompletedTask;
        //     if (controller is not ITParams<TK1> parameterController)
        //         throw new InvalidOperationException($"{controller.Type.Name} does not implement ITParams<{typeof(TK1).Name}>");
        //
        //     parameterController.SetParams(in tk1);
        //     return EnterAsync(controller, mode);
        // }
        // public ETask EnterAsync<TK1, TK2>(IController controller, in TK1 tk1, in TK2 tk2, EControllerEnter mode = EControllerEnter.Additive)
        // {
        //     if (controller == null) return ETask.CompletedTask;
        //     if (controller is not ITParams<TK1, TK2> parameterController)
        //         throw new InvalidOperationException($"{controller.Type.Name} does not implement ITParams<{typeof(TK1).Name}, {typeof(TK2).Name}>");
        //
        //     parameterController.SetParams(in tk1, in tk2);
        //     return EnterAsync(controller, mode);
        // }
        // public ETask EnterAsync<TK1, TK2, TK3>(IController controller, in TK1 tk1, in TK2 tk2, in TK3 tk3, EControllerEnter mode = EControllerEnter.Additive)
        // {
        //     if (controller == null) return ETask.CompletedTask;
        //     if (controller is not ITParams<TK1, TK2, TK3> parameterController)
        //         throw new InvalidOperationException($"{controller.Type.Name} does not implement ITParams<{typeof(TK1).Name}, {typeof(TK2).Name}, {typeof(TK3).Name}>");
        //
        //     parameterController.SetParams(in tk1, in tk2, in tk3);
        //     return EnterAsync(controller, mode);
        // }
        //
        // public ETask EnterAsync<T, TK1>(in TK1 tk1, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1>, new()
        // {
        //     FDebug.Log($"F.ControllerSystem.EnterAsync(type: {typeof(T).Name})");
        //     
        //     var ctr = GetOrCreate<T>();
        //     ctr.SetParams(in tk1);
        //     
        //     return DoCommandAsync(Command.Create(this, Command.ECommandType.EnterAsync, ctr, mode));
        // }
        // public ETask EnterAsync<T, TK1, TK2>(in TK1 tk1, in TK2 tk2, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1, TK2>, new()
        // {
        //     FDebug.Log($"F.ControllerSystem.EnterAsync(type: {typeof(T).Name})");
        //     
        //     var ctr = GetOrCreate<T>();
        //     ctr.SetParams(in tk1, in tk2);
        //     
        //     return DoCommandAsync(Command.Create(this, Command.ECommandType.EnterAsync, ctr, mode));
        // }
        // public ETask EnterAsync<T, TK1, TK2, TK3>(in TK1 tk1, in TK2 tk2, in TK3 tk3, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1, TK2, TK3>, new()
        // {
        //     FDebug.Log($"F.ControllerSystem.EnterAsync(type: {typeof(T).Name})");
        //     
        //     var ctr = GetOrCreate<T>();
        //     ctr.SetParams(in tk1, in tk2, in tk3);
        //     
        //     return DoCommandAsync(Command.Create(this, Command.ECommandType.EnterAsync, ctr, mode));
        // }
        // public ETask EnterAsync<T>(EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, new()
        // {
        //     return EnterAsync(GetOrCreate<T>(), mode);
        // }
        // public ETask EnterAsync(Type type, EControllerEnter mode = EControllerEnter.Additive)
        // {
        //     return EnterAsync(GetOrCreate(type), mode);
        // }
        // public ETask EnterAsync(IController controller, EControllerEnter mode = EControllerEnter.Additive)
        // {
        //     if (controller == null) return ETask.CompletedTask;
        //     FDebug.Log($"F.ControllerSystem.EnterAsync(type: {controller.Type.Name})");
        //     return DoCommandAsync(Command.Create(this, Command.ECommandType.EnterAsync, controller, mode));
        // }

        // public ETask SetActiveAsync<T>(bool isActive) where T : class, IController, new()
        // {
        //     FDebug.Log($"F.ControllerSystem.SetActiveAsync(type: {typeof(T).Name})");
        //     
        //     var ctr = GetOrCreate<T>();
        //     
        //     return DoCommandAsync(Command.SetActiveAsync(this, ctr, isActive));
        // }

        // public ETask ExitAsync<T>() where T : class, IController, new() => ExitAsync(typeof(T));
        // public ETask ExitAsync(Type type)
        // {
        //     FDebug.Log($"F.ControllerSystem.ExitAsync(type: {type.Name})");
        //
        //     var ctr = GetOrCreate(type);
        //     return DoCommandAsync(Command.Create(this, Command.ECommandType.ExitAsync, ctr));
        // }

        // public ETask ExitAllAsync()
        // {
        //     FDebug.Log($"F.ControllerSystem.ExitAllAsync()");
        //     
        //     return DoCommandAsync(Command.Create(this, Command.ECommandType.ExitAllAsync));
        // }

        // public bool HasEnter<T>() where T : class, IController, new() => HasEnter(Get(typeof(T)));
        public bool HasEnter(IController controller) => _enterList.Contains(controller);
        
        // public bool HasActive<T>() where T : class, IController, new() => HasActive(Get<T>());
        public bool HasActive(IController controller) => controller != null && _enterList.Count > 0 && _enterList[^1] == controller;

        // public void Create(Assembly assembly)
        // {
        //     var types = ReflectionUtility.FindInstanceTypes<IController>(assembly);
        //     foreach (var type in types) GetOrCreate(type);
        // }
        //
        // public T Get<T>() where T : class, IController => Get(typeof(T)) as T;
        // public IController Get(Type type) => _controllerDict.ContainsKey(type) ? _controllerDict[type] : null;
        // public T GetOrCreate<T>() where T : class, IController, new() => GetOrCreate(typeof(T)) as T;
        // public IController GetOrCreate(Type type)
        // {
        //     if (_controllerDict.TryGetValue(type, out var ctr)) return ctr;
        //     
        //     var t = Activator.CreateInstance(type) as IController;
        //     if (t == null)
        //     {
        //         FDebug.LogError($"Controller {type.FullName} does not implement IController!");
        //         return null;
        //     }
        //     _controllerDict.Add(type, t);
        //     
        //     try
        //     {
        //         t.Create();
        //     }
        //     catch (Exception e) { FDebug.LogError(e.ToString()); }
        //     return t;
        // }
    }
}

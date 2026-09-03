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
    internal partial class ControllerManager : Singleton<ControllerManager>, IControllerManager
    {
        public event Action<IController> OnEnter; 
        public event Action<IController> OnExit;
        
        public IController Current => _enterList.Count > 0 ? _enterList[^1] : null;
        public IReadOnlyList<IController> EnterList => _enterList;
        
        private readonly ETaskQueue _taskQueue = ETaskQueue.Create(false, nameof(ControllerManager));
        private readonly Dictionary<Type, IController> _controllerDict = new();
        private readonly List<IController> _enterList = new();
        private readonly List<IController> _updateList = new();
        private bool _needRefresh;

        internal void Update()
        {
            if (_needRefresh)
            {
                _needRefresh = false;
                _updateList.Clear();
                foreach (var controller in _enterList) _updateList.Add(controller);
            }

            foreach (var c in _updateList) c.Update();
        }
        internal void LateUpdate()
        {
            foreach (var c in _updateList) c.LateUpdate();
        }
        internal void Destroy()
        {
            foreach (var c in _controllerDict.Values) c.Destroy();
        }

        private async ETask DoCommandAsync(Command command)
        {
            var result = await _taskQueue.ExecuteAsync(command);
            if (result == ECommandResult.Failed)
            {
                FDebug.Log($"ControllerSystem.DoCommandAsync failed");
            }
        }

        public ETask EnterAsync<T, TK1>(in TK1 tk1, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1>, new()
        {
            FDebug.Log($"F.ControllerSystem.EnterAsync(type: {typeof(T).Name})");
            
            var ctr = GetOrCreate<T>();
            ctr.SetParams(in tk1);
            
            return DoCommandAsync(Command.Create(this, Command.ECommandType.EnterAsync, ctr, mode));
        }
        public ETask EnterAsync<T, TK1, TK2>(in TK1 tk1, in TK2 tk2, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1, TK2>, new()
        {
            FDebug.Log($"F.ControllerSystem.EnterAsync(type: {typeof(T).Name})");
            
            var ctr = GetOrCreate<T>();
            ctr.SetParams(in tk1, in tk2);
            
            return DoCommandAsync(Command.Create(this, Command.ECommandType.EnterAsync, ctr, mode));
        }
        public ETask EnterAsync<T, TK1, TK2, TK3>(in TK1 tk1, in TK2 tk2, in TK3 tk3, EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, ITParams<TK1, TK2, TK3>, new()
        {
            FDebug.Log($"F.ControllerSystem.EnterAsync(type: {typeof(T).Name})");
            
            var ctr = GetOrCreate<T>();
            ctr.SetParams(in tk1, in tk2, in tk3);
            
            return DoCommandAsync(Command.Create(this, Command.ECommandType.EnterAsync, ctr, mode));
        }
        public ETask EnterAsync<T>(EControllerEnter mode = EControllerEnter.Additive) where T : class, IController, new()
        {
            return EnterAsync(GetOrCreate<T>(), mode);
        }
        public ETask EnterAsync(Type type, EControllerEnter mode = EControllerEnter.Additive)
        {
            return EnterAsync(GetOrCreate(type), mode);
        }
        public ETask EnterAsync(IController controller, EControllerEnter mode = EControllerEnter.Additive)
        {
            if (controller == null) return ETask.CompletedTask;
            FDebug.Log($"F.ControllerSystem.EnterAsync(type: {controller.Type.Name})");
            return DoCommandAsync(Command.Create(this, Command.ECommandType.EnterAsync, controller, mode));
        }

        public ETask SetActiveAsync<T>(bool isActive) where T : class, IController, new()
        {
            FDebug.Log($"F.ControllerSystem.SetActiveAsync(type: {typeof(T).Name})");
            
            var ctr = GetOrCreate<T>();
            
            return DoCommandAsync(Command.SetActiveAsync(this, ctr, isActive));
        }

        public ETask ExitAsync<T>() where T : class, IController, new() => ExitAsync(typeof(T));
        public ETask ExitAsync(Type type)
        {
            FDebug.Log($"F.ControllerSystem.ExitAsync(type: {type.Name})");

            var ctr = GetOrCreate(type);
            return DoCommandAsync(Command.Create(this, Command.ECommandType.ExitAsync, ctr));
        }

        public ETask ExitAllAsync()
        {
            FDebug.Log($"F.ControllerSystem.ExitAllAsync()");
            
            return DoCommandAsync(Command.Create(this, Command.ECommandType.ExitAllAsync));
        }

        public T Get<T>() where T : class, IController => Get(typeof(T)) as T;
        public IController Get(Type type) => _controllerDict.ContainsKey(type) ? _controllerDict[type] : null;
        public T GetOrCreate<T>() where T : class, IController, new()
        {
            return Get<T>() ?? Create<T>();
        }
        public IController GetOrCreate(Type type)
        {
            return Get(type) ?? Create(type);
        }

        public bool HasEnter<T>() where T : class, IController, new() => HasEnter(typeof(T));
        public bool HasEnter(Type type)
        {
            var ctr = Get(type);
            return ctr?.IsEnter ?? false;
        }
        
        public bool HasActive<T>() where T : class, IController, new()
        {
            var ctr = Get<T>();
            return ctr?.IsActive ?? false;
        }

        public void Create(Assembly assembly)
        {
            var types = ReflectionUtility.FindInstanceTypes<IController>(assembly);
            foreach (var type in types) if (!_controllerDict.ContainsKey(type)) Create(type);
        }

        private T Create<T>() where T : class, IController, new()
        {
            var t = new T();
            _controllerDict.Add(typeof(T), t);
            
            try
            {
                t.Create();
            }
            catch (Exception e) { FDebug.LogError(e.ToString()); }
            return t;
        }

        private IController Create(Type type)
        {
            var t = Activator.CreateInstance(type) as IController;
            if (t == null)
            {
                FDebug.LogError($"Controller {type.FullName} does not implement IController!");
                return null;
            }
            _controllerDict.Add(type, t);
            
            try
            {
                t.Create();
            }
            catch (Exception e) { FDebug.LogError(e.ToString()); }
            return t;
        }

    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace EasyFramework
{
    public class FWorld
    {
        public IReadOnlyList<ISystem> SystemList => _systemList;
        
        public readonly FEntityManager EntityManager = new();
        private readonly List<ISystem> _systemList = new();
        
        private readonly ConcurrentDictionary<Type, ISystem> _systemDict = new();
        private bool _needRefresh;
        
        public void Update()
        {
            if (_needRefresh)
            {
                _needRefresh = false;
                _systemList.Clear();
                foreach (var system in _systemDict.Values) _systemList.Add(system);
                _systemList.Sort((a, b) => b.Order.CompareTo(a.Order));
                // Systems = _systemDict.Values.OrderByDescending(item => item.Order).ToArray();
            }

            EntityManager.Update();
            foreach (var value in _systemList) value.Update();
        }
        public void LateUpdate()
        {
            EntityManager.LateUpdate();
            foreach (var value in _systemList) value.LateUpdate();
        }
        public void Destroy()
        {
            EntityManager.Destroy();
            foreach (var value in _systemDict.Values) value.Destroy();
        }
        public void Dispose()
        {
            EntityManager.Destroy();
            foreach (var value in _systemDict.Values) value.Destroy();
            _systemDict.Clear();
        }
        public T GetOrCreateSystem<T>() where T : class, ISystem, new()
        {
            var type = typeof(T);
            if (_systemDict.TryGetValue(type, out var value)) return value as T;
            return CreateSystem<T>();
        }
        
        public T GetSystem<T>() where T : class, ISystem, new()
        {
            var type = typeof(T);
            if (_systemDict.TryGetValue(type, out var value)) return value as T;
            return null;
        }
        
        public T CreateSystem<T>() where T : class, ISystem, new()
        {
            var type = typeof(T);
            if (_systemDict.TryGetValue(type, out var value)) return value as T;
            
            var t = new T();
            try
            {
                t.Create();
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
            _systemDict.TryAdd(type, t);
            
            _needRefresh = true;
            return t;
        }
        
        public void CreateSystem(Assembly assembly)
        {
            var types = EasyFrameworkReflection.FindInstanceTypes(typeof(ISystem), assembly);

            List<ISystem> tmpList = new();
            
            foreach (Type type in types)
            {
                if (type.IsAbstract || !type.IsClass) continue;
                if (_systemDict.ContainsKey(type)) continue;
                
                var t = Activator.CreateInstance(type) as ISystem;
                if (t == null) continue;
                
                _systemDict.TryAdd(type, t);
                tmpList.Add(t);
                
            }
            
            tmpList.Sort((a, b) => b.Order.CompareTo(a.Order));
            try
            {
                foreach (var system in tmpList)
                {
                    system.Create();
                }
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
            
            _needRefresh = true;
        }

    }
}
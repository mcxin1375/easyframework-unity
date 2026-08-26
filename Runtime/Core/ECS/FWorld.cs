/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyFramework
{
    public class FWorld
    {
        public readonly int Index;
        public readonly FEntityManager EntityManager = new();
        public bool Alive { get; private set; } = true;
        
        public IReadOnlyList<ISystem> SystemList => _systemList;
        private readonly List<ISystem> _systemList = new();
        private readonly Dictionary<Type, ISystem> _systemDict = new();
        private bool _needRefresh;

        internal FWorld(int index)
        {
            Index = index;
        }

        public void Update()
        {
            if (!Alive) return;
            if (_needRefresh)
            {
                _needRefresh = false;
                _systemList.Clear();
                foreach (var system in _systemDict.Values) _systemList.Add(system);
                _systemList.Sort(CompareSystems);
                // Systems = _systemDict.Values.OrderByDescending(item => item.Order).ToArray();
            }

            EntityManager.Update();
            foreach (var value in _systemList) value.Update();
        }
        public void LateUpdate()
        {
            if (!Alive) return;
            EntityManager.LateUpdate();
            foreach (var value in _systemList) value.LateUpdate();
        }
        public void Destroy()
        {
            if (!Alive) return;
            Alive = false;
            EntityManager.Destroy();
            foreach (var value in _systemDict.Values)
                try { value.Destroy(); }
                catch (Exception e) { FDebug.LogException(e); }
            _systemDict.Clear();
            _systemList.Clear();
            _needRefresh = false;
        }
        public T GetOrCreateSystem<T>() where T : class, ISystem, new()
        {
            if (!Alive) return null;
            var type = typeof(T);
            if (_systemDict.TryGetValue(type, out var value)) return value as T;
            return CreateSystem<T>();
        }
        
        public T GetSystem<T>() where T : class, ISystem
        {
            var type = typeof(T);
            if (_systemDict.TryGetValue(type, out var value)) return value as T;
            return null;
        }
        
        public T CreateSystem<T>() where T : class, ISystem, new()
        {
            if (!Alive) return null;
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
                return null;
            }
            _systemDict.Add(type, t);
            
            _needRefresh = true;
            return t;
        }
        
        public void CreateSystems(Assembly assembly)
        {
            if (!Alive) return;
            var types = EasyFrameworkReflection.FindInstanceTypes(typeof(ISystem), assembly);

            List<ISystem> tmpList = new();
            
            foreach (Type type in types)
            {
                if (type.IsAbstract || !type.IsClass) continue;
                if (_systemDict.ContainsKey(type)) continue;
                
                ISystem t;
                try { t = Activator.CreateInstance(type) as ISystem; }
                catch (Exception e) { FDebug.LogException(e); continue; }
                if (t == null) continue;
                
                tmpList.Add(t);
                
            }
            
            tmpList.Sort(CompareSystems);
            foreach (var system in tmpList)
            {
                try
                {
                    system.Create();
                    _systemDict.Add(system.GetType(), system);
                }
                catch (Exception e) { FDebug.LogException(e); }
            }
            _needRefresh = true;
        }

        private static int CompareSystems(ISystem a, ISystem b)
        {
            var result = b.Order.CompareTo(a.Order);
            return result != 0 ? result : string.CompareOrdinal(a.GetType().FullName, b.GetType().FullName);
        }

    }
}

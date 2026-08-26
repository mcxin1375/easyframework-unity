
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyFramework
{
    public interface IEntityComponent
    {
        public FEntity FEntity { get; }
        // internal bool IsDestroy { get; }
        internal void Create(FEntity entity);
        internal void Destroy();
        internal void Update();
        internal void LateUpdate();
    }
    public class FEntity
    {
        public Guid Guid { get; private set; }
        public bool Alive { get; private set; }
        private readonly Dictionary<Type, IEntityComponent> _typeDict = new();

        internal FEntity(Guid guid)
        {
            Guid = guid;
            Alive = true;
        }
        internal void Update()
        {
            foreach (var value in _typeDict.Values) value.Update();
        }
        internal void LateUpdate()
        {
            foreach (var value in _typeDict.Values) value.LateUpdate();
        }
        internal void Destroy()
        {
            Guid = Guid.Empty;
            Alive = false;
            try
            {
                foreach (var value in _typeDict.Values) value.Destroy();
            }
            catch (Exception e) { FDebug.LogException(e); }
            _typeDict.Clear();
        }

        public override string ToString() => Guid.ToString();

        public void AddComponent<T>(T entityComponent) where T : class, IEntityComponent
        {
            var type = typeof(T);
            if (_typeDict.ContainsKey(type)) return;
            _typeDict.Add(type, entityComponent);

            try
            {
                entityComponent.Create(this);
            }
            catch (Exception e) { FDebug.LogException(e); }
        }
        public T AddComponent<T>() where T : class, IEntityComponent, new()
        {
            var type = typeof(T);
            if (_typeDict.ContainsKey(type)) return GetComponent<T>();
            var t = new T();
            _typeDict.Add(type, t);

            try
            {
                t.Create(this);
            }
            catch (Exception e) { FDebug.LogException(e); }
            return t;
        }
        public T AddComponent<T, TK1>(TK1 tk1) where T : class, IEntityComponent, IEntityComponent<TK1>, new()
        {
            var type = typeof(T);
            if (_typeDict.ContainsKey(type)) return GetComponent<T>();
            var t = new T();
            _typeDict.Add(type, t);
            try
            {
                t.SetParams(tk1);
                t.Create(this);
            }
            catch (Exception e) { FDebug.LogException(e); }
            return t;
        }
        public T AddComponent<T, TK1, TK2>(TK1 tk1, TK2 tk2) where T : class, IEntityComponent, IEntityComponent<TK1, TK2>, new()
        {
            var type = typeof(T);
            if (_typeDict.ContainsKey(type)) return GetComponent<T>();
            var t = new T();
            _typeDict.Add(type, t);
            try
            {
                t.SetParams(tk1, tk2);
                t.Create(this);
            }
            catch (Exception e) { FDebug.LogException(e); }
            return t;
        }
        public T AddComponent<T, TK1, TK2, TK3>(TK1 tk1, TK2 tk2, TK3 tk3) where T : class, IEntityComponent, IEntityComponent<TK1, TK2, TK3>, new()
        {
            var type = typeof(T);
            if (_typeDict.ContainsKey(type)) return GetComponent<T>();
            var t = new T();
            _typeDict.Add(type, t);
            try
            {
                t.SetParams(tk1, tk2, tk3);
                t.Create(this);
            }
            catch (Exception e) { FDebug.LogException(e); }
            return t;
        }
        public void RemoveComponent<T>() where T : class, IEntityComponent
        {
            var type = typeof(T);
            if (_typeDict.TryGetValue(type, out var value))
            {
                _typeDict.Remove(type);
                value.Destroy();
            }
        }
        public T GetComponent<T>() where T : class, IEntityComponent
        {
            var type = typeof(T);
            if (_typeDict.TryGetValue(type, out var value))
            {
                return value as T;
            }

            foreach (var dictValue in _typeDict.Values)
            {
                if (dictValue is T t) return t;
            }

            return null;
        }
        public IEntityComponent[] GetComponents()
        {
            return _typeDict.Values.ToArray();
        }
    }
}
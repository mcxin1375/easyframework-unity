/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe: 
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public class FEntity
    {
        public Guid Guid { get; private set; }
        public bool Alive { get; private set; } = true;
        private readonly List<IEntityComponent> _components = new();

        internal FEntity(Guid guid)
        {
            Guid = guid;
        }
        internal void Destroy()
        {
            if (!Alive) return;
            Alive = false;
            Guid = Guid.Empty;
            while (_components.Count > 0)
            {
                var component = _components[^1];
                _components.RemoveAt(_components.Count - 1);
                try { component.OnRemoveComponent(); }
                catch (Exception e) { FDebug.LogException(e); }
            }
        }

        public override string ToString() => Guid.ToString();

        public void AddComponent<T>(T component) where T : class, IEntityComponent
        {
            if (!Alive) throw new InvalidOperationException("Cannot add a component to a destroyed entity.");
            if (component == null) throw new ArgumentNullException(nameof(component));
            foreach (var value in _components)
                if (value.GetType() == component.GetType()) throw new InvalidOperationException($"Component {component.GetType().Name} already exists.");
            _components.Add(component);
            try { component.OnAddComponent(); }
            catch (Exception e) { FDebug.LogException(e); }
        }
        public T AddComponent<T>() where T : class, IEntityComponent, new()
        {
            var component = new T();
            AddComponent(component);
            return component;
        }
        public void RemoveComponent<T>() where T : class, IEntityComponent
        {
            for (var i = _components.Count - 1; i >= 0; i--)
            {
                if (_components[i] is not T component) continue;

                _components.RemoveAt(i);
                try
                {
                    component.OnRemoveComponent();
                }
                catch (Exception e) { FDebug.LogException(e); }
            }
        }
        public bool HasComponent<T>() where T : class, IEntityComponent
        {
            return GetComponent<T>() != null;
        }
        public T GetComponent<T>() where T : class, IEntityComponent
        {
            foreach (var component in _components) if (component is T t) return t;
            return null;
        }
        public IEntityComponent[] GetComponents()
        {
            return _components.ToArray();
        }
    }
}

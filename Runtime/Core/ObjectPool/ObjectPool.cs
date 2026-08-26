/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public interface IObjectPoolEvent
    {
        void OnRent() { }
        void OnReturn() { }
        void OnCreate() { }
        void OnDispose() { }
    }
    
    public sealed class ObjectPool<T> : IPoolDebug where T : class, new()
    {
        private static ObjectPool<T> _shared;
        public static ObjectPool<T> Shared
        {
            get
            {
                _shared ??= new ObjectPool<T>();
                return _shared;
            }
        }
        
        public int MaxSize { get; set; } = 128;
        public Func<T> CreateFunc { get; set; }

        public int CreatedCount { get; private set; }
        public int PooledCount => _pool.Count;
        public Type ObjectType { get; } = typeof(T);

        private readonly Queue<T> _pool = new();
        private readonly object _locker = new();

        private ObjectPool()
        {
            this.AddDebug();
        }

        public T Rent()
        {
            var item = TakeOrCreate();
            if (item is IObjectPoolEvent e) e.OnRent();
            return item;
        }

        private T TakeOrCreate()
        {
            lock (_locker)
            {
                if (_pool.Count > 0) return _pool.Dequeue();
            }
            CreatedCount++;
            var item = CreateFunc();
            if (item is IObjectPoolEvent e) e.OnCreate();
            return item;
        }

        public void Return(T item)
        {
            var e = item as IObjectPoolEvent;
            e?.OnReturn();
            
            lock (_locker)
            {
                if (_pool.Count >= MaxSize)
                {
                    e?.OnDispose();
                    return;
                }

                if (_pool.Contains(item))
                {
                    FDebug.LogError($"Object pool already contains item {item}");
                    return;
                }

                _pool.Enqueue(item);
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                foreach (var t in _pool)
                {
                    if (t is IObjectPoolEvent e) e.OnDispose();
                }
                _pool.Clear();
            }
        }
    }
}
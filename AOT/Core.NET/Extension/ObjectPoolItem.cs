/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public interface IPoolItem
    {
        void OnRent();
        void OnReturn();
        void OnDispose();
    }

    public class ObjectPoolItem<T> : IPoolDebug where T : class, IPoolItem, new()
    {
        private static ObjectPoolItem<T> _shared;
        public static ObjectPoolItem<T> Shared
        {
            get
            {
                _shared ??= new();
                return _shared;
            }
        }
        
        public int MaxSize { get; set; } = 1024;
        public Func<T> CreateFunc { get; set; }
        
        public int CreatedCount { get; private set; }
        public int PooledCount
        {
            get
            {
                lock (_locker)
                {
                    return _pool.Count;
                }
            }
        }
        public Type ObjectType { get; } = typeof(T);
        
        private readonly Queue<T> _pool = new();
        private readonly object _locker = new();
        
        public ObjectPoolItem()
        {
            this.AddDebug();
        }

        public T Rent()
        {
            T item = null;
            lock (_locker)
            {
                if (_pool.Count > 0) item = _pool.Dequeue();
                else
                {
                    CreatedCount++;
                    item = CreateFunc();
                    // item = CreateFunc?.Invoke() ?? new T();
                }
            }
            
            item.OnRent();
            return item;
            // Debug.Log($"ObjectPool<{typeof(T).Name}> create new object, CreateCount: {_index}");
        }

        public void Return(T item)
        {
            lock (_locker)
            {
                if (_pool.Count >= MaxSize)
                {
                    item.OnDispose();
                    return;
                }
                
                if (_pool.Contains(item))
                {
                    FDebug.LogError($"Object pool already contains item {item}");
                    return;
                }

                _pool.Enqueue(item);
            }
            item.OnReturn();
        }

        public void Clear()
        {
            lock (_locker)
            {
                while (_pool.Count > 0)
                {
                    var item = _pool.Dequeue();
                    item.OnDispose();
                }
            }
        }
    }
}
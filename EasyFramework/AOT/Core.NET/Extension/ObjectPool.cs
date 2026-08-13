/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
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

        private ObjectPool()
        {
            this.AddDebug();
        }

        public T Rent()
        {
            lock (_locker)
            {
                if (_pool.Count > 0) return _pool.Dequeue();
                CreatedCount++;
            }
            return CreateFunc?.Invoke() ?? new T();
        }

        public void Return(T item)
        {
            lock (_locker)
            {
                if (_pool.Count >= MaxSize)
                {
                    if (item is IDisposable disposable) disposable.Dispose();
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
                    if (t is IDisposable disposable) disposable.Dispose();
                }
                _pool.Clear();
            }
        }
    }
}
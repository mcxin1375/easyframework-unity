/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:计时器
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public class ObjectEventInfo
    {
        public readonly Type ObjectType;
        
        public readonly Dictionary<Type, List<(object instance, int order)>> EventDict = new();
        private readonly Dictionary<Type, List<object>> _sortedEventCache = new();
        // private readonly Dictionary<Type, object[]> _sortedEventCache = new();

        public ObjectEventInfo(Type objectType)
        {
            ObjectType = objectType;
        }
        
        public void RegisterEvent(Type eventType, object eventObj, int order, bool refreshCache = true)
        {
            if (!EventDict.TryGetValue(eventType, out var list))
            {
                list = new List<(object, int)>();
                EventDict[eventType] = list;
            }
            list.Add((eventObj, order));
            
            if (refreshCache)
            {
                list.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                // 替代 Select().ToArray()
                if (!_sortedEventCache.TryGetValue(eventType, out var sortedList))
                {
                    sortedList = new List<object>(list.Count);
                    _sortedEventCache[eventType] = sortedList;
                }
                else
                {
                    sortedList.Clear();
                }

                for (int i = 0; i < list.Count; ++i)
                    sortedList.Add(list[i].instance);
            }
        }

        public void Invoke<T>(Action<T> action) where T : class
        {
            if (_sortedEventCache.TryGetValue(typeof(T), out var list))
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    try
                    {
                        action((T)list[i]); // 由于提前检查类型，这里无装箱
                    }
                    catch (Exception e)
                    {
                        FDebug.LogError(e.ToString());
                    }
                }
            }
        }
        
        public T[] GetEvents<T>() where T : class
        {
            if (_sortedEventCache.TryGetValue(typeof(T), out var list))
            {
                var result = new T[list.Count];
                for (int i = 0; i < list.Count; ++i) result[i] = (T)list[i];
                return result;
            }

            return Array.Empty<T>();
        }
        
        public void RefreshOrder()
        {
            foreach (var pair in EventDict)
            {
                var key = pair.Key;
                var list = pair.Value;
                list.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                if (!_sortedEventCache.TryGetValue(key, out var sortedList))
                {
                    sortedList = new List<object>(list.Count);
                    _sortedEventCache[key] = sortedList;
                }
                else
                {
                    sortedList.Clear();
                }

                for (int i = 0; i < list.Count; ++i) sortedList.Add(list[i].instance);
            }
        }
    }
}
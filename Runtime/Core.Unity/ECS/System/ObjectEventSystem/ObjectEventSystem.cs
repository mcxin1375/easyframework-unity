// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:计时器
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using UnityEngine;
//
// namespace EasyFramework
// {
//     public class ObjectEventSystem : FSystem
//     {
//         internal Dictionary<Type, ObjectEventInfo> ObjectEventInfoDict => _objectEventInfoDict;
//         
//         private readonly Dictionary<Type, object> _registerObjects = new();
//         private readonly Dictionary<Type, Type> _eventToObjectTypeDict = new();
//         private readonly Dictionary<Type, ObjectEventInfo> _objectEventInfoDict = new();
//         
//         protected override void OnCreate()
//         {
//             base.OnCreate();
//             
//             var interfaceTypes = EasyFrameworkReflection.FindTypesByAttribute(typeof(ObjectEventSystemInterfaceAttribute));
//             foreach (var interfaceType in interfaceTypes)
//             {
//                 var attribute = interfaceType.GetCustomAttribute<ObjectEventSystemInterfaceAttribute>();
//                 _eventToObjectTypeDict.Add(interfaceType, attribute.ObjectType);
//             }
//             
//             var typeObjects = EasyFrameworkReflection.CreateObjectsByAttribute(typeof(ObjectEventSystemAttribute));
//             foreach (var o in typeObjects) RegisterObject(o, false);
//
//             foreach (var value in _objectEventInfoDict.Values) value.RefreshOrder();
//         }
//
//         public void RegisterObject(params object[] instances)
//         {
//             foreach (var instance in instances) RegisterObject(instance, true);
//         }
//
//         internal void Invoke<T>(Type objectType, Action<T> action) where T : class
//         {
//             if (_objectEventInfoDict.TryGetValue(objectType, out var objectEventInfo))
//             {
//                 objectEventInfo.Invoke(action);
//             }
//         }
//
//         internal T[] GetEvents<T>(Type objectType) where T : class
//         {
//             if (_objectEventInfoDict.TryGetValue(objectType, out var objectEventInfo))
//             {
//                 return objectEventInfo.GetEvents<T>();
//             }
//             return Array.Empty<T>();
//         }
//
//         private void RegisterObject(object instance, bool refreshCache)
//         {
//             if (_registerObjects.ContainsKey(instance.GetType()))
//             {
//                 FDebug.LogError($"Object {instance.GetType()} is already registered.", LogTag.EasyFramework);
//                 return;
//             }
//             
//             var type = instance.GetType();
//             var interfaces = type.GetInterfaces();
//
//             // 获取排序信息
//             var orderMap = type.GetCustomAttributes(typeof(ObjectEventSystemOrderAttribute), false)
//                 .Cast<ObjectEventSystemOrderAttribute>()
//                 .ToDictionary(a => a.InterfaceType, a => a.Order);
//
//             foreach (var iface in interfaces)
//             {
//                 if (!iface.IsInterface || !iface.IsInstanceOfType(instance)) continue;
//
//                 int order = orderMap.TryGetValue(iface, out var o) ? o : 0;
//
//                 RegisterEvent(iface, instance, order, refreshCache);
//             }
//             
//             _registerObjects.Add(type, instance);
//         }
//         private void RegisterEvent(Type eventType, object eventObject, int order, bool refreshCache = true)
//         {
//             if (_eventToObjectTypeDict.TryGetValue(eventType, out var objectType))
//             {
//                 RegisterEvent(objectType, eventType, eventObject, order, refreshCache);
//             }
//         }
//         private void RegisterEvent(Type objectType, Type eventType, object eventObject, int order, bool refreshCache = true)
//         {
//             if (!_objectEventInfoDict.TryGetValue(objectType, out var objectEventInfo))
//             {
//                 objectEventInfo = new ObjectEventInfo(objectType);
//                 _objectEventInfoDict.Add(objectType, objectEventInfo);
//             }
//             objectEventInfo.RegisterEvent(eventType, eventObject, order, refreshCache);
//         }
//     }
// }
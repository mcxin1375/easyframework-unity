/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyFramework
{
    public static class ReflectionUtility
    {
        public const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        public const BindingFlags DefaultBindingFlagsStatic = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        public static readonly Assembly[] TagAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => assembly.GetCustomAttribute<EasyFrameworkReflectionAttribute>() != null)
            .ToArray();

        public static T FindFieldOrProperty<T>(object obj) where T : class => FindFieldOrProperty<T>(obj.GetType(), obj);
        public static T FindFieldOrProperty<T>(this Type type, object obj = null) where T : class
        {
            if (type == null) return null;
            
            BindingFlags bindingFlags = obj == null ? DefaultBindingFlagsStatic : DefaultBindingFlags;
            var tType = typeof(T);

            var fields = type.GetFields(bindingFlags);
            foreach (var fi in fields)
            {
                if (fi.FieldType == tType || tType.IsAssignableFrom(fi.FieldType))
                {
                    if (fi.GetValue(obj) is T t) return t;
                }
            }

            var properties = type.GetProperties(bindingFlags);
            foreach (var prop in properties)
            {
                if (prop.PropertyType == tType || tType.IsAssignableFrom(prop.PropertyType))
                {
                    if (prop.GetValue(obj) is T t) return t;
                }
            }

            return FindFieldOrProperty<T>(type.BaseType, obj);
        }

        public static T[] FindFieldsAndProperties<T>(object obj) where T : class => FindFieldsAndProperties<T>(obj.GetType(), obj);
        public static T[] FindFieldsAndProperties<T>(this Type type, object obj = null) where T : class
        {
            if (type == null) return null;
            
            BindingFlags bindingFlags = obj == null ? DefaultBindingFlagsStatic : DefaultBindingFlags;
            HashSet<T> tmpList = new();
            var tType = typeof(T);

            void RunType(Type baseType)
            {
                if (baseType == null) return;
                
                var fields = baseType.GetFields(bindingFlags);
                foreach (var fi in fields)
                {
                    if (fi.FieldType == tType || tType.IsAssignableFrom(fi.FieldType))
                    {
                        if (fi.GetValue(obj) is T t) tmpList.Add(t);
                    }
                }

                var properties = baseType.GetProperties(bindingFlags);
                foreach (var prop in properties)
                {
                    if (prop.PropertyType == tType || tType.IsAssignableFrom(prop.PropertyType))
                    {
                        if (prop.GetValue(obj) is T t) tmpList.Add(t);
                    }
                }
                
                RunType(baseType.BaseType);
            }

            RunType(type);
            return tmpList.ToArray();
        }
        
        public static T CreateInstance<T>(Type type)
        {
            return (T)Activator.CreateInstance(type);
        }
        public static object CreateObject(Type type)
        {
            return Activator.CreateInstance(type);
        }
        public static T CreateInstance<T>(params Assembly[] assemblies)
        {
            var types = FindInstanceTypes(typeof(T), assemblies);
            if (types.Length == 0) return default;

            Type createType = types[0];
            int createOrder = createType.GetCustomAttribute<ReflectionOrderAttribute>()?.Order ?? 0;
            for (int i = 1; i < types.Length; i++)
            {
                var type = types[i];
                var order = type.GetCustomAttribute<ReflectionOrderAttribute>()?.Order ?? 0;
                if (order > createOrder)
                {
                    createType = type;
                    createOrder = order;
                }
            }
            return (T)Activator.CreateInstance(createType);
        }

        public static T[] CreateInstances<T>(params Assembly[] assemblies)
        {
            var types = FindInstanceTypes(typeof(T), assemblies);
            return CreateInstancesByTypes<T>(types);
        }
        public static T[] CreateInstancesByAttribute<T>(Type attribute, params Assembly[] assemblies)
        {
            var types = FindTypesByBaseTypeAndAttribute(typeof(T), attribute, true, assemblies);
            return CreateInstancesByTypes<T>(types);
        }

        public static object[] CreateObjectsByAttribute<T>(params Assembly[] assemblies) => CreateObjectsByAttribute(typeof(T), assemblies);
        public static object[] CreateObjectsByAttribute(Type attributeType, params Assembly[] assemblies)
        {
            var types = FindInstanceTypesByAttribute(attributeType, assemblies);
            return CreateObjectsByTypes(types);
        }

        public static object[] CreateObjects<T>(params Assembly[] assemblies) => CreateObjects(typeof(T), assemblies);
        public static object[] CreateObjects(Type baseType, params Assembly[] assemblies)
        {
            var types = FindInstanceTypes(baseType, assemblies);
            return CreateObjectsByTypes(types);
        }
        
        public static T[] CreateInstancesByTypes<T>(Type[] types)
        {
            var arr = new T[types.Length];
            for (int i = 0; i < arr.Length; i++) arr[i] = (T)Activator.CreateInstance(types[i]);
            return arr;
        }
        public static object[] CreateObjectsByTypes(Type[] types)
        {
            var arr = new object[types.Length];
            for (int i = 0; i < arr.Length; i++) arr[i] = Activator.CreateInstance(types[i]);
            return arr;
        }
        
        public static Type FindType(string fullName, Assembly[] assemblies = null)
        {
            if (string.IsNullOrEmpty(fullName)) return null;
            assemblies ??= AppDomain.CurrentDomain.GetAssemblies(); 
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types) if (type.FullName == fullName) return type;
            }
            return null;
        }
        
        
        public static Type[] FindInstanceTypes<T>(params Assembly[] assemblies) => FindInstanceTypes(typeof(T), assemblies);
        public static Type[] FindInstanceTypes(Type baseType, params Assembly[] assemblies)
        {
            return FindTypesByBaseTypeAndAttribute(baseType, null, true, assemblies);
        }
        
        public static Type[] FindInstanceTypesByAttribute<T>(params Assembly[] assemblies) => FindInstanceTypesByAttribute(typeof(T), assemblies);
        public static Type[] FindInstanceTypesByAttribute(Type attributeType, params Assembly[] assemblies)
        {
            return FindTypesByBaseTypeAndAttribute(null, attributeType, true, assemblies);
        }
        
        
        public static Type[] FindTypesByAttribute<T>(params Assembly[] assemblies) => FindTypesByAttribute(typeof(T), assemblies);
        public static Type[] FindTypesByAttribute(Type attributeType, params Assembly[] assemblies)
        {
            return FindTypesByBaseTypeAndAttribute(null, attributeType, false, assemblies);
        }
        
        public static Type[] FindTypes<T>(params Assembly[] assemblies) => FindTypes(typeof(T), assemblies);
        public static Type[] FindTypes(Type baseType, params Assembly[] assemblies)
        {
            return FindTypesByBaseTypeAndAttribute(baseType, null, false, assemblies);
        }
        
        public static Type[] FindTypesByBaseTypeAndAttribute(Type baseType, Type attributeType, bool instanceType, Assembly[] assemblies = null)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = AppDomain.CurrentDomain.GetAssemblies(); 

            List<Type> tmpList = new();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (baseType != null && !baseType.IsAssignableFrom(type)) continue;
                    if (attributeType != null && type.GetCustomAttribute(attributeType) == null) continue;
                    if (instanceType && (type.IsAbstract || !type.IsClass)) continue;
                    tmpList.Add(type);
                }
            }
            return tmpList.ToArray();;
        }

        public static T GetCustomAttribute<T>(Type type) where T : Attribute => type.GetCustomAttribute<T>(true);
    }
}

/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyFramework
{
    public static partial class UnityHelper
    {
        private static readonly Dictionary<string, Transform> TempDict = new();
        
        public static void AutoSetComponents(object obj, GameObject viewObj, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            try
            {
                Transform[] transformArr = viewObj.GetComponentsInChildren<Transform>(true);
                foreach (Transform transform in transformArr) TempDict[transform.name] = transform;

                RunComponentType(obj, obj.GetType());
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
            finally
            {
                TempDict.Clear();
            }
        }
        
        private static void RunComponentType(object obj, Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            var fields = type.GetFields(bindingFlags);
            foreach (FieldInfo fieldInfo in fields)
            {
                if (!fieldInfo.FieldType.IsSubclassOf(typeof(Component))) continue;

                if (TempDict.TryGetValue(fieldInfo.Name, out Transform t))
                {
                    Component comp = t.gameObject.GetComponent(fieldInfo.FieldType);
                    if (comp != null) fieldInfo.SetValue(obj, comp);
                }
            }
            
            var properties = type.GetProperties(bindingFlags);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!propertyInfo.PropertyType.IsSubclassOf(typeof(Component))) continue;
                if (!propertyInfo.CanWrite) continue;

                if (TempDict.TryGetValue(propertyInfo.Name, out Transform t))
                {
                    Component comp = t.gameObject.GetComponent(propertyInfo.PropertyType);
                    if (comp != null) propertyInfo.SetValue(obj, comp);
                }
            }

            if (type.BaseType == null || type.BaseType == typeof(System.Object)) return;
            RunComponentType(obj, type.BaseType);
        }

        public static void AutoSetUIToolkitElement(object obj, VisualElement viewObj, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            AutoSetUIToolkitElement(obj, viewObj, obj.GetType(), bindingFlags);
        }
        public static void AutoSetUIToolkitElement(object obj, VisualElement viewObj, Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            var fields = type.GetFields(bindingFlags);
            foreach (FieldInfo fieldInfo in fields)
            {
                VisualElement ve = viewObj.Q(fieldInfo.Name);
                if (ve != null) fieldInfo.SetValue(obj, ve);
            }
            
            var properties = type.GetProperties(bindingFlags);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!propertyInfo.CanWrite) continue;
                
                VisualElement ve = viewObj.Q(propertyInfo.Name);
                if (ve != null) propertyInfo.SetValue(obj, ve);
            }

            if (type.BaseType == null || type.BaseType == typeof(System.Object)) return;
            AutoSetUIToolkitElement(obj, viewObj, type.BaseType, bindingFlags);
        }
    }
}
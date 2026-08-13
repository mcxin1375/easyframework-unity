
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyFramework.Editor
{

    public class EditorToolExtension<T> where T : IEditorToolExtension
    {
        private static T[] _extensions;
        public static T[] Extensions
        {
            get
            {
                if (_extensions == null) Refresh();
                return _extensions;
            }
        }
        
        private static T[] _objArray;
        private static readonly List<T> TMPList = new();

        public static void Refresh()
        {
            TMPList.Clear();
            
            var types = EasyFrameworkReflection.FindInstanceTypes(typeof(T));
            var scriptableType = typeof(ScriptableObject);

            if (_objArray == null)
            {
                foreach (var type in types)
                {
                    if (scriptableType.IsAssignableFrom(type)) continue;

                    TMPList.Add(EasyFrameworkReflection.CreateInstance<T>(type));
                }

                _objArray = TMPList.ToArray();
            }
            else
            {
                TMPList.AddRange(_objArray);
            }

            foreach (var type in types)
            {
                if (scriptableType.IsAssignableFrom(type))
                {
                    var scriptableObjects = UnityEditorHelper.FindAssetsByType(type);
                    foreach (var scriptableObject in scriptableObjects)
                    {
                        if(scriptableObject is T toolExtension) TMPList.Add(toolExtension);
                    }
                }
            }
            _extensions = TMPList.OrderBy(item => item.Order).ToArray();
        }
    }
}
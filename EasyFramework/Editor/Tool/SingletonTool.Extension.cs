
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public abstract partial class SingletonTool<T>
    {
        private IToolEvent<T>[] _extensions;
        public IToolEvent<T>[] Extensions 
        {
            get
            {
                if (_extensions == null) RefreshExtensions();
                return _extensions;
            }
        }
        
        private IToolEvent<T>[] _objArray;
        private readonly List<IToolEvent<T>> _tmpList = new();

        public void RefreshExtensions()
        {
            _tmpList.Clear();
            
            var types = EasyFrameworkReflection.FindInstanceTypes(typeof(IToolEvent<T>));
            var scriptableType = typeof(ScriptableObject);

            if (_objArray == null)
            {
                foreach (var type in types)
                {
                    if (scriptableType.IsAssignableFrom(type)) continue;

                    _tmpList.Add(EasyFrameworkReflection.CreateInstance<IToolEvent<T>>(type));
                }

                _objArray = _tmpList.ToArray();
            }
            else
            {
                _tmpList.AddRange(_objArray);
            }

            foreach (var type in types)
            {
                if (scriptableType.IsAssignableFrom(type))
                {
                    var scriptableObjects = UnityEditorHelper.FindAssetsByType(type);
                    foreach (var scriptableObject in scriptableObjects)
                    {
                        if(scriptableObject is IToolEvent<T> toolExtension) _tmpList.Add(toolExtension);
                    }
                }
            }
            _extensions = _tmpList.OrderBy(item => item.Order).ToArray();
        }
        
        public virtual void Execute()
        {
            RefreshExtensions();

            var timeDebug = FDebug.StartTime();
            FDebug.Log($"[{GetType().Name} - {typeof(T).Name}] Execute");

            foreach (var extension in Extensions) extension.OnExecuteBefore();
            UpgradeVersion();
            foreach (var extension in Extensions) extension.OnExecute();
            foreach (var extension in Extensions) extension.OnExecuteAfter();
            
            FDebug.Log($"[{GetType().Name} - {typeof(T).Name}] Execute Completed! Time: {timeDebug.StopToSeconds():hh:mm:nn}");
            
            AssetDatabase.Refresh();
        }
    }
}
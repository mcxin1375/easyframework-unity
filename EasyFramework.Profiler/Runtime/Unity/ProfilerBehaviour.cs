/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/12/25
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Profiler
{
    internal class ProfilerBehaviour : SingletonBehaviour<ProfilerBehaviour>
    {
        private EasyFrameworkProfilerSettings Settings => EasyFrameworkProfilerSettings.Instance;
        private ErrorGUIBehaviour _errorGUIBehaviour;

        void Awake()
        {
            transform.SetParent(F.Behaviour.transform);
            
            _errorGUIBehaviour = gameObject.AddComponentEx<ErrorGUIBehaviour>();
        }

        public void OnException(string condition, string stackTrace, LogType type)
        {
            if (Settings.errorGUIBehaviour)
            {
                _errorGUIBehaviour.AddLog(condition, stackTrace, type);
            }
        }
    }
}
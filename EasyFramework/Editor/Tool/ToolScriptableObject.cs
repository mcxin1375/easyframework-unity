/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Editor
{
    public abstract class ToolScriptableObject<T> : ScriptableObject, IToolEvent<T> where T : SingletonTool<T>, new()
    {
        [Header("Base Settings")]
        public bool enabled = true;
        public int order;
        
        int IToolEvent<T>.Order => order;

        void IToolEvent<T>.OnExecute()
        {
            if (!enabled) return;
            
            OnExecute();
        }

        protected abstract void OnExecute();
    }
}
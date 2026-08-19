/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/1/22
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;

namespace EasyFramework.Editor
{
    public static class ToolExtension<T> where T : IToolExtensionObject
    {
        private static T[] _instances;
        public static T[] Instances => _instances ?? GetObjects();

        private static readonly List<T> _tmpList = new(); 
        
        public static T[] GetObjects(bool forceRefresh = false)
        {
            if (_instances == null || forceRefresh) Refresh();
            return _instances;
        }

        public static void Refresh()
        {
            _tmpList.Clear();

            foreach (var obj in ToolExtensionObjectPool.GetInstanceObjects<T>())
            {
                _tmpList.Add((T)obj);
            }

            foreach (var obj in ToolExtensionObjectPool.GetScriptableObjects<T>(true))
            {
                _tmpList.Add((T)obj);
            }

            _instances = _tmpList.OrderBy(item => item.Order).ToArray();
        }
    }
}
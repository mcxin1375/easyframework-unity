using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework
{
    internal class PoolManager : Singleton<PoolManager>, IPoolManager
    {
        private readonly Dictionary<string, ResPoolBehaviour> _resPoolDict = new();

        public GameObject Rent(string resName, Transform parent = null)
        {
            if (!_resPoolDict.TryGetValue(resName, out var resPool))
            {
                resPool = ResPoolBehaviour.Create(resName);
                _resPoolDict.Add(resName, resPool);
            }
            return resPool.Rent(parent);
        }
        public void Return(GameObject gameObject)
        {
            if (gameObject == null) return;
            gameObject.TryReturnPoolEx();
        }
        public void CreatePool(string resName, int preLoadCount = 0)
        {
            if (!_resPoolDict.TryGetValue(resName, out var resPool))
            {
                resPool = ResPoolBehaviour.Create(resName);
                _resPoolDict.Add(resName, resPool);
            }
            resPool.PreCreate(preLoadCount);
        }
        public void DestroyPool(string resName)
        {
            if (_resPoolDict.Remove(resName, out var resPool)) resPool.Destroy();
        }
        public void DestroyAllPool()
        {
            foreach (var resPool in _resPoolDict.Values) resPool.Destroy();
            _resPoolDict.Clear();
        }
    }
}
using UnityEngine;

namespace EasyFramework
{
    public interface IPoolManager
    {
        GameObject Rent(string resName, Transform parent = null);
        void Return(GameObject gameObject);
        void CreatePool(string resName, int preLoadCount = 0);
        void DestroyPool(string resName);
        void DestroyAllPool();
    }
}
using UnityEngine;

namespace EasyFramework.Samples.Modules
{
    // 异步入口会等待初始化；Rent/Return/Destroy 方法须在初始化后调用。
    // 先准备构建目录中的 PooledCube.prefab；此模板独占该资源名的池。
    // 跨帧使用时由业务持有租出对象，结束后 Return，全部归还后再 DestroyPool。
    public static class PoolExample
    {
        public static void Create(string resourceName, int preloadCount = 0)
            => F.PoolManager.CreatePool(resourceName, preloadCount);

        public static async ETask CreateAsync(string resourceName, int preloadCount = 0)
        {
            await F.InitializeAsync();
            Create(resourceName, preloadCount);
        }

        public static GameObject Rent(string resourceName, Transform parent = null)
            => F.PoolManager.Rent(resourceName, parent);

        public static void Return(GameObject instance) => F.PoolManager.Return(instance);

        // 仅由此资源池的拥有者在全部租借实例归还后销毁指定资源名的池。
        public static void Destroy(string resourceName) => F.PoolManager.DestroyPool(resourceName);

        // 此调用销毁全局所有资源池，不能用于清理单个业务模块。
        public static void DestroyAll() => F.PoolManager.DestroyAllPool();

        public static async ETask RunAsync(Transform parent = null)
        {
            await F.InitializeAsync();
            const string resourceName = "PooledCube";
            GameObject item = null;
            try
            {
                F.PoolManager.CreatePool(resourceName, 3);
                item = F.PoolManager.Rent(resourceName, parent);
                if (item != null)
                {
                    // 在这里使用 item。
                }
            }
            finally
            {
                if (item != null) F.PoolManager.Return(item);
                F.PoolManager.DestroyPool(resourceName);
            }
        }
    }
}

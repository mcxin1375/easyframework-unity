using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFramework.Samples.Modules
{
    // 异步入口会等待初始化；下列同步查询/加载方法须由调用方先等待 F.InitializeAsync()。
    // 先准备资源构建目录中的资源；跨帧持有时由业务持有同一请求并在结束后释放。
    public static class ResourceExample
    {
        public static AssetBundle LoadBundle(string resourceName, IResRequest request = null)
            => F.ResLoader.Load(resourceName, request);

        public static async ETask<AssetBundle> LoadBundleAsync(string resourceName, IResRequest request = null)
        {
            await F.InitializeAsync();
            return await F.ResLoader.LoadAsync(resourceName, request);
        }

        public static T LoadAsset<T>(string resourceName, IResRequest request = null) where T : Object
            => F.ResLoader.LoadAsset<T>(resourceName, request);

        public static async ETask<T> LoadAssetAsync<T>(string resourceName, IResRequest request = null) where T : Object
        {
            await F.InitializeAsync();
            return await F.ResLoader.LoadAssetAsync<T>(resourceName, request);
        }

        public static T[] LoadAllAssets<T>(string resourceName, IResRequest request = null) where T : Object
            => F.ResLoader.LoadAllAssets<T>(resourceName, request);

        public static async ETask<T[]> LoadAllAssetsAsync<T>(string resourceName, IResRequest request = null) where T : Object
        {
            await F.InitializeAsync();
            return await F.ResLoader.LoadAllAssetsAsync<T>(resourceName, request);
        }

        public static GameObject Create(string resourceName, Transform parent = null, IResRequest request = null)
            => F.ResLoader.CreateObj(resourceName, parent, request);

        // 使用 Unity Resources 路径，不经过 AssetBundle；返回实例由调用方 Destroy。
        public static GameObject CreateFromResources(string resourcesPath, Transform parent = null)
            => F.ResLoader.CreateObjResources(resourcesPath, parent);

        public static async ETask<GameObject> CreateAsync(string resourceName, Transform parent = null, IResRequest request = null)
        {
            await F.InitializeAsync();
            return await F.ResLoader.CreateObjAsync(resourceName, parent, request);
        }

        public static async ETask<GameObject[]> CreateManyAsync(string resourceName, int count, Transform parent = null, IResRequest request = null)
        {
            await F.InitializeAsync();
            return await F.ResLoader.CreateObjAsync(resourceName, count, parent, request);
        }

        // 普通 Unload 仅结束传入请求的持有；加载资产的其他活跃请求继续持有资源。
        public static void Unload(string resourceName, IResRequest request = null)
            => F.ResLoader.Unload(resourceName, request);

        // 强制卸载会绕过所有持有者，仅在该 Bundle 已无业务使用时显式调用。
        public static void UnloadForce(string resourceName, bool unloadLoadedObjects = false)
            => F.ResLoader.UnloadForce(resourceName, unloadLoadedObjects);

        // 全局强制卸载所有 Bundle，仅用于应用级清理，不用于单个模块释放。
        public static void UnloadAllForce(bool unloadLoadedObjects = false)
            => F.ResLoader.UnloadAllForce(unloadLoadedObjects);

        public static bool IsLoading(string resourceName) => F.ResLoader.IsLoading(resourceName);
        public static bool IsUnloading(string resourceName) => F.ResLoader.IsUnloading(resourceName);
        public static float GetLoadingProgress(string resourceName) => F.ResLoader.GetLoadingProgress(resourceName);
        public static string[] GetDependencies(string resourceName) => F.ResLoader.GetAllDependencies(resourceName);

        // 这是直接的资源场景原语；一般业务使用 SceneLoader 以获得状态跟踪和活动场景管理。
        public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IResRequest request = null)
            => F.ResLoader.LoadScene(sceneName, mode, request);

        public static async ETask<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IResRequest request = null)
        {
            await F.InitializeAsync();
            return await F.ResLoader.LoadSceneAsync(sceneName, mode, request);
        }

        public static async ETask<AsyncOperation> UnloadSceneAsync(string sceneName, IResRequest request = null)
        {
            await F.InitializeAsync();
            return await F.ResLoader.UnloadSceneAsync(sceneName, request);
        }

        public static async ETask RunAsync(string resourceName = "Cube", Transform parent = null)
        {
            await F.InitializeAsync();
            var request = new Request();
            GameObject instance = null;
            try
            {
                instance = await F.ResLoader.CreateObjAsync(resourceName, parent, request);
                if (instance != null)
                {
                    // 在这里使用 instance；加载失败可能返回 null。
                }
            }
            finally
            {
                if (instance != null) Object.Destroy(instance);
                request.Alive = false;
                F.ResLoader.Unload(resourceName, request);
            }
        }

        private sealed class Request : IResRequest
        {
            public bool Alive { get; set; } = true;
        }
    }
}

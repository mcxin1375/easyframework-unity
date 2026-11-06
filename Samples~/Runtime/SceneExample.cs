using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFramework.Samples.Modules
{
    // 异步入口会等待初始化；同步 Load 和查询/激活方法须在初始化后调用。
    // 先准备资源构建目录中的 MyRoom.unity；Additive 使用结束后等待 UnloadAsync，再丢弃场景对象引用。
    public static class SceneExample
    {
        public static string CurrentActiveScene => F.SceneLoader.CurrentActiveScene;

        // Single 会替换当前场景；需要保留时显式使用 Additive。
        public static void Load(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true)
            => F.SceneLoader.LoadScene(sceneName, mode, activateOnLoad);

        public static async ETask LoadAsync(
            string sceneName = "MyRoom",
            LoadSceneMode mode = LoadSceneMode.Additive,
            bool activateOnLoad = false)
        {
            await F.InitializeAsync();
            await F.SceneLoader.LoadSceneAsync(sceneName, mode, activateOnLoad);
            if (!F.SceneLoader.IsLoaded(sceneName))
                throw new InvalidOperationException("场景未加载，请检查场景资源名和构建配置。");
        }

        public static GameObject FindRoot(string sceneName, string rootName)
        {
            return F.SceneLoader.FindSceneRootObj(sceneName, rootName);
        }

        public static async ETask UnloadAsync(string sceneName = "MyRoom")
        {
            await F.InitializeAsync();
            await F.SceneLoader.UnloadSceneAsync(sceneName);
        }

        // 全局卸载所有由 SceneLoader 管理的场景，仅在应用级场景切换时使用。
        public static async ETask UnloadAllAsync()
        {
            await F.InitializeAsync();
            await F.SceneLoader.UnloadAllSceneAsync();
        }

        public static float GetLoadProgress(string sceneName) => F.SceneLoader.GetLoadProgress(sceneName);
        public static float GetUnloadProgress(string sceneName) => F.SceneLoader.GetUnloadProgress(sceneName);
        public static bool IsLoaded(string sceneName) => F.SceneLoader.IsLoaded(sceneName);
        public static bool IsLoading(string sceneName) => F.SceneLoader.IsLoading(sceneName);
        public static bool IsUnloading(string sceneName) => F.SceneLoader.IsUnloading(sceneName);
        public static Scene GetScene(string sceneName) => F.SceneLoader.GetScene(sceneName);
        public static void SetActive(string sceneName) => F.SceneLoader.SetActive(sceneName);
        public static GameObject FindActiveSceneRoot(string rootName) => F.SceneLoader.FindActiveSceneRootObj(rootName);
    }
}

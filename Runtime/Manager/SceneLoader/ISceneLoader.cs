using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFramework
{
    public interface ISceneLoader
    {
        string CurrentActiveScene { get; }

        void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true);
        ETask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true);
        ETask UnloadSceneAsync(string sceneName);
        ETask UnloadAllSceneAsync();
        float GetLoadProgress(string sceneName);
        float GetUnloadProgress(string sceneName);
        bool IsLoaded(string sceneName);
        bool IsLoading(string sceneName);
        bool IsUnloading(string sceneName);
        Scene GetScene(string sceneName);
        void SetActive(string sceneName);
        GameObject FindActiveSceneRootObj(string name);
        GameObject FindSceneRootObj(string sceneName, string name);
    }
}
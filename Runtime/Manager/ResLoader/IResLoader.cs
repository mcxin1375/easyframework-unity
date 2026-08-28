using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFramework
{
    public interface IResLoader
    {
        AssetBundle Load(string abName, IResRequest request = null);
        ETask<AssetBundle> LoadAsync(string abName, IResRequest request = null);
        void Unload(string abName, IResRequest request = null);
        void UnloadForce(string abName, bool unloadAllLoadedObjects = false);
        void UnloadAllForce(bool unloadAllLoadedObjects = false);
        bool IsLoading(string abName);
        bool IsUnloading(string abName);
        float GetLoadingProgress(string abName);
        string[] GetAllDependencies(string abName);
        
        T LoadAsset<T>(string abName, IResRequest request = null) where T : UnityEngine.Object;
        T[] LoadAllAssets<T>(string abName, IResRequest request = null) where T : UnityEngine.Object;
        ETask<T> LoadAssetAsync<T>(string abName, IResRequest request = null) where T : UnityEngine.Object;
        ETask<T[]> LoadAllAssetsAsync<T>(string abName, IResRequest request = null) where T : UnityEngine.Object;
        
        void LoadScene(string sceneNam, LoadSceneMode mode = LoadSceneMode.Single, IResRequest request = null);
        ETask<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IResRequest request = null);
        ETask<AsyncOperation> UnloadSceneAsync(string sceneName, IResRequest handler = null);
        
        // string GetResFile(string fullName);
        // string LoadDataAllText(string fullName);
        // byte[] LoadDataAllBytes(string fullName);
    }
}
/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EasyFramework.Editor
{
    internal class AssetBundleLoaderEditor : Singleton<AssetBundleLoaderEditor>, IResLoader
    {
        private readonly Dictionary<string, AssetBundleBuild> _assetsMap;

        public AssetBundleLoaderEditor()
        {
            var assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();
            _assetsMap = assetBundleBuilds.ToDictionary(item => item.assetBundleName.Replace(EasyFrameworkConst.ABSuffix, ""), item => item);
        }

        public ETask InitializeAsync() => ETask.CompletedTask;
        public string GetFilePath(string abName)
        {
            if (_assetsMap.TryGetValue(abName, out var info)) return info.assetNames[0];
            return string.Empty;
        }

        public AssetBundle Load(string abName, IResRequest request = null) => null;
        public ETask<AssetBundle> LoadAsync(string abName, IResRequest request = null) => ETask.FromResult<AssetBundle>(null);
        public void Unload(string abName, IResRequest request = null) { }
        public void UnloadForce(string abName, bool unloadAllLoadedObjects = false) { }
        public void UnloadAllForce(bool unloadAllLoadedObjects = false) { }
        public bool IsLoading(string abName) => false;
        public bool IsUnloading(string abName) => false;
        public float GetLoadingProgress(string abName) => 0;
        public string[] GetAllDependencies(string abName) => null;
        
        public T LoadAsset<T>(string abName, IResRequest request = null) where T : Object
        {
            if (_assetsMap.TryGetValue(abName, out var abBuild))
            {
                for (int i = 0; i < abBuild.assetNames.Length; i++)
                {
                    var file = abBuild.assetNames[i];
                    T t = AssetDatabase.LoadAssetAtPath<T>(file);
                    if (t != null) return t;
                }
            }

            return null;
        }
        public T[] LoadAllAssets<T>(string abName, IResRequest request = null) where T : Object
        {
            if (_assetsMap.TryGetValue(abName, out var abBuild))
            {
                List<T> tmp = new List<T>();
                for (int i = 0; i < abBuild.assetNames.Length; i++)
                {
                    var file = abBuild.assetNames[i];
                    Object[] arr = AssetDatabase.LoadAllAssetsAtPath(file);
                    foreach (Object o in arr)
                        if (o is T t)
                            tmp.Add(t);
                }

                return tmp.ToArray();
            }

            return null;
        }
        public ETask<T> LoadAssetAsync<T>(string abName, IResRequest request = null) where T : Object => ETask.FromResult(LoadAsset<T>(abName, request));
        public ETask<T[]> LoadAllAssetsAsync<T>(string abName, IResRequest request = null) where T : Object => ETask.FromResult(LoadAllAssets<T>(abName, request));
        
        
        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IResRequest request = null)
        {
            string scenePath = GetFilePath(sceneName);
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(mode));
        }
        public ETask<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IResRequest request = null)
        {
            string scenePath = GetFilePath(sceneName);
            return ETask.FromResult(UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(mode)));
        }
        public ETask<AsyncOperation> UnloadSceneAsync(string sceneName, IResRequest handler = null)
        {
            // string scenePath = F.ResLoader.GetAssetPath(sceneName);
            return ETask.FromResult(UnityEditor.SceneManagement.EditorSceneManager.UnloadSceneAsync(sceneName));
        }
    }
}
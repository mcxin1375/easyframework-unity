/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
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
            var assetBundleBuilds = AssetBundleBuilderUtility.CreateAssetBundleBuildBySettings();
            _assetsMap = assetBundleBuilds.ToDictionary(item => NormalizeAbName(item.assetBundleName), item => item);
        }

        public string GetFilePath(string abName)
        {
            if (_assetsMap.TryGetValue(NormalizeAbName(abName), out var info) &&
                info.assetNames?.Length > 0)
                return info.assetNames[0];
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
            if (!_assetsMap.TryGetValue(NormalizeAbName(abName), out var abBuild)) return null;
            return LoadFirstAsset<T>(abBuild);
        }
        public T LoadAsset<T>(string abName, string assetName, IResRequest request = null) where T : Object
        {
            if (!_assetsMap.TryGetValue(NormalizeAbName(abName), out var abBuild)) return null;

            var assetNames = abBuild.assetNames ?? Array.Empty<string>();
            for (var i = 0; i < assetNames.Length; i++)
            {
                var assetPath = assetNames[i];
                var addressableName = abBuild.addressableNames?.Length == assetNames.Length
                    ? abBuild.addressableNames[i]
                    : Path.GetFileNameWithoutExtension(assetPath);
                if (addressableName != assetName) continue;
                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }

            return null;
        }

        private static T LoadFirstAsset<T>(AssetBundleBuild abBuild) where T : Object
        {
            foreach (var assetPath in abBuild.assetNames ?? Array.Empty<string>())
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null) return asset;
            }

            return null;
        }
        public T[] LoadAllAssets<T>(string abName, IResRequest request = null) where T : Object
        {
            if (_assetsMap.TryGetValue(NormalizeAbName(abName), out var abBuild))
            {
                List<T> tmp = new List<T>();
                foreach (var file in abBuild.assetNames ?? Array.Empty<string>())
                {
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
        public ETask<T> LoadAssetAsync<T>(string abName, string assetName, IResRequest request = null) where T : Object => ETask.FromResult(LoadAsset<T>(abName, assetName, request));
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

        private static string NormalizeAbName(string abName)
        {
            var suffix = EasyFrameworkSettings.Instance.abSuffix;
            return abName.EndsWith(suffix, StringComparison.Ordinal) ? abName.Substring(0, abName.Length - suffix.Length) : abName;
        }
    }
}

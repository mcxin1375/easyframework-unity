/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/


using System;
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
            var assetBundleBuilds = AssetBundleBuilderUtility.CreateAssetBundleBuildBySettings();
            _assetsMap = assetBundleBuilds.ToDictionary(build => build.assetBundleName, StringComparer.Ordinal);
        }

        public string GetFilePath(string abName)
        {
            if (!TryGetBuild(abName, out var build) || build.assetNames.Length == 0) return string.Empty;

            var path = FindAssetPath(build, WithoutBundleSuffix(abName), out var caseMismatch);
            return path ?? (caseMismatch ? string.Empty : build.assetNames[0]);
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
            if (!TryGetBuild(abName, out var build)) return null;

            var path = FindAssetPath(build, WithoutBundleSuffix(abName), out var caseMismatch);
            if (path != null) return AssetDatabase.LoadAssetAtPath<T>(path);
            return caseMismatch ? null : LoadFirstAsset<T>(build);
        }
        public T LoadAsset<T>(string abName, string assetName, IResRequest request = null) where T : Object
        {
            if (!TryGetBuild(abName, out var build)) return null;

            var path = FindAssetPath(build, assetName, out _);
            return path == null ? null : AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static string FindAssetPath(AssetBundleBuild build, string assetName, out bool caseMismatch)
        {
            caseMismatch = false;
            for (var i = 0; i < build.addressableNames.Length; i++)
            {
                var address = build.addressableNames[i];
                if (address == assetName) return build.assetNames[i];
                if (string.Equals(address, assetName, StringComparison.OrdinalIgnoreCase)) caseMismatch = true;
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
            if (TryGetBuild(abName, out var abBuild))
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
            var scenePath = GetFilePath(sceneName);
            return ETask.FromResult(UnityEditor.SceneManagement.EditorSceneManager.UnloadSceneAsync(string.IsNullOrEmpty(scenePath) ? sceneName : scenePath));
        }

        private bool TryGetBuild(string abName, out AssetBundleBuild build)
        {
            if (_assetsMap.TryGetValue(abName, out build)) return true;

            var nameKey = AssetBundleLoaderHelper.NameToKey(abName);
            if (!_assetsMap.TryGetValue(nameKey, out build)) return false;

            _assetsMap.Add(abName, build);
            return true;
        }

        private static string WithoutBundleSuffix(string abName)
        {
            var suffix = EasyFrameworkSettings.Instance.abSuffix;
            return abName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? abName.Substring(0, abName.Length - suffix.Length) : abName;
        }
    }
}

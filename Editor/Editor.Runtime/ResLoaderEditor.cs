// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Build.Pipeline;
// using UnityEngine.SceneManagement;
// using Object = UnityEngine.Object;
//
// namespace EasyFramework.Editor
// {
//     public class ResLoaderEditor : IResLoader
//     {
//         private readonly Dictionary<string, AssetBundleBuild> _assetsMap;
//
//         public ResLoaderEditor()
//         {
//             var assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();
//             _assetsMap = assetBundleBuilds.ToDictionary(item => item.assetBundleName.Replace(EasyFrameworkSettings.AssetBundleSuffix, ""), item => item);
//         }
//
//         public string GetFilePath(string abName)
//         {
//             if (_assetsMap.TryGetValue(abName, out var info)) return info.assetNames[0];
//             return string.Empty;
//         }
//
//         public void Load(string abName, IAssetBundleRequest handler = null)
//         {
//         }
//         public void Unload(string abName, IAssetBundleRequest handler = null)
//         {
//         }
//         public bool IsLoading(string abName)
//         {
//             return false;
//         }
//         public bool IsUnloading(string abName)
//         {
//             return false;
//         }
//         public float GetLoadingProgress(string abName)
//         {
//             return 0;
//         }
//
//         public T LoadAsset<T>(string abName, IAssetBundleRequest request = null) where T : Object
//         {
//             if (_assetsMap.TryGetValue(abName, out var abBuild))
//             {
//                 for (int i = 0; i < abBuild.assetNames.Length; i++)
//                 {
//                     var file = abBuild.assetNames[i];
//                     T t = AssetDatabase.LoadAssetAtPath<T>(file);
//                     if (t != null) return t;
//                 }
//             }
//
//             return null;
//         }
//         public T[] LoadAllAssets<T>(string abName, IAssetBundleRequest request = null) where T : Object
//         {
//             if (_assetsMap.TryGetValue(abName, out var abBuild))
//             {
//                 List<T> tmp = new List<T>();
//                 for (int i = 0; i < abBuild.assetNames.Length; i++)
//                 {
//                     var file = abBuild.assetNames[i];
//                     Object[] arr = AssetDatabase.LoadAllAssetsAtPath(file);
//                     foreach (Object o in arr)
//                         if (o is T t)
//                             tmp.Add(t);
//                 }
//
//                 return tmp.ToArray();
//             }
//
//             return null;
//         }
//         public ETask<T> LoadAssetAsync<T>(string abName, IAssetBundleRequest request = null) where T : Object => EasyTask.FromResult(LoadAsset<T>(abName, request));
//         public EasyTask<T[]> LoadAllAssetsAsync<T>(string abName, IAssetBundleRequest request = null) where T : Object => EasyTask.FromResult(LoadAllAssets<T>(abName, request));
//
//         public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IAssetBundleRequest handler = null)
//         {
//             string scenePath = GetFilePath(sceneName);
//             UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(mode));
//         }
//         public EasyTask<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, IAssetBundleRequest handler = null)
//         {
//             string scenePath = GetFilePath(sceneName);
//             return EasyTask.FromResult(UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(mode)));
//         }
//         public EasyTask<AsyncOperation> UnloadSceneAsync(string sceneName, IAssetBundleRequest handler = null)
//         {
//             // string scenePath = F.ResLoader.GetAssetPath(sceneName);
//             return EasyTask.FromResult(UnityEditor.SceneManagement.EditorSceneManager.UnloadSceneAsync(sceneName));
//         }
//
//     }
// }
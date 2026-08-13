// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2020/4/20
// // describe:
// //----------------------------------------------------------------*/
//
//
// using System;
// using EasyFramework;
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public class AssetBundleDebugWindow : EditorWindow
//     {
//         public static void Init()
//         {
//             AssetBundleDebugWindow debugWindow = (AssetBundleDebugWindow)EditorWindow.GetWindow(typeof(AssetBundleDebugWindow), false, "AssetBundleEditorWindow", true);
//             debugWindow.Show();
//         }
//
//         enum EDrawType
//         {
//             GameObjectPool,
//             TPAtlasPool,
//             ScenePool,
//             AssetBundleInfo,
//             AssetBundle
//         }
//
//         Vector2 scrollVec;
//         float intervalTime = 1;
//         float addTime;
//         private string _tab = EDrawType.AssetBundleInfo.ToString();
//         
//         private string _searchText;
//         private bool _selectLoaded = true;
//         private bool _selectUnloaded;
//         private bool _selectRefer = true;
//         private bool _selectDeps = true;
//
//         protected void Awake()
//         {
//             addTime = 0;
//         }
//
//         protected void Update()
//         {
//             addTime += Time.fixedTime;
//             if (addTime > intervalTime)
//             {
//                 addTime = 0;
//                 Repaint();
//             }
//         }
//
//         protected void OnDestroy()
//         {
//
//         }
//
//         protected void OnGUI()
//         {
//             GUILayout.BeginHorizontal();
//
//             string[] tabArr = Enum.GetNames(typeof(EDrawType));
//             foreach (string s in tabArr)
//             {
//                 GUI.color = _tab == s ? Color.grey : Color.white;
//                 if (GUILayout.Button(s, GUILayout.Width(position.width / tabArr.Length), GUILayout.Height(60)))
//                 {
//                     _tab = s;
//                 }
//             }
//             EDrawType drawType = Enum.Parse<EDrawType>(_tab);
//
//             GUI.color = Color.white;
//
//             GUILayout.EndHorizontal();
//
//             _searchText = GUILayout.TextField(_searchText);
//             GUILayout.BeginHorizontal();
//             _selectLoaded = GUILayout.Toggle(_selectLoaded, "Loaded");
//             _selectUnloaded = GUILayout.Toggle(_selectUnloaded, "Unloaded");
//             _selectRefer = GUILayout.Toggle(_selectRefer, "Refer");
//             _selectDeps = GUILayout.Toggle(_selectDeps, "Deps");
//             GUILayout.EndHorizontal();
//             
//             if (!Application.isPlaying)
//             {
//                 GUILayout.Space(10);
//                 GUILayout.Label("游戏未运行", GUILayout.Width(200));
//                 return;
//             }
//
//             GUILayout.Space(10);
//             GUILayout.BeginHorizontal();
//
//             scrollVec = GUILayout.BeginScrollView(scrollVec);
//             if (drawType == EDrawType.GameObjectPool)
//             {
//                 var dict = F.ResSystem.ResPoolDict;
//                 foreach (var keyValuePair in dict)
//                 {
//                     var info = keyValuePair.Value;
//                     if (!string.IsNullOrWhiteSpace(_searchText) && !info.ResName.ToLower().Contains(_searchText.ToLower())) continue;
//                     GUILayout.Label($"ResName: {info.ResName}");
//                 }
//             }
//             else if (drawType == EDrawType.ScenePool)
//             {
//                 foreach (var keyValuePair in F.SceneSystem.ComponentDict)
//                 {
//                     var info = keyValuePair.Value;
//                     
//                     bool loaded = _selectLoaded && (info.State == EResState.Loaded || info.State == EResState.Loading);
//                     bool unloaded = _selectUnloaded && (info.State == EResState.Unloaded || info.State == EResState.Unloading);
//                     if (!loaded && !unloaded) continue;
//                     
//                     if (!string.IsNullOrWhiteSpace(_searchText) && !info.SceneName.ToLower().Contains(_searchText.ToLower())) continue;
//                     
//                     GUILayout.Label($"AssetName: {info.SceneName}, State: {info.State}, Mode: {info.Mode}, IsLoaded:{info.Scene.isLoaded}, IsActive:{info.IsActive}");
//                 }
//             }
//             else if (drawType == EDrawType.AssetBundleInfo)
//             {
//                 var dict = F.AssetBundleLoader.ABDict;
//                 if (dict != null)
//                 {
//                     foreach (var keyValuePair in dict)
//                     {
//                         var info = keyValuePair.Value;
//
//                         bool loaded = _selectLoaded && (info.State == AssetBundleState.Loaded || info.State == AssetBundleState.Loading);
//                         bool unloaded = _selectUnloaded && (info.State == AssetBundleState.Unloaded || info.State == AssetBundleState.Unloading);
//                         if (!loaded && !unloaded) continue;
//
//                         if (!string.IsNullOrWhiteSpace(_searchText) && !info.AssetName.ToLower().Contains(_searchText.ToLower())) continue;
//
//                         int lifeTime = Mathf.FloorToInt(EasyFrameworkSettings.Instance.assetBundleUnloadTime + info.RequestTime - Time.time);
//                         lifeTime = lifeTime < 0 ? 0 : lifeTime;
//                         // GUILayout.Label($"AssetName: {info.AssetName}, State: {info.State}, LoadProgress: {info.LoadProgress}, LifeTime: {lifeTime}");
//                         GUILayout.Label($"AssetName: {info.AssetName}, State: {info.State}, LifeTime: {lifeTime}");
//                         if (_selectRefer)
//                         {
//                             if (info.ReferNum > 0)
//                             {
//                                 foreach (string assetName in info.ReferList)
//                                 {
//                                     GUILayout.Label($"             Refer: {assetName}");
//                                 }
//                             }
//                         }
//                         if (_selectDeps)
//                         {
//                             if (info.Deps?.Length > 0)
//                             {
//                                 foreach (string v in info.Deps)
//                                 {
//                                     GUILayout.Label($"             Dep: {v}");
//                                 }
//                             }
//                         }
//                     }
//                 }
//             }
//             else if (drawType == EDrawType.AssetBundle)
//             {
//                 var abs = AssetBundle.GetAllLoadedAssetBundles();
//                 foreach (var assetBundle in abs)
//                 {
//                     if (!string.IsNullOrWhiteSpace(_searchText) && !assetBundle.name.ToLower().Contains(_searchText.ToLower())) continue;
//                     GUILayout.Label($"assetBundle: {assetBundle.name}");
//                 }
//             }
//
//             GUILayout.EndScrollView();
//             GUILayout.EndHorizontal();
//
//         }
//
//     }
// }
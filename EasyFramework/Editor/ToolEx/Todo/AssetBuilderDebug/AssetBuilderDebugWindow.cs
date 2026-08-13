// using UnityEditor;
// using UnityEngine.Build.Pipeline;
// using UnityEngine.UIElements;
//
// namespace EasyFramework.Editor
// {
//     public class AssetBuilderDebugWindow : UIToolkitEditorWindow<AssetBuilderDebugWindow>
//     {
//         public TextElement LabBuildPath;
//         public TextElement LabBuildCount;
//         public TextField TextFieldSearch;
//         
//         public AssetBuilderDebugListViewEx ListViewEx = new AssetBuilderDebugListViewEx();
//         public AssetBuilderDebugTreeViewEx TreeViewEx = new AssetBuilderDebugTreeViewEx();
//
//         private AssetBundleBuild[] _assetBundleBuilds;
//         private AssetBundleBuildFileTreeDebug _assetBundleBuildFileTreeDebug;
//         private AssetBundleBuildDepsDebug _assetBundleBuildDepsDebug;
//         private SVCInfo _svcInfo;
//         private CompatibilityAssetBundleManifest _compatibilityAssetBundleManifest;
//         
//         private string _searchStr;
//         private string _selectTab;
//         
//         protected override void OnOpen()
//         {
//             base.OnOpen();
//
//             ListViewEx.Refresh(new[]
//             {
//                 "AssetBundleBuildDebug",
//                 "AssetBundleManifestDebug",
//                 nameof(AssetBundleBuildFileTreeDebug), 
//                 nameof(AssetBundleBuildDepsDebug),
//                 "ShaderVariantCollectionDebug"
//             });
//
//             LabBuildPath.text = $"AssetBundlePath: {AssetBundleBuilder.Instance.ProjectDataPath}";
//
//             _assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();
//             LabBuildCount.text = $"AssetBundleBuild Count: {_assetBundleBuilds.Length}";
//
//             // var sw = new System.Diagnostics.Stopwatch();
//             // sw.Start();
//             //
//             // var list = AssetBundleBuilderSettings.Instance.CreateAssetBundleBuilds();
//             // var sortArr = list.OrderBy(item => item.assetBundleName).ToArray();
//             // TreeViewEx.RefreshList(sortArr);
//             //
//             // sw.Stop();
//             // Debug.Log($"{sw.Elapsed:hh\\:mm\\:ss\\.fff}");
//
//         }
//
//         private void OnInspectorUpdate()
//         {
//             if (_searchStr != TextFieldSearch.value)
//             {
//                 _searchStr = TextFieldSearch.value;
//                 SelectTab(_selectTab);
//             }
//         }
//
//         public void SelectTab(string tab)
//         {
//             _selectTab = tab;
//             switch (tab)
//             {
//                 case "AssetBundleBuildDebug":
//                     TreeViewEx.UpdateAssetBundleList(_assetBundleBuilds);
//                     break;
//                 case nameof(AssetBundleBuildFileTreeDebug):
//
//                     if (_assetBundleBuildFileTreeDebug == null)
//                         _assetBundleBuildFileTreeDebug = new AssetBundleBuildFileTreeDebug(_assetBundleBuilds);
//                     TreeViewEx.UpdateAssetBundleBuildTreeInfo(_assetBundleBuildFileTreeDebug);
//
//                     break;
//                 
//                 case nameof(AssetBundleBuildDepsDebug):
//                     if (_assetBundleBuildDepsDebug == null)
//                         _assetBundleBuildDepsDebug = new AssetBundleBuildDepsDebug(_assetBundleBuilds);
//                     TreeViewEx.UpdateAssetBundleDeps(_assetBundleBuildDepsDebug);
//                     break;
//                 
//                 case "ShaderVariantCollectionDebug":
//                     if (_svcInfo == null)
//                         _svcInfo = SVCCollector.Instance.CreateShaderVariantCollectionInfo();
//                     TreeViewEx.UpdateShaderVariantCollection(_svcInfo);
//                     break;
//                 
//                 case "AssetBundleManifestDebug":
//                     // if (_compatibilityAssetBundleManifest == null)
//                         // _compatibilityAssetBundleManifest = AssetDatabase.LoadAssetAtPath<CompatibilityAssetBundleManifest>(AssetBundleBuilderHelper.ManifestFile);
//                     // TreeViewEx.Update(_compatibilityAssetBundleManifest);
//                     break;
//             }
//         }
//
//
//     }
// }
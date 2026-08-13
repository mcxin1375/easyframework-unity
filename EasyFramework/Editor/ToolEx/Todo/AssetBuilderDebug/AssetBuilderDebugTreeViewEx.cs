// using System.Collections.Generic;
// using System.Linq;
// using EasyFramework;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Build.Pipeline;
// using UnityEngine.UIElements;
//
// namespace EasyFramework.Editor
// {
//     public class AssetBuilderDebugTreeViewEx : UIToolkitTreeViewEx<AssetBuilderDebugWindow>
//     {
//         protected override string ViewName => "TreeView";
//
//         public void UpdateAssetBundleList(AssetBundleBuild[] assetBundleBuilds)
//         {
//             var items = new List<TreeViewItemData<string>>();
//             int addCount = 0;
//
//             bool search = !string.IsNullOrWhiteSpace(Window.TextFieldSearch.value);
//             string searchStr = Window.TextFieldSearch.value.ToLower();
//             
//             TreeViewItemData<string> CreateTreeViewItemData(int rootIndex, AssetBundleBuild assetBundleBuild)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//                 for (int i = 0; i < assetBundleBuild.assetNames.Length; i++)
//                 {
//                     var assetName = assetBundleBuild.assetNames[i];
//                     var addressableName = assetBundleBuild.addressableNames[i];
//                     addCount++;
//                     var fileStr = $"{addressableName} : {assetName}";
//                     treeViewSubItemsData.Add(new TreeViewItemData<string>(addCount, fileStr));
//                 }
//
//                 var content = $"{assetBundleBuild.assetBundleName}  (files: {assetBundleBuild.assetNames.Length})";
//                 // Debug.Log($"{rootIndex}, {content}");
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, content, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             foreach (var assetBundleLogInfo in assetBundleBuilds)
//             {
//                 if (search && !assetBundleLogInfo.assetBundleName.ToLower().Contains(searchStr)) continue;
//
//                 items.Add(CreateTreeViewItemData(addCount, assetBundleLogInfo));
//                 addCount++;
//             }
//
//             TreeView.SetRootItems(items);
//             TreeView.selectionType = SelectionType.Multiple;
//             TreeView.Rebuild();
//         }
//
//         public void UpdateAssetBundleBuildTreeInfo(AssetBundleBuildFileTreeDebug assetBundleBuildFileTreeDebug)
//         {
//             var items = new List<TreeViewItemData<string>>();
//             int addCount = 0;
//
//             bool search = !string.IsNullOrWhiteSpace(Window.TextFieldSearch.value);
//             if (search)
//             {
//                 string searchStr = Window.TextFieldSearch.value.ToLower();
//
//                 void CreateTreeViewItemData(int rootIndex, AssetBundleBuildFileTreeDebug treeInfo)
//                 {
//                     foreach (var childTreeInfo in treeInfo.ChildDict.Values)
//                     {
//                         addCount++;
//                         CreateTreeViewItemData(addCount, childTreeInfo);
//                     }
//
//                     foreach (var keyValue in treeInfo.ChildFiles)
//                     {
//                         if (search && !keyValue.Key.ToLower().Contains(searchStr)) continue;
//                     
//                         addCount++;
//                         var fileStr = $"{keyValue.Key} : {FormatHelper.FormatByte(keyValue.Value)}";
//                         items.Add(new TreeViewItemData<string>(addCount, fileStr));
//                     }
//
//                     if (treeInfo.RootDirectory.ToLower().Contains(searchStr))
//                     {
//                         var content = $"{treeInfo.RootDirectory} : {FormatHelper.FormatByte(treeInfo.TotalSize)}";
//                         var treeViewItemData = new TreeViewItemData<string>(rootIndex, content);
//                         items.Add(treeViewItemData);
//                     }
//                 }
//
//                 foreach (var keyValue in assetBundleBuildFileTreeDebug.ChildDict)
//                 {
//                     CreateTreeViewItemData(addCount, keyValue.Value);
//                     addCount++;
//                 }
//             }
//             else
//             {
//                 TreeViewItemData<string> CreateTreeViewItemData(int rootIndex, AssetBundleBuildFileTreeDebug treeInfo)
//                 {
//                     var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//                     foreach (var childTreeInfo in treeInfo.ChildDict.Values)
//                     {
//                         addCount++;
//                         var createInfo = CreateTreeViewItemData(addCount, childTreeInfo);
//                         treeViewSubItemsData.Add(createInfo);
//                     }
//
//                     foreach (var keyValue in treeInfo.ChildFiles)
//                     {
//                         addCount++;
//                         var fileStr = $"{keyValue.Key} : {FormatHelper.FormatByte(keyValue.Value)}";
//                         // Debug.Log($"{addCount}, {fileStr}");
//                         treeViewSubItemsData.Add(new TreeViewItemData<string>(addCount, fileStr));
//                     }
//                 
//                     var content = $"{treeInfo.RootDirectory} : {FormatHelper.FormatByte(treeInfo.TotalSize)}";
//                     // Debug.Log($"{rootIndex}, {content}");
//                     var treeViewItemData = new TreeViewItemData<string>(rootIndex, content, treeViewSubItemsData);
//                     return treeViewItemData;
//                 }
//
//                 foreach (var keyValue in assetBundleBuildFileTreeDebug.ChildDict)
//                 {
//                     items.Add(CreateTreeViewItemData(addCount, keyValue.Value));
//                     addCount++;
//                 }
//             }
//
//             TreeView.SetRootItems(items);
//             TreeView.selectionType = SelectionType.Multiple;
//             TreeView.Rebuild();
//         }
//
//         public void UpdateAssetBundleDeps(AssetBundleBuildDepsDebug assetBundleBuildDepsDebug)
//         {
//             var sortArr = assetBundleBuildDepsDebug.DepFiles.Values.OrderByDescending(item => item.DepList.Count)
//                 .ToArray();
//             
//             var items = new List<TreeViewItemData<string>>();
//             int addCount = 0;
//
//             bool search = !string.IsNullOrWhiteSpace(Window.TextFieldSearch.value);
//             string searchStr = Window.TextFieldSearch.value.ToLower();
//             
//             TreeViewItemData<string> CreateTreeViewItemData(int rootIndex, AssetBundleFileDepsInfo treeInfo)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//
//                 foreach (var keyValue in treeInfo.DepList)
//                 {
//                     addCount++;
//                     var fileStr = $"{keyValue}";
//                     // Debug.Log($"{addCount}, {fileStr}");
//                     treeViewSubItemsData.Add(new TreeViewItemData<string>(addCount, fileStr));
//                 }
//
//                 var content = $"{treeInfo.File} : {treeInfo.DepList.Count}";
//                 // Debug.Log($"{rootIndex}, {content}");
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, content, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//
//             foreach (var value in sortArr)
//             {
//                 if (search && !value.File.ToLower().Contains(searchStr)) continue;
//                 
//                 items.Add(CreateTreeViewItemData(addCount, value));
//                 addCount++;
//             }
//
//             TreeView.SetRootItems(items);
//             TreeView.selectionType = SelectionType.Multiple;
//             TreeView.Rebuild();
//         }
//         
//         public void UpdateShaderVariantCollection(SVCInfo svcInfo)
//         {
//             var items = new List<TreeViewItemData<string>>();
//             int addCount = 0;
//
//             bool search = !string.IsNullOrWhiteSpace(Window.TextFieldSearch.value);
//             string searchStr = Window.TextFieldSearch.value.ToLower();
//             
//             TreeViewItemData<string> CreateTreeViewItemData(int rootIndex, SVCShaderInfo info)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//
//                 var variants = info.ShaderVariantDict.Values.ToArray();
//                 foreach (var variant in variants)
//                 {
//                     addCount++;
//                     string fileStr = $"{variant.passType}  {SVCHelper.KeywordsToString(variant.keywords)}";
//                     treeViewSubItemsData.Add(new TreeViewItemData<string>(addCount, fileStr));
//                 }
//
//                 var content = $"{info.ShaderName}     (variants: {info.ShaderVariantDict.Count})";
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, content, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             
//             foreach (var value in svcInfo.ShaderDict.Values)
//             {
//                 if (search && !value.ShaderName.ToLower().Contains(searchStr)) continue;
//                 
//                 items.Add(CreateTreeViewItemData(addCount, value));
//                 addCount++;
//             }
//
//             TreeView.SetRootItems(items);
//             TreeView.selectionType = SelectionType.Multiple;
//             TreeView.Rebuild();
//         }
//         
//         public void Update(CompatibilityAssetBundleManifest manifest)
//         {
//             var items = new List<TreeViewItemData<string>>();
//             int addCount = 0;
//
//             bool search = !string.IsNullOrWhiteSpace(Window.TextFieldSearch.value);
//             string searchStr = Window.TextFieldSearch.value.ToLower();
//             
//             TreeViewItemData<string> CreateTreeViewItemData(int rootIndex, string abName)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//
//                 var deps = manifest.GetAllDependencies(abName);
//                 foreach (var dep in deps)
//                 {
//                     addCount++;
//                     string fileStr = $"dep: {dep}";
//                     treeViewSubItemsData.Add(new TreeViewItemData<string>(addCount, fileStr));
//                 }
//
//                 var content = $"{abName}     (deps: {deps.Length})";
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, content, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             if (manifest != null)
//             {
//                 var assetBundles = manifest.GetAllAssetBundles();
//                 foreach (var value in assetBundles)
//                 {
//                     if (search && !value.ToLower().Contains(searchStr)) continue;
//                 
//                     items.Add(CreateTreeViewItemData(addCount, value));
//                     addCount++;
//                 }
//
//             }
//
//             TreeView.SetRootItems(items);
//             TreeView.selectionType = SelectionType.Multiple;
//             TreeView.Rebuild();
//         }
//
//         protected override void BindItem(VisualElement ve, int index)
//         {
//             var item = TreeView.GetItemDataForIndex<string>(index);
//             (ve as Label).text = item;
//         }
//
//         protected override VisualElement MakeItem()
//         {
//             return new Label()
//             {
//                 style = { unityTextAlign = TextAnchor.MiddleLeft } 
//             };
//         }
//     }
// }
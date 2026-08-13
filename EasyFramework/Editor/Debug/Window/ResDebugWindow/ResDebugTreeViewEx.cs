// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Build.Pipeline;
// using UnityEngine.UIElements;
//
// namespace EasyFramework.Editor
// {
//     public class ResDebugTreeViewEx : UIToolkitTreeViewEx<ResDebugWindow>
//     {
//         protected override string ViewName => "TreeView";
//
//         public void Update(string[] dataBuilds)
//         {
//             var items = new List<TreeViewItemData<string>>();
//             int addCount = 0;
//
//             bool search = !string.IsNullOrWhiteSpace(Window.TextFieldSearch.value);
//             string searchStr = Window.TextFieldSearch.value.ToLower();
//             
//             TreeViewItemData<string> CreateTreeViewItemData(int rootIndex, string build)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//                 
//                 addCount++;
//                 treeViewSubItemsData.Add(new TreeViewItemData<string>(addCount, build));
//                 
//                 var content = $"{Path.GetFileName(build)}";
//                 // Debug.Log($"{rootIndex}, {content}");
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, content, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             foreach (var build in dataBuilds)
//             {
//                 if (search && !Path.GetFileName(build).ToLower().Contains(searchStr)) continue;
//
//                 items.Add(CreateTreeViewItemData(addCount, build));
//                 addCount++;
//             }
//
//             TreeView.SetRootItems(items);
//             TreeView.selectionType = SelectionType.Multiple;
//             TreeView.Rebuild();
//         }
//         public void Update(AssetBundleBuild[] assetBundleBuilds)
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
//         public void Update(AssetBundleBuildDepsDebug assetBundleBuildDepsDebug)
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
//                 var assetBundles = manifest.GetAllAssetBundles().OrderBy(item => item);
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
//         public void Update(SVCInfo svcInfo)
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
//         public void UpdatePackageList(DLCBuilderPackage[] dlcPackages)
//         {
//             var items = new List<TreeViewItemData<string>>();
//             int addCount = 0;
//         
//             bool search = !string.IsNullOrWhiteSpace(Window.TextFieldSearch.value);
//             string searchStr = Window.TextFieldSearch.value.ToLower();
//             
//             TreeViewItemData<string> CreateTreeViewItemDataInfo(int rootIndex, string rootContent, DLCResBuildInfo buildInfo)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//         
//                 if (buildInfo.ResFiles?.Length > 0)
//                 {
//                     for (int i = 0; i < buildInfo.ResFiles.Length; i++)
//                     {
//                         addCount++;
//                         var fileStr = $"{buildInfo.ResFiles[i]}";
//                         treeViewSubItemsData.Add(new TreeViewItemData<string>(addCount, fileStr));
//                     }
//                 }
//         
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, rootContent, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             TreeViewItemData<string> CreateTreeViewItemDataDict(int rootIndex, DLCBuilderPackage package)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//                 foreach (var dlcResBuildInfo in package.ResBuildList)
//                 {
//                     if (search && !dlcResBuildInfo.ResName.ToLower().Contains(searchStr)) continue;
//                         
//                     addCount++;
//                     var content = $"{dlcResBuildInfo.ResTag} : {Path.GetFileName(dlcResBuildInfo.BuildFile)}";
//                     treeViewSubItemsData.Add(CreateTreeViewItemDataInfo(addCount, content, dlcResBuildInfo));
//                 }
//                 
//                 var allCount = package.ResBuildList.Count;
//                 var rootContent = $"[{package.PackageName} : {allCount}]";
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, rootContent, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             if (dlcPackages?.Length > 0)
//             {
//                 foreach (var package in dlcPackages)
//                 {
//                     items.Add(CreateTreeViewItemDataDict(addCount, package));
//                     addCount++;
//                 }
//             }
//         
//             TreeView.SetRootItems(items);
//             TreeView.selectionType = SelectionType.Multiple;
//             TreeView.Rebuild();
//         }
//         
//         public void UpdatePackageZip(DLCBuilderPackage[] dlcPackages)
//         {
//             var items = new List<TreeViewItemData<string>>();
//             int addCount = 0;
//
//             bool search = !string.IsNullOrWhiteSpace(Window.TextFieldSearch.value);
//             string searchStr = Window.TextFieldSearch.value.ToLower();
//             
//             TreeViewItemData<string> CreateTreeViewItemDataInfo(int rootIndex, DLCResBuildInfo buildInfo)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//
//                 if (buildInfo.ResFiles?.Length > 0)
//                 {
//                     for (int i = 0; i < buildInfo.ResFiles.Length; i++)
//                     {
//                         addCount++;
//                         var fileStr = $"{buildInfo.ResFiles[i]}";
//                         treeViewSubItemsData.Add(new TreeViewItemData<string>(addCount, fileStr));
//                     }
//                 }
//
//                 var content = $"{Path.GetFileName(buildInfo.BuildFile)}";
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, content, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             TreeViewItemData<string> CreateTreeViewItemData(int rootIndex, string rootContent, List<DLCResBuildInfo> fileList)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//                 
//                 for (int i = 0; i < fileList.Count; i++)
//                 {
//                     var buildInfo = fileList[i];
//                     if (search && !buildInfo.ResName.ToLower().Contains(searchStr)) continue;
//                     
//                     addCount++;
//                     treeViewSubItemsData.Add(CreateTreeViewItemDataInfo(addCount, buildInfo));
//                 }
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, rootContent, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             TreeViewItemData<string> CreateTreeViewItemDataDict(int rootIndex, DLCBuilderPackage package)
//             {
//                 var treeViewSubItemsData = new List<TreeViewItemData<string>>();
//                 var arr = package.ZipDict.OrderBy(item => item.Key);
//                 foreach (var keyValue in arr)
//                 {
//                     // var num = keyValue.Value.Sum(item => item.Value.Count);
//                     addCount++;
//                     var content = $"[{keyValue.Key} : {keyValue.Value.Count}]";
//                     treeViewSubItemsData.Add(CreateTreeViewItemData(addCount, content, keyValue.Value));
//                 }
//                 var allCount = arr.Sum(item => item.Value.Count);
//                 var rootContent = $"[{package.PackageName} : {allCount}]";
//                 var treeViewItemData = new TreeViewItemData<string>(rootIndex, rootContent, treeViewSubItemsData);
//                 return treeViewItemData;
//             }
//             if (dlcPackages?.Length > 0)
//             {
//                 foreach (var package in dlcPackages)
//                 {
//                     items.Add(CreateTreeViewItemDataDict(addCount, package));
//                     addCount++;
//                 }
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
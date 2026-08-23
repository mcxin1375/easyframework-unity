// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
//
// namespace EasyFramework.Editor
// {
//     public partial class DLCBuilder
//     {
//
//         public DLCVersion LoadDLCVersion(string dlcVersion)
//         {
//             if (string.IsNullOrWhiteSpace(dlcVersion) || dlcVersion == "0") 
//                 dlcVersion = Version.version.ToString();
//             
//             var file = $"{ProjectDataPath}/{dlcVersion}/{nameof(DLCVersion)}.json";
//             return NewtonsoftHelper.LoadOrCreate<DLCVersion>(file);
//         }
//         
//         public string[] GetPackageFiles(string dlcVersion, string[] packages)
//         {
//             if (string.IsNullOrWhiteSpace(dlcVersion) || dlcVersion == "0") 
//                 dlcVersion = Version.version.ToString();
//             
//             var outputPath = $"{ProjectDataPath}/{dlcVersion}";
//             // var dlcListBuildPath = $"{outputPath}/{DLCResMode.DLCList}";
//             
//             var tmpList = new List<string>();
//             var packagesHash = packages.ToHashSet();
//             foreach (var packageName in packagesHash)
//             {
//                 string packageVersionFile = $"{outputPath}/{packageName}.json";
//                 if (!File.Exists(packageVersionFile)) continue;
//                 
//                 tmpList.Add(packageVersionFile);
//                 var packageVersion = NewtonsoftHelper.LoadOrCreate<DLCPackageVersion>(packageVersionFile);
//                 foreach (var resFileInfo in packageVersion.listArray)
//                 {
//                     // string file = $"{dlcListBuildPath}/{resFileInfo.name}";
//                     // tmpList.Add(file);
//                 }
//             }
//
//             return tmpList.ToArray();
//         }
//
//         public DLCVersion[] LoadAllVersions()
//         {
//             if (!Directory.Exists(ProjectDataPath)) return null;
//
//             List<DLCVersion> tmpList = new();
//             var directories = Directory.GetDirectories(ProjectDataPath);
//             foreach (var directory in directories)
//             {
//                 string file = $"{directory}/{nameof(DLCVersion)}.json";
//                 if (File.Exists(file))
//                 {
//                     var dlcVersion = NewtonsoftHelper.LoadOrCreate<DLCVersion>(file);
//                     tmpList.Add(dlcVersion);
//                 }
//             }
//
//             return tmpList.OrderByDescending(item => item.versionName).ToArray();
//         }
//         
//         public DLCBuilderPackage[] GetDLCBuilderPackages(bool buildFiles = true)
//         {
//             // var settings = DLCBuilderSettings.Instance;
//             // var basePackage = new DLCBuilderPackage(EDLCOptions.DLC.ToString());
//             // Dictionary<string, DLCBuilderPackage> packageDict = new()
//             // {
//             //     [basePackage.PackageName] = basePackage
//             // };
//             //
//             // foreach (var rootDirectory in settings.dlcRootDirectories)
//             // {
//             //     if (!Directory.Exists(rootDirectory)) continue;
//             //     
//             //     string[] directories = Directory.GetDirectories(rootDirectory);
//             //     foreach (var directory in directories)
//             //     {
//             //         string packageName = Path.GetFileName(directory);
//             //         if (!packageDict.TryGetValue(packageName, out var package))
//             //         {
//             //             package = new DLCBuilderPackage(packageName);
//             //             packageDict.Add(packageName, package);
//             //         }
//             //         package.AddDirectory(directory);
//             //     }
//             // }
//             //
//             // foreach (var extension in settings.Extensions)
//             // {
//             //     if (extension.Packages == null) continue;
//             //
//             //     foreach (var request in extension.Packages)
//             //     {
//             //         if (!packageDict.TryGetValue(request.PackageName, out var package))
//             //         {
//             //             package = new DLCBuilderPackage(request.PackageName);
//             //             packageDict.Add(request.PackageName, package);
//             //         }
//             //         foreach (var directory in request.BuildDirectories) package.AddDirectory(directory);
//             //     }
//             // }
//             //
//             // void AddFileToPackage(string resType, string buildFile, string[] sourceFiles)
//             // {
//             //     if (sourceFiles?.Length > 0)
//             //     {
//             //         var mainFile = sourceFiles[0];
//             //         foreach (var packageCreate in packageDict.Values)
//             //         {
//             //             if (packageCreate.ContainsFile(mainFile))
//             //             {
//             //                 packageCreate.Add(resType, buildFile, sourceFiles);
//             //                 return;
//             //             }
//             //         }
//             //     }
//             //     basePackage.Add(resType, buildFile, sourceFiles);
//             // }
//             //
//             // if (buildFiles)
//             // {
//             //     var assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();
//             //     var dataBuilds = DataBuilder.Instance.GetDataFiles();
//             //     var abBuildDict = assetBundleBuilds.ToDictionary(item => item.assetBundleName, item => item);
//             //     var dataBuildDict = dataBuilds.ToDictionary(Path.GetFileName, item => item);
//             //
//             //     var abFiles = AssetBundleBuilder.Instance.GetBuildFiles();
//             //     var dllFiles = HybridCLRBuilder.Instance.GetBuildFiles();
//             //     var dataFiles = DataBuilder.Instance.GetBuildFiles();
//             //     if (abFiles?.Length > 0)
//             //     {
//             //         foreach (var buildFile in abFiles)
//             //         {
//             //             string fileName = Path.GetFileName(buildFile);
//             //             string[] resFiles = null;
//             //             if (abBuildDict.TryGetValue(fileName, out var assetBundleBuild)) resFiles = assetBundleBuild.assetNames;
//             //             AddFileToPackage(nameof(AssetBundleBuilder), buildFile, resFiles);
//             //         }
//             //     }
//             //     if (dllFiles?.Length > 0)
//             //     {
//             //         foreach (var buildFile in dllFiles)
//             //             AddFileToPackage(nameof(HybridCLRBuilder), buildFile, null);
//             //     }
//             //     if (dataFiles?.Length > 0)
//             //     {
//             //         foreach (var buildFile in dataFiles)
//             //         {
//             //             string fileName = Path.GetFileName(buildFile);
//             //             string[] resFiles = null;
//             //             if (dataBuildDict.TryGetValue(fileName, out var dataBuild)) resFiles = new string[] { dataBuild };
//             //             AddFileToPackage(nameof(DataBuilder), buildFile, resFiles);
//             //         }
//             //     }
//             // }
//
//             // return packageDict.Values.ToArray();
//             return null;
//         }
//     }
// }
/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class DLCBuilderUtility
    {
        public static DLCBuilderVersion GetNewestBuilderVersion()
        {
            var file = $"{DLCBuilder.Instance.ProjectPlatformPath}/{DLCBuilderVersionList.FileName}";
            var versionList = ConfigHelper.LoadOrCreate<DLCBuilderVersionList>(file);
            return versionList.versions?.Length > 0 ? versionList.versions[0] : null;
        }
        public static string GetDLCVersionFilePath(string dlcVersion)
        {
            return $"{DLCBuilder.Instance.ProjectPlatformPath}/{dlcVersion}/{DLCVersion.FileName}";
        }
        public static string GetDLCVersionFilePath(string dlcVersion, Platform platform)
        {
            return $"{DLCBuilder.Instance.GetProjectDataPath(platform)}/{dlcVersion}/{DLCVersion.FileName}";
        }
        
        public static void BuildBySettings(string outputPath)
        {
            var toolEvents = DLCBuilder.Instance.ToolEvents;
            
            foreach (var extension in toolEvents) extension.OnExecuteBefore();
            
            var settings = DLCBuilderSettings.Instance;
            var versionName = GetBuildVersionNameId();
            var outputDir = $"{outputPath}/{versionName}";
            var sourceDirs = new string[]
            {
                AssetBundleBuilder.Instance.ProjectPlatformPath,
                
#if EF_HYBRIDCLR
                HybridCLRBuilder.Instance.ProjectPlatformPath
#endif
            };
            
            var dlcVersionFile =  $"{outputDir}/{DLCVersion.FileName}";
            DLCVersion dlcVersion = new();
            dlcVersion.versionIndex = EasyFrameworkSettings.Instance.dlcVersionIndex;
            dlcVersion.versionName = versionName;
            dlcVersion.versionUid = Guid.NewGuid().ToString();
            
            if ((settings.buildOptions & EDLCMode.DLC) > 0)
            {
                BuildDLC($"{outputDir}/{EDLCMode.DLC}", sourceDirs, out dlcVersion.dlcVersionInfoUid);
            }
            ConfigHelper.Save(dlcVersion, dlcVersionFile, true);
            
            var lastVersionFile =  $"{outputPath}/{DLCVersion.LatestFileName}";
            ConfigHelper.Save(dlcVersion, lastVersionFile, true);
            
            DLCBuilderVersion dlcBuilderVersion = new();
            dlcBuilderVersion.dlcVersion = dlcVersion;
            dlcBuilderVersion.SetToolVersion(DLCBuilder.Instance.Version);
            dlcBuilderVersion.abBuilderVersion = AssetBundleBuilder.Instance.Version;
            
#if EF_HYBRIDCLR
            dlcBuilderVersion.dllBuilderVersion = HybridCLRBuilder.Instance.Version;
#endif
            ConfigHelper.Save(dlcBuilderVersion, $"{outputDir}/{DLCBuilderVersion.FileName}", true);
            

            DLCBuilderVersionList.Refresh(outputPath, settings.maxCacheNum);
            
            foreach (var extension in toolEvents) extension.OnExecuteAfter();
        }
        
        public static void BuildDLC(string outputDir, string[] sourceDirs, out string uid)
        {
            // Debug.Log($"DLCBuilder - BuildDLCList");
            
            uid = Guid.NewGuid().ToString();
            FileHelper.CreateDirectory(outputDir);
            FileHelper.ClearDirectory(outputDir);

            HashSet<string> sourcesHashSet = new();
            if (sourceDirs?.Length > 0)
            {
                foreach (var sourceDir in sourceDirs)
                {
                    if (!Directory.Exists(sourceDir)) continue;
                    
                    var files = Directory.GetFiles(sourceDir);
                    foreach (var file in files) sourcesHashSet.Add(file);
                }
            }

            int index = 0;
            List<HashFileInfo> hashFileList = new();
            try
            {
                foreach (var resFile in sourcesHashSet)
                {
                    index++;
                    EditorUtility.DisplayProgressBar($"DLCBuilder - BuildModeList", $"({index}/{sourcesHashSet.Count}) {resFile}", index / (float)sourcesHashSet.Count);

                    var fi = new FileInfo(resFile);
                    var md5 = MD5Helper.MD5File(resFile);
                    var hashFileName = $"{md5}{Path.GetExtension(resFile)}";
                    hashFileList.Add(new HashFileInfo
                    {
                        resName = Path.GetFileName(resFile),
                        fileName = hashFileName,
                        length = fi.Length,
                    });

                    var toFile = $"{outputDir}/{hashFileName}";
                    File.Copy(resFile, toFile, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            DLCVersionInfo versionInfo = new();
            versionInfo.uid = uid;
            versionInfo.hashFiles = hashFileList.ToArray();
            
            ConfigHelper.Save(versionInfo, $"{outputDir}/{DLCVersionInfo.FileName}", true);
        }
        
        private static string GetBuildVersionNameId()
        {
            var settings = DLCBuilderSettings.Instance;
            switch (settings.buildNameType)
            {
                case EDLCBuildNameType.AppName:
                    if (!EasyFrameworkSettings.AppSettings.AppName.IsNullOrEmpty())
                        return EasyFrameworkSettings.AppSettings.AppName;
                    break;
            }
            return DLCBuilder.Instance.Version.buildIndex.ToString();
        }
        
        // private void BuildDLC(string outputDir, DLCResMode mode)
        // {
        //     var dirName = Path.GetFileName(outputDir);
        //     if (Directory.Exists(outputDir)) FileHelper.ClearDirectory(outputDir);
        //
        //     bool buildList = mode == DLCResMode.DLCList || mode == DLCResMode.All;
        //     bool buildZip = mode == DLCResMode.DLCZip || mode == DLCResMode.All;
        //     
        //     var dlcListBuildPath = $"{outputDir}/{DLCResMode.DLCList}";
        //     var dlcZipBuildPath = $"{outputDir}/{DLCResMode.DLCZip}";
        //     
        //     List<string> dlcListFiles = new();
        //     List<string> zipFileNameList = new();
        //     List<DLCPackageVersion> packageList = new();
        //     
        //     var packageBuildRequests = GetDLCBuilderPackages();
        //     foreach (var dlcPackageCreate in packageBuildRequests)
        //     {
        //         DLCPackageVersion dlcPackageVersion = new DLCPackageVersion();
        //         dlcPackageVersion.dlcVersion = DLCBuilder.Instance.Version.version;
        //         dlcPackageVersion.packageName = dlcPackageCreate.PackageName;
        //         dlcPackageVersion.packageFileName = $"{dlcPackageCreate.PackageName}.json";
        //         string packageVersionFile = $"{outputDir}/{dlcPackageVersion.packageFileName}";
        //
        //         string[] listFiles = dlcPackageCreate.ResBuildList.Select(item => item.BuildFile).ToArray();
        //         dlcListFiles.AddRange(listFiles);
        //         if (buildList)
        //         {
        //             FileHelper.CopyFiles(listFiles, dlcListBuildPath, true, false, (s, p, len) =>
        //             {
        //                 EditorUtility.DisplayProgressBar($"{dlcPackageCreate.PackageName} - ResList", $"({p}/{len}) {s}", p / (float)len);
        //             });
        //         }
        //         dlcPackageVersion.listArray = ResFileHelper.CreateResFileInfos(listFiles, (p) =>
        //         {
        //             var index = (int)(p * listFiles.Length);
        //             var file = index >= listFiles.Length || index < 0 ? null : listFiles[index];
        //             EditorUtility.DisplayProgressBar($"{dlcPackageCreate.PackageName} - ResList Md5", $"({index}/{listFiles.Length}) {file}", p);
        //         });
        //         dlcPackageVersion.listResSize = dlcPackageVersion.listArray.Sum(item => item.length);
        //
        //         if (buildZip)
        //         {
        //             int zipProgress = 0;
        //             List<string> zipFileList = new();
        //             foreach (var keyValue in dlcPackageCreate.ZipDict)
        //             {
        //                 if (keyValue.Value.Count == 0) continue;
        //
        //                 string pathToMd5 = MD5Helper.MD5String(keyValue.Key);
        //                 string zipName = $"{pathToMd5}.zip";
        //                 string[] files = keyValue.Value.Select(item => item.BuildFile).ToArray();
        //                 string zipFile = $"{dlcZipBuildPath}/{zipName}";
        //                 zipFileList.Add(zipFile);
        //                 zipFileNameList.Add(zipName);
        //
        //                 zipProgress++;
        //                 ZipHelper.ZipFiles(files, zipFile, (s, p, len) =>
        //                 {
        //                     EditorUtility.DisplayProgressBar($"{dlcPackageCreate.PackageName} - ResZip [{zipProgress}/{dlcPackageCreate.ZipDict.Count}]",
        //                         $"({p}/{len}) {s}", p / (float)len);
        //                 });
        //             }
        //
        //             dlcPackageVersion.zipArray = ResFileHelper.CreateResFileInfos(zipFileList.ToArray(), (p) =>
        //             {
        //                 var index = (int)(p * listFiles.Length);
        //                 var file = index >= listFiles.Length || index < 0 ? null : listFiles[index];
        //                 EditorUtility.DisplayProgressBar($"{dlcPackageCreate.PackageName} - ResZip Md5", $"({index}/{listFiles.Length}) {file}", p);
        //             });
        //             dlcPackageVersion.zipResSize = dlcPackageVersion.zipArray.Sum(item => item.length);
        //         }
        //
        //         NewtonsoftHelper.Save(packageVersionFile, dlcPackageVersion);
        //         packageList.Add(dlcPackageVersion);
        //     }
        //     EditorUtility.ClearProgressBar();
        //     
        //     FileHelper.DeleteNotExistsRelativeFiles(dlcListBuildPath, dlcListFiles.Select(Path.GetFileName).ToArray());
        //     FileHelper.DeleteNotExistsRelativeFiles(dlcZipBuildPath, zipFileNameList.ToArray());
        //
        //     var packageFileNameHash = packageList.Select(item => item.packageFileName).ToHashSet();
        //     var packageVersionFiles = Directory.GetFiles(outputDir);
        //     foreach (var file in packageVersionFiles)
        //     {
        //         string fileName = Path.GetFileName(file);
        //         if (!packageFileNameHash.Contains(fileName)) FileHelper.DeleteFile(file);
        //     }
        //
        //     var forcedPackages = new HashSet<string> { EDLCOptions.DLC.ToString() };
        //     foreach (var packageName in EasyFrameworkAOTSettings.Instance.dlcBuiltinPackages) forcedPackages.Add(packageName);
        //
        //     var dlcVersion = new DLCVersion();
        //     dlcVersion.mainVersion = EasyFrameworkAOTSettings.App.MainVersion;
        //     dlcVersion.uid = Guid.NewGuid().ToString();
        //     dlcVersion.name = dirName;
        //     dlcVersion.packages = packageBuildRequests.Select(item => item.PackageName).ToArray();
        //     dlcVersion.forcedPackages = forcedPackages.ToArray();
        //     // dlcVersion.assetBuilderVer = Version.svnRevision;
        //     // dlcVersion.assetBuilderResStr = $"{FE.AssetBundleBuilder.Version.svnRevision}.{FE.DllBuilder.Version.svnRevision}.{FE.DataBuilder.Version.svnRevision}";
        //     // dlcVersion.buildTime = Version.DateTimeStr;
        //     dlcVersion.resMode = mode;
        //     dlcVersion.dlcBuilderVersion = DLCBuilder.Instance.Version;
        //     dlcVersion.assetBundleBuilderVersion = AssetBundleBuilder.Instance.Version;
        //     dlcVersion.dllBuilderVersion = HybridCLRBuilder.Instance.Version;
        //     dlcVersion.dataBuilderVersion = DataBuilder.Instance.Version;
        //     
        //     NewtonsoftHelper.Save($"{outputDir}/{nameof(DLCVersion)}.json", dlcVersion);
        //
        //     foreach (var package in packageList)
        //     {
        //         Debug.Log($"[{package.packageName}] list: {package.listArray.Length}, {FormatHelper.FormatByte(package.listResSize)} zip: {package.zipArray.Length}, {FormatHelper.FormatByte(package.zipResSize)}");
        //     }
        //     
        //     Debug.Log($"Build DLCVersion: {dlcVersion.Version}, svnRevision: {dlcVersion.SvnRevision}");
        // }
    }
}

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
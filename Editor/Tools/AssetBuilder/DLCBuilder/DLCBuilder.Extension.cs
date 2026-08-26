/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace EasyFramework.Editor
{
    public partial class DLCBuilder
    {
        public void BuildBySettings()
        {
            foreach (var extension in ToolEvents) extension.OnExecuteBefore();
            
            var settings = DLCBuilderSettings.Instance;
            var versionName = string.IsNullOrEmpty(settings.versionCustomId) ? Version.buildIndex.ToString() : settings.versionCustomId;
            var outputDir = $"{ProjectDataPath}/{versionName}";
            var sourceDirs = new string[]
            {
                AssetBundleBuilder.Instance.ProjectDataPath,
                
#if EF_HYBRIDCLR
                HybridCLRBuilder.Instance.ProjectDataPath
#endif
            };
            
            if ((settings.buildOptions & EDLCMode.List) > 0)
            {
                BuildDLC($"{outputDir}/{EDLCMode.List}", sourceDirs);
            }
            
            DLCVersion dlcVersion = new();
            dlcVersion.versionIndex = EasyFrameworkSettings.Instance.dlcVersionIndex;
            dlcVersion.versionName = versionName;
            dlcVersion.versionUid = Guid.NewGuid().ToString();
            ConfigHelper.Save(dlcVersion, $"{outputDir}/{DLCVersion.FileName}", true);
            
            DLCBuilderVersion dlcBuilderVersion = new();
            dlcBuilderVersion.dlcVersion = dlcVersion;
            dlcBuilderVersion.SetToolVersion(Version);
            dlcBuilderVersion.abBuilderVersion = AssetBundleBuilder.Instance.Version;
            
#if EF_HYBRIDCLR
            dlcBuilderVersion.dllBuilderVersion = HybridCLRBuilder.Instance.Version;
#endif
            ConfigHelper.Save(dlcBuilderVersion, $"{outputDir}/{DLCBuilderVersion.FileName}", true);

            DLCBuilderVersionList.Refresh(ProjectDataPath, settings.maxCacheNum);
            
            foreach (var extension in ToolEvents) extension.OnExecuteAfter();
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
        
        private void BuildDLC(string outputDir, string[] sourceDirs)
        {
            // Debug.Log($"DLCBuilder - BuildDLCList");
            
            FileHelper.CreateDirectory(outputDir);
            FileHelper.ClearDirectory(outputDir);

            List<string> fileList = new();
            if (sourceDirs?.Length > 0)
            {
                foreach (var sourceDir in sourceDirs)
                {
                    if (!Directory.Exists(sourceDir)) continue;
                    
                    var files = Directory.GetFiles(sourceDir);
                    if (files.Length > 0) fileList.AddRange(files);
                }
            }

            int index = 0;
            List<HashFileInfo> hashFileList = new();
            try
            {
                foreach (var resFile in fileList)
                {
                    index++;
                    EditorUtility.DisplayProgressBar($"DLCBuilder - BuildDLCList", $"({index}/{fileList.Count}) {resFile}", index / (float)fileList.Count);

                    var fi = new FileInfo(resFile);
                    var md5 = MD5Helper.MD5File(resFile);
                    var hashFileName = $"{md5}{Path.GetExtension(resFile)}";
                    hashFileList.Add(new HashFileInfo
                    {
                        fileName = Path.GetFileName(resFile),
                        hashFileName = hashFileName,
                        length = fi.Length,
                    });

                    var toFile = $"{outputDir}/{hashFileName}";
                    File.Copy(resFile, toFile);
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
            versionInfo.hashFiles = hashFileList.ToArray();
            
            ConfigHelper.Save(versionInfo, $"{outputDir}/{DLCVersionInfo.FileName}", true);
        }

        public DLCBuilderVersion GetNewBuilderVersion()
        {
            var file = $"{ProjectDataPath}/{DLCBuilderVersionList.FileName}";
            var versionList = ConfigHelper.LoadOrCreate<DLCBuilderVersionList>(file);
            return versionList.versions?.Length > 0 ? versionList.versions[0] : null;
        }
        public string GetDLCVersionFile(string dlcVersion)
        {
            return $"{ProjectDataPath}/{dlcVersion}/{DLCVersion.FileName}";
        }
        public string GetDLCVersionFile(string dlcVersion, Platform platform)
        {
            return $"{GetProjectDataPath(platform)}/{dlcVersion}/{DLCVersion.FileName}";
        }

    }
}
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

        public static DLCVersion GetLatestVersion()
        {
            var file = $"{DLCBuilder.Instance.ProjectPlatformPath}/{DLCVersion.LatestFileName}";
            return ConfigHelper.LoadOrCreate<DLCVersion>(file);
        }

        public static void BuildBySettings(string outputPath)
        {
            var settings = DLCBuilderSettings.Instance;
            var versionName = GetBuildVersionNameId();
            var versionPath = GetVersionPath(outputPath, versionName);
            var latestPath = Path.Combine(outputPath, DLCVersion.LatestFileName);
            var listPath = Path.Combine(outputPath, DLCBuilderVersionList.FileName);
            var outputDir = FileHelper.CreateTempPath(versionPath);
            var lastVersionFile = FileHelper.CreateTempPath(latestPath);
            var listFile = FileHelper.CreateTempPath(listPath);
            var outputs = new List<(string tempPath, string targetPath)>
            {
                (outputDir, versionPath), (lastVersionFile, latestPath), (listFile, listPath)
            };
            try
            {
                Directory.CreateDirectory(outputDir);
                var sourceDirs = new List<string>
                {
                    AssetBundleBuilder.Instance.ProjectPlatformPath,
                };
#if EF_HYBRIDCLR
                if (HybridCLR.Editor.SettingsUtil.Enable) sourceDirs.Add(HybridCLRBuilder.Instance.ProjectPlatformPath);
#endif

                var dlcVersionFile =  $"{outputDir}/{DLCVersion.FileName}";
                DLCVersion dlcVersion = new();
                dlcVersion.versionIndex = EasyFrameworkSettings.Instance.dlcVersionIndex;
                dlcVersion.versionName = versionName;
                dlcVersion.versionUid = Guid.NewGuid().ToString();

                if ((settings.buildMode & EDLCModeOptions.DLC) > 0)
                {
                    BuildDLCContents($"{outputDir}/{EDLCModeOptions.DLC}", sourceDirs.ToArray(), out dlcVersion.dlcVersionInfoUid);
                }
                else throw new InvalidOperationException("A DLC build must include the base DLC mode.");
                ConfigHelper.Save(dlcVersion, dlcVersionFile, true);

                ConfigHelper.Save(dlcVersion, lastVersionFile, true);

                DLCBuilderVersion dlcBuilderVersion = new();
                dlcBuilderVersion.dlcVersion = dlcVersion;
                dlcBuilderVersion.SetToolVersion(DLCBuilder.Instance.Version);
                dlcBuilderVersion.abBuilderVersion = AssetBundleBuilder.Instance.Version;

#if EF_HYBRIDCLR
                dlcBuilderVersion.dllBuilderVersion = HybridCLRBuilder.Instance.Version;
#endif
                ConfigHelper.Save(dlcBuilderVersion, $"{outputDir}/{DLCBuilderVersion.FileName}", true);


                ValidateVersion(outputDir);
                var versionList = CreateVersionList(outputPath, dlcBuilderVersion, settings.maxCacheNum,
                    settings.copyVersionToStreamingAssets ? settings.releaseVersion : null, out var obsoleteDirectories);
                ConfigHelper.Save(versionList, listFile, true);
                if (settings.copyVersionToStreamingAssets)
                {
                    var selectedPath = string.IsNullOrWhiteSpace(settings.releaseVersion) || settings.releaseVersion == versionName
                        ? outputDir : GetVersionPath(outputPath, settings.releaseVersion);
                    var target = EasyFrameworkSettings.Instance.StreamingAssetsDLCPath;
                    var stage = FileHelper.CreateTempPath(target);
                    outputs.Add((stage, target));
                    PrepareStreamingAssets(selectedPath, stage);
                }
                FileHelper.ReplacePathsFromTemp(outputs.ToArray(), obsoleteDirectories);
            }
            finally
            {
                foreach (var output in outputs) FileHelper.DeleteDirectoryOrFile(output.tempPath);
            }
        }

        public static void BuildDLC(string outputDir, string[] sourceDirs, out string uid)
        {
            var stage = FileHelper.CreateTempPath(outputDir);
            try
            {
                BuildDLCContents(stage, sourceDirs, out uid);
                FileHelper.ReplacePathsFromTemp(new[] { (stage, outputDir) });
            }
            finally { FileHelper.DeleteDirectoryOrFile(stage); }
        }

        private static void BuildDLCContents(string outputDir, string[] sourceDirs, out string uid)
        {
            uid = Guid.NewGuid().ToString();
            FileHelper.CreateDirectory(outputDir);

            HashSet<string> sourcesHashSet = new();
            if (sourceDirs == null || sourceDirs.Length == 0)
                throw new InvalidOperationException("DLC source directories are required.");
            foreach (var sourceDir in sourceDirs)
            {
                if (!Directory.Exists(sourceDir)) throw new DirectoryNotFoundException($"DLC source does not exist: {sourceDir}");
                foreach (var file in Directory.GetFiles(sourceDir)) sourcesHashSet.Add(file);
            }

            int index = 0;
            Dictionary<string, HashFileInfo> hashFiles = new(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var resFile in sourcesHashSet)
                {
                    index++;
                    EditorUtility.DisplayProgressBar($"DLCBuilder - BuildModeList", $"({index}/{sourcesHashSet.Count}) {resFile}", index / (float)sourcesHashSet.Count);

                    var fi = new FileInfo(resFile);
                    var md5 = MD5Helper.MD5File(resFile);
                    var hashFileName = $"{md5}{Path.GetExtension(resFile)}";
                    var name = Path.GetFileName(resFile);
                    var info = new HashFileInfo
                    {
                        resName = name,
                        fileName = hashFileName,
                        length = fi.Length,
                    };
                    if (hashFiles.TryGetValue(name, out var previous))
                    {
                        if (previous.fileName == hashFileName && previous.length == info.length) continue;
                        throw new InvalidDataException($"Conflicting DLC resource name: {name}");
                    }

                    var toFile = $"{outputDir}/{hashFileName}";
                    File.Copy(resFile, toFile, true);
                    if (!FileHelper.VerifyFileIntegrity(toFile, info.length, md5))
                        throw new InvalidDataException($"DLC copy verification failed: {resFile}");
                    hashFiles.Add(name, info);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            DLCVersionInfo versionInfo = new();
            versionInfo.uid = uid;
            versionInfo.hashFiles = hashFiles.Values.ToArray();

            ConfigHelper.Save(versionInfo, $"{outputDir}/{DLCVersionInfo.FileName}", true);
        }

        public static void CopyVersionToStreamingAssets(string dlcVersion)
        {
            dlcVersion = string.IsNullOrWhiteSpace(dlcVersion) ? DLCBuilder.Instance.LatestVersion?.versionName : dlcVersion;
            var source = GetVersionPath(DLCBuilder.Instance.ProjectPlatformPath, dlcVersion);
            var target = EasyFrameworkSettings.Instance.StreamingAssetsDLCPath;
            var stage = FileHelper.CreateTempPath(target);
            try
            {
                PrepareStreamingAssets(source, stage);
                FileHelper.ReplacePathsFromTemp(new[] { (stage, target) });
            }
            finally { FileHelper.DeleteDirectoryOrFile(stage); }
        }

        private static string GetVersionPath(string root, string versionName)
        {
            ValidateFileName(versionName);
            return Path.Combine(root, versionName);
        }

        private static void ValidateFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith(".") ||
                name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains('/') || name.Contains('\\'))
                throw new InvalidDataException($"Invalid DLC file/version name: {name}");
        }

        private static void ValidateVersion(string versionPath)
        {
            var version = ConfigHelper.Load<DLCVersion>($"{versionPath}/{DLCVersion.FileName}");
            var builder = ConfigHelper.Load<DLCBuilderVersion>($"{versionPath}/{DLCBuilderVersion.FileName}");
            if (version == null || string.IsNullOrWhiteSpace(version.versionUid) ||
                builder?.dlcVersion?.versionUid != version.versionUid)
                throw new InvalidDataException($"Invalid DLC version metadata: {versionPath}");
            ValidateFileName(version.versionName);
            ValidateManifest($"{versionPath}/{EDLCModeOptions.DLC}", version.dlcVersionInfoUid);
        }

        private static void ValidateManifest(string path, string expectedUid)
        {
            var info = ConfigHelper.Load<DLCVersionInfo>($"{path}/{DLCVersionInfo.FileName}");
            if (info?.hashFiles == null || string.IsNullOrWhiteSpace(expectedUid) || info.uid != expectedUid)
                throw new InvalidDataException($"Invalid DLC manifest: {path}");
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in info.hashFiles)
            {
                if (file == null) throw new InvalidDataException($"Null DLC manifest entry: {path}");
                ValidateFileName(file.resName);
                ValidateFileName(file.fileName);
                var md5 = FileHelper.ExtractMD5FromFileName(file.fileName);
                if (!names.Add(file.resName) || file.length < 0 || md5 == null ||
                    !FileHelper.VerifyFileIntegrity(Path.Combine(path, file.fileName), file.length, md5))
                    throw new InvalidDataException($"Invalid DLC resource: {path}/{file.fileName}");
            }
        }

        private static void PrepareStreamingAssets(string source, string stage)
        {
            ValidateVersion(source);
            Directory.CreateDirectory(stage);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var name = Path.GetFileName(file);
                if (!names.Add(name)) throw new InvalidDataException($"Duplicate StreamingAssets filename: {name}");
                var copied = Path.Combine(stage, name);
                var length = new FileInfo(file).Length;
                var md5 = MD5Helper.MD5File(file);
                File.Copy(file, copied);
                if (!FileHelper.VerifyFileIntegrity(copied, length, md5))
                    throw new InvalidDataException($"StreamingAssets copy verification failed: {file}");
            }
            var version = ConfigHelper.Load<DLCVersion>($"{stage}/{DLCVersion.FileName}");
            ValidateManifest(stage, version.dlcVersionInfoUid);
        }

        private static DLCBuilderVersionList CreateVersionList(string root, DLCBuilderVersion current,
            int maxCount, string releaseVersion, out string[] obsoleteDirectories)
        {
            var versions = new List<DLCBuilderVersion>();
            foreach (var directory in Directory.GetDirectories(root))
            {
                var name = Path.GetFileName(directory);
                if (name.StartsWith(".") || name == current.dlcVersion.versionName) continue;
                var file = $"{directory}/{DLCBuilderVersion.FileName}";
                if (!File.Exists(file)) continue;
                var version = ConfigHelper.Load<DLCBuilderVersion>(file);
                if (version?.dlcVersion?.versionName != name)
                    throw new InvalidDataException($"Invalid historical DLC version: {directory}");
                versions.Add(version);
            }
            versions = versions.OrderByDescending(item => item.dateTime).ToList();
            versions.Insert(0, current);
            var keep = maxCount > 0 ? versions.Take(maxCount).ToList() : versions.ToList();
            var obsolete = new List<string>();
            foreach (var version in versions)
            {
                if (keep.Contains(version)) continue;
                if (version.dlcVersion.versionName == releaseVersion) keep.Add(version);
                else obsolete.Add(GetVersionPath(root, version.dlcVersion.versionName));
            }
            obsoleteDirectories = obsolete.ToArray();
            return new DLCBuilderVersionList { versions = keep.ToArray() };
        }

        private static string GetBuildVersionNameId()
        {
            var settings = DLCBuilderSettings.Instance;
            switch (settings.buildName)
            {
                case EDLCNameOptions.AppName:
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

/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2020/7/3
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;
using UnityEngine.Build.Pipeline;

#if UNITY_2018_3_OR_NEWER
using BuildCompression = UnityEngine.BuildCompression;
#else
using BuildCompression = UnityEditor.Build.Content.BuildCompression;
#endif

namespace EasyFramework.Editor
{
    public static class AssetBundleBuilderUtility
    {
        private const string BuildLayoutReportFileName = "AssetBundleBuildLayout.json";
        private const string AnalysisReportFileName = "AssetBundleBuildLayout.Analysis.json";

        public static void BuildBySettings(string outputPath)
        {
            var settings = AssetBundleBuilderSettings.Instance;
            if ((settings.buildAssetBundleOptions & BuildAssetBundleOptions.DryRunBuild) != 0)
                throw new BuildFailedException("DryRunBuild cannot publish AssetBundles. Use BuildManifestOnly instead.");

            var stage = FileHelper.CreateTempPath(outputPath);
            CompatibilityAssetBundleManifest bundleManifest = null;
            AssetBundleBuildLayoutReport buildLayoutReport = null;
            try
            {
                if (settings.createManifestFile) BuildManifestOnly();
                bundleManifest = BuildAssetBundle(stage, settings.buildAssetBundleOptions, out buildLayoutReport);
                var manifest = ConfigHelper.Load<AssetBundleManifest>($"{stage}/{AssetBundleManifest.FileName}");
                if (manifest?.abNames == null || !manifest.abNames.SequenceEqual(bundleManifest.GetAllAssetBundles()))
                    throw new BuildFailedException("AssetBundleManifest.json does not match the completed build.");
                // SBP names its diagnostic manifest after the output directory; keep the published name stable.
                File.Move(Path.Combine(stage, Path.GetFileName(stage) + ".manifest"),
                    Path.Combine(stage, new DirectoryInfo(outputPath).Name + ".manifest"));
                FileHelper.ReplacePathsFromTemp(new[] { (stage, outputPath) });
                try
                {
                    SaveBuildLayoutReport(buildLayoutReport, BuildLayoutReportFileName);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"AssetBundleBuilder: 无法保存正式构建布局报告：{exception.Message}");
                }
            }
            finally
            {
                if (bundleManifest != null) UnityEngine.Object.DestroyImmediate(bundleManifest);
                FileHelper.DeleteDirectoryOrFile(stage);
            }
        }
        
        private static CompatibilityAssetBundleManifest BuildAssetBundle(string outputPath,
            BuildAssetBundleOptions buildOptions)
        {
            return BuildAssetBundle(outputPath, buildOptions, out _);
        }

        private static CompatibilityAssetBundleManifest BuildAssetBundle(string outputPath,
            BuildAssetBundleOptions buildOptions, out AssetBundleBuildLayoutReport buildLayoutReport)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var assetBundleBuilds = CreateAssetBundleBuildBySettings();

            FileHelper.CreateDirectory(outputPath);

            var results = BuildAssetBundlesWithLayout(outputPath, assetBundleBuilds, buildOptions,
                buildTarget, false, false, out buildLayoutReport);
            var bundleManifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            bundleManifest.SetResults(results.BundleInfos);
            File.WriteAllText($"{outputPath}/{Path.GetFileName(outputPath)}.manifest", bundleManifest.ToString());
            if ((buildOptions & BuildAssetBundleOptions.DryRunBuild) != 0) return bundleManifest;

            try
            {
                var abManifest = new AssetBundleManifest();
                abManifest.abNames = bundleManifest.GetAllAssetBundles();
                var bundleNames = abManifest.abNames.ToHashSet();
                foreach (var build in assetBundleBuilds)
                    if (!bundleNames.Contains(build.assetBundleName))
                        throw new BuildFailedException($"Built manifest is missing AssetBundle: {build.assetBundleName}");

                foreach (var bundle in abManifest.abNames)
                {
                    var fileName = (buildOptions & BuildAssetBundleOptions.AppendHashToAssetBundleName) != 0
                        ? $"{bundle}_{bundleManifest.GetAssetBundleHash(bundle)}"
                        : bundle;
                    var bundleFile = Path.Combine(outputPath, fileName);
                    if (!File.Exists(bundleFile) || new FileInfo(bundleFile).Length == 0)
                        throw new BuildFailedException($"Built AssetBundle is missing or empty: {bundleFile}");
                    var deps = bundleManifest.GetAllDependencies(bundle);
                    if (deps.Length == 0) continue;
                    abManifest.depDict[bundle] = deps;
                }
                
                var manifestFile = $"{outputPath}/{AssetBundleManifest.FileName}";
                ConfigHelper.Save(abManifest, manifestFile, true);
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(bundleManifest);
                throw;
            }

            return bundleManifest;
        }

        public static AssetBundleBuildLayoutReport AnalyzeBySettings()
        {
            var settings = AssetBundleBuilderSettings.Instance;
            var builds = CreateAssetBundleBuildBySettings();
            EnsureNoDirtyProjectAssets();
            var outputPath = FileHelper.CreateTempPath(Path.Combine(
                AssetBundleBuilder.Instance.ToolsPlatformPath, "AssetBundleLayoutAnalysis"));

            try
            {
                FileHelper.CreateDirectory(outputPath);
                BuildAssetBundlesWithLayout(outputPath, builds, settings.buildAssetBundleOptions,
                    EditorUserBuildSettings.activeBuildTarget, true, true, out var report);
                SaveBuildLayoutReport(report, AnalysisReportFileName);
                return report;
            }
            finally
            {
                FileHelper.DeleteDirectoryOrFile(outputPath);
            }
        }

        public static AssetBundleBuildLayoutReport LoadLastBuildLayoutReport()
        {
            return LoadBuildLayoutReport(BuildLayoutReportFileName);
        }

        public static AssetBundleBuildLayoutReport LoadLastAnalysisReport()
        {
            return LoadBuildLayoutReport(AnalysisReportFileName);
        }

        private static IBundleBuildResults BuildAssetBundlesWithLayout(string outputPath,
            AssetBundleBuild[] builds, BuildAssetBundleOptions options, BuildTarget target,
            bool stopAfterLayout, bool isolateTemporaryOutput, out AssetBundleBuildLayoutReport report)
        {
            var parameters = new BundleBuildParameters(target, BuildPipeline.GetBuildTargetGroup(target), outputPath);
            if ((options & BuildAssetBundleOptions.ForceRebuildAssetBundle) != 0)
                parameters.UseCache = false;

            if ((options & BuildAssetBundleOptions.AppendHashToAssetBundleName) != 0)
                parameters.AppendHash = true;

            if ((options & BuildAssetBundleOptions.ChunkBasedCompression) != 0)
                parameters.BundleCompression = BuildCompression.LZ4;
            else if ((options & BuildAssetBundleOptions.UncompressedAssetBundle) != 0)
                parameters.BundleCompression = BuildCompression.Uncompressed;
            else
                parameters.BundleCompression = BuildCompression.LZMA;

            if ((options & BuildAssetBundleOptions.DisableWriteTypeTree) != 0)
                parameters.ContentBuildFlags |= ContentBuildFlags.DisableWriteTypeTree;

#if BUILD_OPTIONS_RECURSE_DEPENDENCIES_2022_3 || BUILD_OPTIONS_RECURSE_DEPENDENCIES_2023_3 || UNITY_6000_0_OR_NEWER
            if ((options & BuildAssetBundleOptions.RecurseDependencies) != 0)
                parameters.NonRecursiveDependencies = false;
#endif

            if (isolateTemporaryOutput)
            {
                parameters.TempOutputFolder = Path.Combine(outputPath, "Temp");
                parameters.ScriptOutputFolder = Path.Combine(outputPath, "ScriptAssemblies");
            }

            var captureTask = new AssetBundleBuildLayoutCapture(target.ToString(), options, stopAfterLayout);
            var tasks = DefaultBuildTasks.Create(DefaultBuildTasks.Preset.AssetBundleCompatible);
            var packingCallbackIndex = -1;
            for (var index = 0; index < tasks.Count; index++)
            {
                if (tasks[index] is PostPackingCallback)
                {
                    packingCallbackIndex = index;
                    break;
                }
            }

            if (packingCallbackIndex < 0)
                throw new InvalidOperationException("SBP AssetBundle task list does not contain PostPackingCallback.");

            var captureTaskIndex = packingCallbackIndex + 1;
            tasks.Insert(captureTaskIndex, captureTask);
            if (stopAfterLayout)
            {
                while (tasks.Count > captureTaskIndex + 1)
                    tasks.RemoveAt(tasks.Count - 1);
            }

            var content = new BundleBuildContent(builds);
            var exitCode = ContentPipeline.BuildAssetBundles(parameters, content, out IBundleBuildResults results, tasks);
            if (exitCode < ReturnCode.Success)
                throw new BuildFailedException($"AssetBundle build failed: {exitCode}");

            report = captureTask.Report;
            if (report == null)
                throw new BuildFailedException("AssetBundle layout capture did not produce a report.");

            return results;
        }

        private static void SaveBuildLayoutReport(AssetBundleBuildLayoutReport report, string fileName)
        {
            if (report?.bundles == null) return;

            var reportPath = Path.Combine(AssetBundleBuilder.Instance.DebugPlatformPath, fileName);
            var tempPath = FileHelper.CreateTempPath(reportPath);
            try
            {
                File.WriteAllText(tempPath, JsonUtility.ToJson(report, true));
                FileHelper.ReplaceFileFromTemp(tempPath, reportPath);
            }
            finally
            {
                FileHelper.DeleteFile(tempPath);
            }
        }

        private static AssetBundleBuildLayoutReport LoadBuildLayoutReport(string fileName)
        {
            var reportPath = Path.Combine(AssetBundleBuilder.Instance.DebugPlatformPath, fileName);
            if (!File.Exists(reportPath)) return null;

            try
            {
                return JsonUtility.FromJson<AssetBundleBuildLayoutReport>(File.ReadAllText(reportPath));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AssetBundleBuilder: 无法读取布局报告 {reportPath}: {exception.Message}");
                return null;
            }
        }

        private static void EnsureNoDirtyProjectAssets()
        {
            var dirtyPaths = GetDirtyProjectAssetPaths();
            if (dirtyPaths.Length == 0) return;

            throw new BuildFailedException(
                $"AssetBundle layout analysis stopped because this asset has unsaved changes: {dirtyPaths[0]}");
        }

        public static string[] GetDirtyProjectAssetPaths()
        {
            var dirtyPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var asset in Resources.FindObjectsOfTypeAll<UnityEngine.Object>())
            {
                if (asset == null || !EditorUtility.IsPersistent(asset) || !EditorUtility.IsDirty(asset)) continue;

                var assetPath = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(assetPath)) continue;
                if (IsProjectAssetPath(assetPath)) dirtyPaths.Add(assetPath);
            }

            return dirtyPaths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private static bool IsProjectAssetPath(string assetPath)
        {
            if (assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) return true;

            var packageInfo = assetPath.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase)
                ? UnityEditor.PackageManager.PackageInfo.FindForAssetPath(assetPath)
                : null;
            return packageInfo?.source == UnityEditor.PackageManager.PackageSource.Embedded;
        }
        
        public static void BuildManifestOnly()
        {
            var settings = AssetBundleBuilderSettings.Instance;
            var stage = FileHelper.CreateTempPath(AssetBundleBuilder.Instance.ProjectPlatformPath);
            var manifestFile = $"{AssetBundleBuilder.Instance.AssetsPlatformPath}/{PlatformHelper.PlatformName}.asset";
            var manifestTempFile = FileHelper.CreateTempPath(manifestFile);
            var manifestTempMeta = FileHelper.CreateTempPath(manifestFile + ".meta");
            var temporaryAsset = $"{AssetBundleBuilder.Instance.AssetsPlatformPath}/__Manifest_{System.Guid.NewGuid():N}.asset";
            CompatibilityAssetBundleManifest bundleManifest = null;
            try
            {
                bundleManifest = BuildAssetBundle(stage, settings.buildAssetBundleOptions | BuildAssetBundleOptions.DryRunBuild);
                FileHelper.CreateDirectory(AssetBundleBuilder.Instance.AssetsPlatformPath);
                AssetDatabase.CreateAsset(bundleManifest, temporaryAsset);
                AssetDatabase.SaveAssets();
                File.Copy(temporaryAsset, manifestTempFile);
                var replacements = new List<(string tempPath, string targetPath)> { (manifestTempFile, manifestFile) };
                if (!File.Exists(manifestFile + ".meta"))
                {
                    File.Copy(temporaryAsset + ".meta", manifestTempMeta);
                    replacements.Add((manifestTempMeta, manifestFile + ".meta"));
                }
                AssetDatabase.DeleteAsset(temporaryAsset);
                FileHelper.ReplacePathsFromTemp(replacements.ToArray());
                AssetDatabase.ImportAsset(manifestFile);
                FDebug.Log($"BuildManifestOnly: {manifestFile}");
            }
            finally
            {
                AssetDatabase.DeleteAsset(temporaryAsset);
                if (bundleManifest != null) UnityEngine.Object.DestroyImmediate(bundleManifest);
                FileHelper.DeleteDirectoryOrFile(stage);
                FileHelper.DeleteFile(manifestTempFile);
                FileHelper.DeleteFile(manifestTempMeta);
                AssetDatabase.Refresh();
            }
        }
        
        public static void CheckVerifyValidity()
        {
            var assetBundleBuilds = CreateAssetBundleBuildBySettings();

            Dictionary<GUID, AssetBundleBuild> tmpDict = new Dictionary<GUID, AssetBundleBuild>();
            foreach (AssetBundleBuild assetBundleBuild in assetBundleBuilds)
            {
                foreach (string assetPath in assetBundleBuild.assetNames)
                {
                    GUID asset = new GUID(AssetDatabase.AssetPathToGUID(assetPath));
                    if (tmpDict.ContainsKey(asset))
                    {
                        var preAb = tmpDict[asset];
                        Debug.LogError($"---------------------------- Multiple AssetBundleBuild File Repeated!");
                        Debug.LogError($"{preAb.assetBundleName} : {assetPath}");
                        Debug.LogError($"{assetBundleBuild.assetBundleName} : {assetPath}");
                        continue;
                    }
                    tmpDict.Add(asset, assetBundleBuild);
                }
            }
        }
        
        
        public static AssetBundleBuild[] CreateAssetBundleBuildBySettings()
        {
            ToolExtension<IAssetBundleBuilderSettings>.Refresh();
            var settings = AssetBundleBuilderSettings.Instance;
            
            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
            
            var directories = new HashSet<string>();
            foreach (var create in AssetBundleBuilder.Instance.ToolSettings)
            {
                if (create.BuildDirectories?.Length > 0)
                {
                    foreach (var directory in create.BuildDirectories)
                        directories.Add(directory);
                }
                if (create.BuildInfos?.Length > 0)
                {
                    foreach (var buildInfo in create.BuildInfos)
                    {
                        switch (buildInfo.abResType)
                        {
                            case EAssetBundleBuildResType.Shader:
                                buildList.Add(CreateShaderDirectory(buildInfo.abName, buildInfo.directories));
                                break;
                            default:
                                buildList.Add(CreateDirectory(buildInfo.abName, buildInfo.directories));
                                break;
                        }
                    }
                }
            }
            
            var arr = CreateAssetBundleBuildsByDirectories(directories.ToArray());
            if (arr?.Length > 0) buildList.AddRange(arr);

            Dictionary<string, AssetBundleBuild> dict = new Dictionary<string, AssetBundleBuild>();
            foreach (AssetBundleBuild ab in buildList)
            {
                if (dict.ContainsKey(ab.assetBundleName)) dict[ab.assetBundleName] = Merge(dict[ab.assetBundleName], ab);
                else dict.Add(ab.assetBundleName, ab);
            }
            
            return dict.Values.ToArray();
        }

        public static AssetBundleBuild[] CreateAssetBundleBuildsByDirectories(params string[] directories)
        {
            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
            List<string> folderList = new List<string>();
            List<string> fileList = new List<string>();
            
            var instance = AssetBundleBuilderSettings.Instance;
            var resFileIgnoreExs = instance.ignoreFileExes.ToHashSet();
            var resFileIgnoreNames = instance.ignoreFileNames.ToHashSet();
            
            void For(string path)
            {
                path = path.Replace("\\", "/");
                if (!Directory.Exists(path)) return;
                if (path.IsEditorPath()) return;
                
                string[] folders = Directory.GetDirectories(path).Where(file => !file.EndsWith(".meta")).ToArray();
                string[] files = Directory.GetFiles(path).Where(file => !file.EndsWith(".meta")).ToArray();

                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file);
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    
                    if (resFileIgnoreExs.Contains(extension) || resFileIgnoreNames.Contains(fileName)) continue;

                    fileList.Add(file.Replace("\\", "/"));
                }

                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    if (name == "Editor") continue;
                    if (name.EndsWith(EasyFrameworkSettings.Instance.abSuffix))
                    {
                        // folderList.Add(folder);
                        string newPath = folder.Replace('\\', '/');
                        if (!folderList.Contains(newPath)) folderList.Add(newPath);
                    }
                    else
                    {
                        For(folder);
                    }
                }
            }

            foreach (var directory in directories)
            {
                For(directory);
            }
            
            foreach (string folder in folderList)
            {
                buildList.Add(AssetBundleBuilderUtility.CreateDirectory(Path.GetFileNameWithoutExtension(folder), folder));
            }
            foreach (string file in fileList)
            {
                buildList.Add(AssetBundleBuilderUtility.CreateFile(Path.GetFileNameWithoutExtension(file), file));
            }

            return buildList.ToArray();
        }

        public static AssetBundleBuild CreateShaderDirectory(string abName, string directory) => CreateShaderDirectory(abName, new string[] { directory });
        public static AssetBundleBuild CreateShaderDirectory(string abName, string[] directories, string[] files = null)
        {
            List<string> tmpList = new List<string>();
            if (files?.Length > 0) tmpList.AddRange(files);
            string[] guids = AssetDatabase.FindAssets("t:Shader", directories);
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IsEditorPath()) continue;
                if (!path.Contains(".meta")) // 过滤掉元数据文件（如果需要）
                {
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    if (shader == null) continue;
                    tmpList.Add(path);
                }
            }
            return CreateFile(abName, tmpList.ToArray());
        }

        public static AssetBundleBuild CreateFile(string file) => CreateFile(Path.GetFileNameWithoutExtension(file), new string[] { file });
        public static AssetBundleBuild CreateFile(string abName, string file) => CreateFile(abName, new string[] { file });
        public static AssetBundleBuild CreateFile(string abName, string[] files) => CreateDirectory(abName, null, files);
        public static AssetBundleBuild CreateDirectory(string abName, string directory) => CreateDirectory(abName, new string[] { directory }, null);
        public static AssetBundleBuild CreateDirectory(string abName, string[] directories) => CreateDirectory(abName, directories, null);
        public static AssetBundleBuild CreateDirectory(string abName, string[] directories, string[] files)
        {
            List<string> assetNameList = new List<string>();
            void Add(string[] arr)
            {
                if (arr?.Length > 0)
                {
                    foreach (string s in arr) if (!assetNameList.Contains(s)) assetNameList.Add(s);
                }
            }

            Add(files);

            if (directories?.Length > 0)
            {
                foreach (string directory in directories)
                {
                    string[] arr = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Where(file => !file.EndsWith(".meta")).ToArray();
                    Add(arr);
                }
            }

            var assetNames = assetNameList.ToArray();
            var addressableNames = CreateAddressableNames(assetNames);
            return CreateAssetBundleBuild(abName, assetNames, addressableNames);
        }

        public static AssetBundleBuild CreateAssetBundleBuild(string abName, string[] assetNames, string[] addressableNames)
        {
            if (string.IsNullOrWhiteSpace(abName))
            {
                Debug.LogError($"CreateAssetBundleBuild abName is null or whitespace");
                return default;
            }

            if (!abName.EndsWith(EasyFrameworkSettings.Instance.abSuffix))
            {
                abName = $"{abName}{EasyFrameworkSettings.Instance.abSuffix}";
            }

            ValidateAddressableNames(abName, assetNames, addressableNames);

            return new AssetBundleBuild
            {
                assetBundleName = abName,
                assetBundleVariant = null,
                assetNames = assetNames,
                addressableNames = addressableNames
            };
        }

        public static AssetBundleBuild Merge(AssetBundleBuild ab, AssetBundleBuild ab2)
        {
            List<string> assetNameList = new List<string>();
            
            foreach (string s in ab.assetNames) if (!assetNameList.Contains(s)) assetNameList.Add(s);
            foreach (string s in ab2.assetNames) if (!assetNameList.Contains(s)) assetNameList.Add(s);

            ab.assetNames = assetNameList.ToArray();
            ab.addressableNames = CreateAddressableNames(ab.assetNames);
            ValidateAddressableNames(ab.assetBundleName, ab.assetNames, ab.addressableNames);
            return ab;
        }

        private static string[] CreateAddressableNames(string[] assetNames)
        {
            if (assetNames == null) return null;

            var normalizedPaths = assetNames.Select(NormalizeAssetPath).ToArray();
            var shortNames = normalizedPaths
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
            var duplicateShortNames = shortNames
                .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return shortNames
                .Select((shortName, index) => duplicateShortNames.Contains(shortName)
                    ? normalizedPaths[index]
                    : shortName)
                .ToArray();
        }

        private static string NormalizeAssetPath(string path)
        {
            return path?.Replace('\\', '/');
        }

        private static void ValidateAddressableNames(string abName, string[] assetNames, string[] addressableNames)
        {
            if (assetNames == null || addressableNames == null) return;
            if (assetNames.Length != addressableNames.Length)
                throw new BuildFailedException($"AssetBundle [{abName}] assetNames and addressableNames count mismatch.");

            for (int i = 0; i < addressableNames.Length; i++)
            {
                var addressableName = addressableNames[i];
                if (string.IsNullOrWhiteSpace(addressableName))
                    throw new BuildFailedException($"AssetBundle [{abName}] contains an empty addressable name.");
            }
        }
    }
}

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
using UnityEditor.Build.Pipeline;
using UnityEngine.Build.Pipeline;
using Debug = UnityEngine.Debug;

namespace EasyFramework.Editor
{
    internal class AssetBundleBuilderExtension : IAssetBundleBuildSettings
    {
        public string[] BuildDirectories { get; } = new string[]
        {
            // "Packages/com.cookie.easyframework/Res/DLC",
            EasyFrameworkPreferences.AssetsDataDLCPath
        };
    }

    public interface IAssetBundleBuilderExtension : IEditorToolExtension
    {
        void OnBuildAssetBundleBefore() { }
        void OnBuildAssetBundleAfter() { }
    }

    public class AssetBundleBuilder : EditorTool<AssetBundleBuilder, IAssetBundleBuilderExtension>
    {
        
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - Build", priority = EasyFrameworkToolsSettings.AssetBundleBuilder)]
        public static void MenuItem1() => AssetBundleBuilder.Instance.Build();
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - BuildManifestOnly", priority = EasyFrameworkToolsSettings.AssetBundleBuilder)]
        public static void MenuItem2() => AssetBundleBuilder.Instance.BuildManifestOnly();
        [MenuItem("EasyFramework/Tools/AssetBundleBuilder - CheckVerifyValidity", priority = EasyFrameworkToolsSettings.AssetBundleBuilder)]
        public static void MenuItem3() => AssetBundleBuilder.Instance.CheckVerifyValidity();

        public void Build()
        {
            foreach (var extension in Extensions) extension.OnBuildAssetBundleBefore();
            
            UpgradeVersion();
            
            var settings = AssetBundleBuilderSettings.Instance;
            if (settings.createManifestFile) BuildManifestOnly();
            var bundleManifest = BuildAssetBundle(ProjectDataPath, settings.buildAssetBundleOptions);
            if (bundleManifest == null) Debug.LogError("CompatibilityAssetBundleManifest is null!");

            foreach (var extension in Extensions) extension.OnBuildAssetBundleAfter();
        }
        
        public void BuildManifestOnly()
        {
            var settings = AssetBundleBuilderSettings.Instance;
            var bundleManifest = BuildAssetBundle(ProjectDataPath, settings.buildAssetBundleOptions | BuildAssetBundleOptions.DryRunBuild);
            if (bundleManifest != null)
            {
                FileHelper.CreateDirectory(AssetsDataPath);
                var manifestFile = $"{AssetsDataPath}/{EasyFrameworkConst.ManifestAssetFileName}";
                if (File.Exists(manifestFile)) AssetDatabase.DeleteAsset(manifestFile);
                AssetDatabase.CreateAsset(bundleManifest, manifestFile);
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private CompatibilityAssetBundleManifest BuildAssetBundle(string outputPath, BuildAssetBundleOptions buildOptions)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();

            FileHelper.CreateDirectory(outputPath);

            var bundleManifest = CompatibilityBuildPipeline.BuildAssetBundles(outputPath, assetBundleBuilds, buildOptions, buildTarget);
            if ((buildOptions & BuildAssetBundleOptions.DryRunBuild) != 0) return bundleManifest;

            if (bundleManifest != null)
            {
                var buildNameList = assetBundleBuilds.Select(item => item.assetBundleName).ToHashSet();
                string[] assetFiles = Directory.GetFiles(outputPath, $"*{EasyFrameworkConst.ABSuffix}", SearchOption.AllDirectories);

                // Debug.Log("------------------------ delete unused asset");
                foreach (string assetFile in assetFiles)
                {
                    string name = Path.GetFileName(assetFile);
                    if (!buildNameList.Contains(name))
                    {
                        File.Delete(assetFile);
                        Debug.Log($"[AB Delete] {name}");
                    }
                }

                var abManifest = new AssetBundleManifest();
                abManifest.AssetBundles = bundleManifest.GetAllAssetBundles();
                foreach (var bundle in abManifest.AssetBundles)
                {
                    var deps = bundleManifest.GetAllDependencies(bundle);
                    if (deps.Length == 0) continue;
                    abManifest.Dependencies.Add(bundle, deps);
                }
                
                var manifestFile = $"{outputPath}/{AssetBundleManifest.FileName}";
                NewtonsoftHelper.Save(manifestFile, abManifest, true);
            }

            return bundleManifest;
        }

        public void CheckVerifyValidity()
        {
            var assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();

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
        
        public string[] GetBuildFiles()
        {
            return Directory.Exists(ProjectDataPath) ? Directory.GetFiles(ProjectDataPath, "*", SearchOption.AllDirectories) : null;
        }
    }
}
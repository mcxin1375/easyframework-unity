/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Build.Pipeline;

namespace EasyFramework.Editor
{
    class AssetBundleBuilderExtension : IToolEvent<ToolBuilder>, IToolEvent<AssetBundleBuilder>
    {
        void IToolEvent<ToolBuilder>.OnExecute() => AssetBundleBuilder.Instance.Execute();
        void IToolEvent<AssetBundleBuilder>.OnExecute() => AssetBundleBuilder.Instance.BuildBySettings();
    }
    
    public partial class AssetBundleBuilder
    {
        public string[] GetBuildFiles()
        {
            return Directory.Exists(ProjectDataPath) ? Directory.GetFiles(ProjectDataPath, "*", SearchOption.AllDirectories) : null;
        }
        
        public void BuildBySettings()
        {
            var settings = AssetBundleBuilderSettings.Instance;
            if (settings.createManifestFile) BuildManifestOnly();
            var bundleManifest = BuildAssetBundle(ProjectDataPath, settings.buildAssetBundleOptions);
            if (bundleManifest == null) Debug.LogError("CompatibilityAssetBundleManifest is null!");
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
                abManifest.abNames = bundleManifest.GetAllAssetBundles();

                List<AssetBundleManifestItem> tmpList = new();
                foreach (var bundle in abManifest.abNames)
                {
                    var deps = bundleManifest.GetAllDependencies(bundle);
                    if (deps.Length == 0) continue;
                    tmpList.Add(new AssetBundleManifestItem()
                    {
                        abName = bundle,
                        deps = deps
                    });
                }

                abManifest.abItems = tmpList.ToArray();
                
                var manifestFile = $"{outputPath}/{AssetBundleManifest.FileName}";
                NewtonsoftHelper.Save(manifestFile, abManifest, true);
            }

            return bundleManifest;
        }
    }
}
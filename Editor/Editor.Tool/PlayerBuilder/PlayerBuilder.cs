/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EasyFramework.Editor
{
    public interface IPlayerBuilderExtension : IEditorToolExtension
    {
        void OnExecuteSettings();
        void OnBuildReport(BuildReport report);
    }

    public class PlayerBuilder : EditorTool<PlayerBuilder>
    {
        public IPlayerBuilderExtension[] Extensions => EditorToolExtension<IPlayerBuilderExtension>.Extensions;
        
        [MenuItem("EasyFramework/Tools/PlayerBuilder - BuildSettings", priority = EasyFrameworkToolsSettings.PlayerBuilder)]
        private static void MenuItem1() => PlayerBuilder.Instance.BuildSettings();
        
        // [MenuItem("EasyFramework/Tools/PlayerBuilder - BuildMainRes", priority = EasyFrameworkToolsSettings.PlayerBuilder + 1)]
        // public static void MenuItem2() => PlayerBuilder.Instance.BuildMainRes();
        //
        // [MenuItem("EasyFramework/Tools/PlayerBuilder - BuildPlayer", priority = EasyFrameworkToolsSettings.PlayerBuilder + 1)]
        // public static void MenuItem3() => PlayerBuilder.Instance.BuildPlayer();
        //
        // [MenuItem("EasyFramework/Tools/PlayerBuilder - BuildProject", priority = EasyFrameworkToolsSettings.PlayerBuilder + 1)]
        // public static void MenuItem4() => PlayerBuilder.Instance.BuildProject();

        public void BuildSettings()
        {
            EditorToolExtension<IPlayerBuilderExtension>.Refresh();
            foreach (var ex in Extensions) ex.OnExecuteSettings();
        }
        
        public BuildReport BuildPlayer(BuildPlayerOptions options)
        {
            var locationDirectory = Path.GetDirectoryName(options.locationPathName);
            FileHelper.CreateDirectory(locationDirectory);

            var result = BuildPipeline.BuildPlayer(options);
            
            var appPlayerVersion = new AppPlayerVersion();
            appPlayerVersion.name = Path.GetFileName(options.locationPathName);
            appPlayerVersion.mainVersion = EasyFrameworkAOTSettings.App.MainVersion;
            var appVersionFile = $"{locationDirectory}/{EasyFrameworkAOTSettings.App.AppName}.json";
            NewtonsoftHelper.Save(appVersionFile, appPlayerVersion);
            
            foreach (var ex in Extensions) ex.OnBuildReport(result);
            
            return result;
        }
        
        public void BuildMainRes(string dlcVersion = "")
        {
            // FileHelper.ClearDirectory(Application.streamingAssetsPath);
            //
            // List<string> packageList = new List<string>()
            // {
            //     EDLCOptions.DLC.ToString()
            // };
            //
            // if (EasyFrameworkAOTSettings.Instance.dlcBuiltinPackages?.Count > 0) packageList.AddRange(EasyFrameworkAOTSettings.Instance.dlcBuiltinPackages);
            // var files = EasyFrameworkAOTSettings.Instance.dlcEnabled
            //     ? DLCBuilder.Instance.GetPackageFiles(dlcVersion, packageList.ToArray())
            //     : GetBuildFiles();
            // Debug.Log($"MainRes Packages: {string.Join(",", packageList.ToArray())} Files: {files?.Length ?? 0}");
            // // return;
            //
            // List<string> zipResList = new();
            // if (files?.Length > 0)
            // {
            //     var zipArr = CreateMainResZipInfo(files);
            //     for (int i = 0; i < zipArr.Length; i++)
            //     {
            //         var mainResZipInfo = zipArr[i];
            //         var zipName = $"Res{i + 1}.zip";
            //         var zipFile = $"{Application.streamingAssetsPath}/{zipName}";
            //         ZipHelper.ZipFiles(mainResZipInfo.fileList.ToArray(), zipFile, (s, p, len) =>
            //         {
            //             EditorUtility.DisplayProgressBar($"Create MainRes [{i + 1}/{zipArr.Length}]", $"({p}/{len}) {s}", p / (float)len);
            //         });
            //         
            //         zipResList.Add(zipFile);
            //     }
            //     EditorUtility.ClearProgressBar();
            // }
            //
            // SVNCommand.TryGetRevision(EasyFrameworkPreferences.ProjectFullPath, out var svnRevision);
            //
            // var dlc = DLCBuilder.Instance.LoadDLCVersion(dlcVersion);
            // MainResInfo mainResVersion = EasyFrameworkAOTSettings.Instance.mainResInfo;
            // mainResVersion.mainResUid = Guid.NewGuid().ToString();
            // mainResVersion.additionalPackages = packageList.ToArray();
            // mainResVersion.mainResZipArray = ResFileHelper.CreateResFileInfos(zipResList.ToArray());
            // EasyFrameworkAOTSettings.Instance.SaveEx();
            //
            // AssetDatabase.Refresh();
        }
        
        [Serializable]
        private class MainResZipInfo
        {
            public long allSize;
            public List<string> fileList = new List<string>();
        }

        private static MainResZipInfo[] CreateMainResZipInfo(string[] files)
        {
            long zipMaxSize = FormatHelper.Mb * PlayerBuilderSettings.Instance.maxZipSizeMb;

            MainResZipInfo zipInfo = null;
            List<MainResZipInfo> tmpList = new();
            foreach (string file in files)
            {
                if (!File.Exists(file)) continue;

                if (zipInfo == null || zipInfo.allSize > zipMaxSize)
                {
                    zipInfo = new MainResZipInfo();
                    tmpList.Add(zipInfo);
                }

                FileInfo fileInfo = new FileInfo(file);
                zipInfo.allSize += fileInfo.Length;
                zipInfo.fileList.Add(file);
            }

            return tmpList.ToArray();
        }
        
    }
}
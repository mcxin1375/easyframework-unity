/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace EasyFramework.Editor
{
    public partial class PlayerBuilder
    {
        public void BuildBySettingsBefore()
        {
            var settings = PlayerBuilderSettings.Instance;
            if (!settings.preSettingsEnabled) return;
            
            var appSettings = EasyFrameworkSettings.AppSettings;
            
            if (!settings.companyName.IsNullOrEmpty())
                PlayerSettings.companyName = settings.companyName;
            if (!settings.productName.IsNullOrEmpty())
                PlayerSettings.productName = settings.productName;
            if (!appSettings.AppVersion.IsNullOrEmpty())
                PlayerSettings.bundleVersion = appSettings.AppVersion;
            if (!appSettings.BundleIdentifier.IsNullOrEmpty())
            {
                var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                PlayerSettings.SetApplicationIdentifier(namedTarget, appSettings.BundleIdentifier);
            }
            
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    var ver = appSettings.BundleVersion > 0
                        ? appSettings.BundleVersion
                        : PlayerBuilder.Instance.Version.buildIndex;
                    PlayerSettings.Android.bundleVersionCode = 0;
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = settings.exportAsGoogleAndroidProject;
                    break;
            }
            
            
            
        }
        
        public void BuildBySettings()
        {
            var settings = PlayerBuilderSettings.Instance;
            if (!settings.enabled) return;

            var buildSettings = ToolSettings?.FirstOrDefault();
            if (buildSettings == null)
            {
                FDebug.LogError("BuildPlayerOptionsSettings is null");
                return;
            }

            var options = buildSettings.BuildPlayerOptions;
            var locationDirectory = Path.GetDirectoryName(options.locationPathName);
            FileHelper.CreateDirectory(locationDirectory);

            var result = BuildPipeline.BuildPlayer(options);
            FDebug.Log($"BuildPlayer: {result.summary.result}");

            foreach (var ex in ToolExtensions) ex.OnBuildReport(result);
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
        //
        // [Serializable]
        // private class MainResZipInfo
        // {
        //     public long allSize;
        //     public List<string> fileList = new List<string>();
        // }
        //
        // private static MainResZipInfo[] CreateMainResZipInfo(string[] files)
        // {
        //     long zipMaxSize = FormatHelper.Mb * PlayerBuilderSettings.Instance.maxZipSizeMb;
        //
        //     MainResZipInfo zipInfo = null;
        //     List<MainResZipInfo> tmpList = new();
        //     foreach (string file in files)
        //     {
        //         if (!File.Exists(file)) continue;
        //
        //         if (zipInfo == null || zipInfo.allSize > zipMaxSize)
        //         {
        //             zipInfo = new MainResZipInfo();
        //             tmpList.Add(zipInfo);
        //         }
        //
        //         FileInfo fileInfo = new FileInfo(file);
        //         zipInfo.allSize += fileInfo.Length;
        //         zipInfo.fileList.Add(file);
        //     }
        //
        //     return tmpList.ToArray();
        // }
        
    }
}
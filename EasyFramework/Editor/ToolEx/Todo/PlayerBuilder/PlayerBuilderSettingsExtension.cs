// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using System.IO;
// using UnityEditor;
// using UnityEditor.Build;
// using UnityEditor.Build.Reporting;
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public class PlayerBuilderSettingsExtension : IPlayerBuilderExtension
//     {
//         public virtual void OnExecuteSettings()
//         {
//             var settings = PlayerBuilderSettings.Instance;
//
//             OnBuildStart();
//             if (settings.streamingAssetsEnabled) OnBuildStreamingAssets();
//             if (settings.buildPlayer) OnBuildPlayer(settings.developmentBuild);
//             if (settings.buildProject) OnBuildPlayer(settings.developmentBuild, true);
//         }
//
//         public void OnBuildReport(BuildReport report)
//         {
//             
//         }
//
//         protected virtual void OnBuildStart()
//         {
//             PlayerSettings.companyName = EasyFrameworkAOTSettings.App.CompanyName;
//             PlayerSettings.productName = EasyFrameworkAOTSettings.App.ProductName;
//             PlayerSettings.bundleVersion = EasyFrameworkAOTSettings.App.BundleVersion;
//             var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
//             PlayerSettings.SetApplicationIdentifier(namedTarget, EasyFrameworkAOTSettings.App.BundleIdentifier);
//         }
//         
//         protected virtual void OnBuildStreamingAssets()
//         {
//             var settings = PlayerBuilderSettings.Instance;
//             // if ((settings.streamingAssetsMode & EStreamingAssetsMode.AssetBundleBuilder) > 0)
//             // {
//             //     FDebug.Log("AssetBundleBuilder");
//             // }
//             // if ((settings.streamingAssetsMode & EStreamingAssetsMode.HybridCLRBuilder) > 0)
//             // {
//             //     FDebug.Log("HybridCLRBuilder");
//             // }
//         }
//         
//         protected virtual void OnBuildPlayer(bool developmentBuild, bool exportProject = false)
//         {
//             Debug.Log($"OnBuildPlayer: {developmentBuild}, {exportProject}");
//             switch (EditorUserBuildSettings.activeBuildTarget)
//             {
//                 case BuildTarget.Android:
//                     EditorUserBuildSettings.exportAsGoogleAndroidProject = exportProject;
//                     break;
//             }
//
//             var options = GetBuildPlayerOptions(developmentBuild, exportProject);
//             var result = PlayerBuilder.Instance.BuildPlayer(options);
//             Debug.Log($"BuildPlayer: {result.summary.result}");
//         }
//
//         protected virtual BuildPlayerOptions GetBuildPlayerOptions(bool developmentBuild, bool exportProject = false)
//         {
//             List<string> editorScenes = new List<string>();
//             foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
//                 if (scene.enabled) editorScenes.Add(scene.path);
//
//             string buildPlayerName = $"{EasyFrameworkAOTSettings.App?.AppName ?? "-"}_{EasyFrameworkAOTSettings.App?.MainVersion}";
//              
//             BuildPlayerOptions options = new BuildPlayerOptions();
//             options.scenes = editorScenes.ToArray();
//             options.target = EditorUserBuildSettings.activeBuildTarget;
//             options.options = BuildOptions.None;
//             
//             if (developmentBuild)
//             {
//                 options.options |= BuildOptions.Development;
//             }
//             else if ((options.options & BuildOptions.Development) == BuildOptions.Development)
//             {
//                 options.options -= BuildOptions.Development;
//             }
//             
//             
//             var basePath = PlayerBuilder.Instance.ProjectDataPath;
//             switch (EditorUserBuildSettings.activeBuildTarget)
//             {
//                 case BuildTarget.Android:
//                     options.locationPathName = $"{basePath}/{buildPlayerName}.apk";
//                     break;
//                 case BuildTarget.StandaloneWindows:
//                     options.locationPathName = $"{basePath}/{buildPlayerName}/{buildPlayerName}.exe";
//                     break;
//                 case BuildTarget.StandaloneWindows64:
//                     options.locationPathName = $"{basePath}/{buildPlayerName}/{buildPlayerName}64.exe";
//                     break;
//                 default:
//                     options.locationPathName = $"{basePath}/{buildPlayerName}";
//                     break;
//             }
//             if (exportProject)
//             {
//                 options.locationPathName = $"{PlayerBuilder.Instance.ProjectDataPath}/Project_{buildPlayerName}";
//             }
//             else
//             {
//             }
//
//             return options;
//         }
//     }
// }
// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using System.IO;
// using System.Text;
// using UnityEditor;
// using UnityEditor.Build.Reporting;
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public class DingTalkExtension : IDLCBuilderExtension, IPlayerBuilderBuildResult
//     {
//         public int Order => int.MaxValue;
//         void IDLCBuilderExtension.OnExecuteAfter()
//         {
//             if (!IsActive()) return;
//             // var settings = DingTalkExtensionSettings.Instance;
//             // if (!settings.dlcBuilder) return;
//             
//             var dlcBuilderVersion = DLCBuilder.Instance.Version;
//             StringBuilder sb = new();
//             if (!string.IsNullOrWhiteSpace(EasyFrameworkPreferences.ServerUrl))
//             {
//                 var link = $"{EasyFrameworkPreferences.ServerUrl}/DLCChangeLog?project={EasyFrameworkPreferences.ProjectName}";
//                 // var link = $"{EasyFrameworkEditorSettings.Instance.httpAddress}/DLCBuilder/{EasyFrameworkEditorHelper.GetPlatformName()}/{DLCBuilderUpdateLogExtension.LogFileName}";
//                 // Debug.Log(link);
//                 sb.AppendLine($"[{PlatformHelper.PlatformName} - 更新日志]({link})\n");
//             }
//
//             sb.AppendLine($"新增DLC版本: {dlcBuilderVersion.version}");
//             // sb.AppendLine(DLCBuilderUpdateLogExtension.GetLastedVersionUpdateLog());
//             
//             SendMessage($"新增DLC版本: {dlcBuilderVersion.version}", sb.ToString());
//         }
//
//         void IPlayerBuilderBuildResult.OnResult(bool exportProject, BuildPlayerOptions buildPlayerOptions, BuildReport buildReport)
//         {
//             if (!IsActive()) return;
//             // var settings = DingTalkExtensionSettings.Instance;
//             // if (!settings.playerBuilder) return;
//             
//             StringBuilder sb = new();
//             if (exportProject)
//                 sb.AppendLine($"**BuildProject:** {Path.GetFileName(buildPlayerOptions.locationPathName)}\n");
//             else
//                 sb.AppendLine($"**BuildPlayer:** {Path.GetFileName(buildPlayerOptions.locationPathName)}\n");
//             sb.AppendLine($"**BuildResult:** {buildReport.summary.result}");
//
//             SendMessage($"PlayerBuilder", sb.ToString());
//         }
//
//         public static void SendMessage(string title, string content)
//         {
//             var settings = DingTalkExtensionSettings.Instance;
//             if (settings.dingTalkConfigs?.Length > 0)
//             {
//                 foreach (var dingTalkConfig in settings.dingTalkConfigs)
//                 {
//                     if (string.IsNullOrWhiteSpace(dingTalkConfig.url) || string.IsNullOrWhiteSpace(dingTalkConfig.secret)) continue;
//                     _ = DingTalkHelper.SendMarkdownMessageAsync(dingTalkConfig.url, dingTalkConfig.secret, title, content);
//                 }
//             }
//         }
//
//         private bool IsActive()
//         {
//             var settings = DingTalkExtensionSettings.Instance;
//             if (Application.isBatchMode)
//             {
//                 return settings.batchModeEnabled;
//             }
//             else
//             {
//                 return settings.editorEnabled;
//             }
//         }
//     }
// }
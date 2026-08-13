// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using System.IO;
// using UnityEditor;
//
// namespace EasyFramework.Editor
// {
//     public class DLCReleaseBuilder : EditorTool<DLCReleaseBuilder>
//     {
//         [MenuItem("EasyFramework/Tools/DLCReleaseBuilder - ReleaseCurrent", priority = EasyFrameworkToolsSettings.DLCReleaseBuilder)]
//         public static void ReleaseCurrent()
//         {
//             var newVersion = DLCBuilder.Instance.GetNewBuilderVersion();
//             if (newVersion != null)
//             {
//                 Instance.Release(EasyFrameworkAOTSettings.App.AppName, newVersion.dlcVersion.versionName);
//             }
//         }
//
//         public void Release(string appName, string dlcVersion)
//         {
//             var dlcVersionFile = DLCBuilder.Instance.GetDLCVersionFile(dlcVersion);
//             var appVersionFile = $"{ProjectDataPath}/{appName}.json";
//
//             FileHelper.CreateDirectory(ProjectDataPath);
//             File.Copy(dlcVersionFile, appVersionFile, true);
//         }
//
//     }
// }
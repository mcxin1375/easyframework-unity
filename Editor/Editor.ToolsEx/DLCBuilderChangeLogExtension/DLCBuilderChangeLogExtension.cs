// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using System.IO;
// using System.Text;
// using EasyFramework.Editor;
//
// namespace EasyFramework.Server
// {
//     public class DLCBuilderChangeLogExtension : IEasyFrameworkTool<DLCBuilder>
//     {
//         public int Order => int.MaxValue;
//
//         public const string LogFileName = "DLCChangeLog.txt";
//
//         void IEasyFrameworkTool<DLCBuilder>.Execute() => UpdateChangeLog();
//
//         public static void UpdateChangeLog()
//         {
//             StringBuilder sb = new();
//             // var dlcBuilderVersion = DLCBuilderVersion.Load();
//             var dlcBuilderVersion = FEditor.DLCBuilder.LoadDLCVersionList();
//             if (dlcBuilderVersion.versions?.Length > 1)
//             {
//                 for (int i = 0; i < dlcBuilderVersion.versions.Length - 1; i++)
//                 {
//                     var endInfo = dlcBuilderVersion.versions[i];
//                     var startInfo = dlcBuilderVersion.versions[i + 1];
//
//                     sb.AppendLine(string.Empty);
//                     sb.AppendLine($"-------------------------------- DLCVersion: {endInfo.Version}");
//                     sb.AppendLine($"AssetBundleBuilder: {startInfo.assetBundleBuilderVersion.revision} - {endInfo.assetBundleBuilderVersion.revision}");
//                     sb.AppendLine($"DataBuilder: {startInfo.dataBuilderVersion.revision} - {endInfo.dataBuilderVersion.revision}");
//                     sb.AppendLine($"DllBuilder: {startInfo.dllBuilderVersion.revision} - {endInfo.dllBuilderVersion.revision} \n");
//                     
//                     var arr = SVNCommand.Log(EasyFrameworkPreferences.ProjectFullPath, startInfo.dlcBuilderVersion.revision, endInfo.dlcBuilderVersion.revision);
//                     if (arr?.Length > 0)
//                     {
//                         for (int j = arr.Length - 1; j >= 0; j--)
//                         {
//                             var info = arr[j];
//                             if (string.IsNullOrWhiteSpace(info.Message)) continue;
//                             // if (!string.IsNullOrWhiteSpace(settings.startWithStr) &&
//                             //     !info.Message.StartsWith(settings.startWithStr)) continue;
//                             // if (settings.ignoreAuthors?.Length > 0 && settings.ignoreAuthors.Contains(info.Author)) continue;
//                         
//                             sb.AppendLine($"[{info.Revision}] {info.Author}");
//                             sb.AppendLine($"{info.Message.Trim()}\n");
//                         }
//                     }
//                 }
//             }
//             
//             var outputPath = $"{FEditor.DLCBuilder.ProjectDataSpace}/{LogFileName}";
//             var content = sb.ToString();
//             Encoding encoding = new UTF8Encoding(true); 
//             File.WriteAllText(outputPath, content, encoding);
//         }
//     }
// }
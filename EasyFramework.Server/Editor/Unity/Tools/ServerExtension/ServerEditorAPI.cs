// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.IO;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using EasyFramework.Editor;
// using EasyFramework.AOT;
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Server.Editor
// {
//     public class ServerEditorAPI
//     {
//         private static EasyFrameworkServerSettings Settings => EasyFrameworkServerSettings.Instance;
//         
//         public static async Task UploadProjectConfigAsync()
//         {
//             var projectConfig = ServerExtensionSettings.Instance.projectUnityConfig;
//             
//             using HttpClient http = new HttpClient(ServerExtensionHelper.CreateCustomValidationHandler());
//             http.Timeout = TimeSpan.FromSeconds(10);
//             
//             var content = ServerExtensionHelper.CreateUploadProjectConfigContent(projectConfig);
//             var response = await http.PostAsync(Settings.UploadProjectConfig, content);
//             if (!response.IsSuccessStatusCode)
//             {
//                 Debug.LogError($"上传失败：{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
//                 return;
//             }
//             Debug.Log("UploadProjectConfigAsync done!");
//         }
//         
//         public static Task UploadDLCAppAsync() => UploadDLCAsync(EasyFrameworkAOTSettings.App.AppName, EasyFrameworkAOTSettings.App.AppName);
//         public static Task UploadDLCAsync(string localVersionName, string serverVersionName)
//         {
//             return UploadDLCAsync(EasyFrameworkAOTSettings.GetPlatformName(), localVersionName, serverVersionName).SafeEx();
//         }
//         public static async Task UploadDLCAsync(string platformName, string localVersionName, string serverVersionName)
//         {
//             var root = $"{DLCBuilder.Instance.ProjectDataPath}/{localVersionName}";
//             var files =  Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
//             var resFileInfos = ResFileHelper.CreateResFileInfos(root, false, true);
//             
//             int totalFiles = files.Length;
//             
//             using HttpClient http = new HttpClient(ServerExtensionHelper.CreateCustomValidationHandler());
//             http.Timeout = TimeSpan.FromSeconds(10);
//
//             var res = await http.PostAsync(Settings.UploadDLCBefore, ServerExtensionHelper.CreateDLCUploadBeforeContent(platformName, serverVersionName, resFileInfos));
//             var resString = await res.Content.ReadAsStringAsync();
//             if (!res.IsSuccessStatusCode)
//             {
//                 Debug.LogError($"{res.StatusCode} - {resString}");
//                 return;
//             }
//             var serverFiles = NewtonsoftHelper.LoadFromText<ResFileInfo[]>(resString);
//             var serverFileDict = serverFiles.ToDictionary(item => item.name, item => item);
//
//             int progressId = Progress.Start("上传 DLC...", $"共 {totalFiles} 个文件");
//             
//             for (int index = 0; index < totalFiles; index++)
//             {
//                 string filePath = files[index];
//                 var fileInfo = new FileInfo(filePath);
//                 string relative = Path.GetRelativePath(root, filePath).Replace("\\", "/");
//                 
//                 Progress.SetDescription(progressId, $"{relative}");
//                 Progress.Report(progressId, (float)index / totalFiles);
//
//                 bool isLast = index == totalFiles - 1;
//                 if (!isLast && serverFileDict.TryGetValue(relative, out var serverFileInfo))
//                 {
//                     if (fileInfo.Length == serverFileInfo.length && fileInfo.LastWriteTime.ToFileTime() == serverFileInfo.writeTime) continue;
//                 }
//
//                 var uploadContent = ServerExtensionHelper.CreateDLCUploadFileContent(filePath, relative, platformName, serverVersionName, isLast);
//                 var uploadRes = await http.PostAsync(Settings.UploadDLC, uploadContent);
//                 if (!uploadRes.IsSuccessStatusCode)
//                 {
//                     Debug.LogError($"上传失败[{relative}]：{uploadRes.StatusCode} - {await uploadRes.Content.ReadAsStringAsync()}");
//                     break;
//                 }
//             }
//
//             Progress.Finish(progressId, Progress.Status.Succeeded);
//             Debug.Log("UploadDLCAsync done!");
//         }
//         
//         public static async Task<SVCErrorConfig> GetSVCErrorConfigAsync()
//         {
//             using HttpClient http = new HttpClient(ServerExtensionHelper.CreateCustomValidationHandler());
//             http.Timeout = TimeSpan.FromSeconds(10);
//             var content = await http.GetStringAsync(Settings.GetSVCErrorConfig);
//             return NewtonsoftHelper.LoadOrCreateFromText<SVCErrorConfig>(content);
//         }
//         
//         public static async Task<HttpResponseMessage> ClearSVCErrorConfigAsync()
//         {
//             using HttpClient http = new HttpClient(ServerExtensionHelper.CreateCustomValidationHandler());
//             http.Timeout = TimeSpan.FromSeconds(10);
//             return await http.PostAsync(EasyFrameworkServerSettings.Instance.ClearSVCErrorConfig, null);
//         }
//         
//         public static async Task UpdateSVCFromErrorLogAsync()
//         {
//             var projectConfig = ServerExtensionSettings.Instance.projectUnityConfig;
//             
//             using HttpClient http = new HttpClient(ServerExtensionHelper.CreateCustomValidationHandler());
//             http.Timeout = TimeSpan.FromSeconds(10);
//             
//             var content = ServerExtensionHelper.CreateUploadProjectConfigContent(projectConfig);
//             var response = await http.PostAsync(Settings.UploadProjectConfig, content);
//             if (!response.IsSuccessStatusCode)
//             {
//                 Debug.LogError($"上传失败：{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
//                 return;
//             }
//             Debug.Log("UpdateSVCFromErrorLogAsync done!");
//         }
//     }
// }
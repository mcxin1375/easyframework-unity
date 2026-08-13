// using System;
// using System.Diagnostics;
// using System.Text;
//
// namespace EasyFramework.Tools
// {
//     public static partial class SVNCommand
//     {
//         public static async Task CheckoutAsync(string url, string path, bool cleanup, bool revert, bool deleteUnversioned, int revision, Action<string> action, CancellationToken token)
//         {
//             await ExecuteActionAsync(SVNHelper.CheckOut(url, path, revision), action, token);
//             if (cleanup) await ExecuteActionAsync(SVNHelper.Cleanup(path), action, token);
//             if (revert) await ExecuteActionAsync(SVNHelper.Revert(path), action, token);
//             if (deleteUnversioned) await DeleteUnversionedFilesAsync(path, action, token);
//             await ExecuteActionAsync(SVNHelper.Update(path, revision), action, token);
//         }
//
//         public static async Task UpdateAsync(string path, bool cleanup, bool revert, bool deleteUnversioned, int revision, Action<string> action, CancellationToken token)
//         {
//             if (cleanup) await ExecuteActionAsync(SVNHelper.Cleanup(path), action, token);
//             if (revert) await ExecuteActionAsync(SVNHelper.Revert(path), action, token);
//             if (deleteUnversioned) await DeleteUnversionedFilesAsync(path, action, token);
//
//             await ExecuteActionAsync(SVNHelper.Update(path, revision), action, token);
//         }
//
//         public static async Task CommitAllAsync(string path, string desc, Action<string> action, CancellationToken token)
//         {
//             await CommitAllAsync(new string[] { path }, desc, action, token);
//         }
//         public static async Task CommitAllAsync(string[] pathArr, string desc, Action<string> action, CancellationToken token)
//         {
//             List<string> delList = new List<string>();
//             foreach (string path in pathArr)
//             {
//                 // 1. 添加未版本控制的文件
//                 if (token.IsCancellationRequested) return;
//                 await ExecuteActionAsync(SVNHelper.Add(path), action, token);
//
//                 // 2. 检查并删除本地已移除的文件（仅删除已被标记为缺失的项）
//                 var lines = await ExecuteOutputLinesAsync(SVNHelper.Status(path), token);
//                 foreach (var line in lines)
//                 {
//                     if (line.StartsWith("!")) // 缺失的文件
//                     {
//                         var missingFile = line.Substring(1).Trim();
//                         delList.Add(missingFile);
//                     }
//                 }
//             }
//             if (token.IsCancellationRequested) return;
//             await ExecuteActionAsync(SVNHelper.Del(delList.ToArray()), action, token);
//
//             // 3. 提交更改
//             if (token.IsCancellationRequested) return;
//             await ExecuteActionAsync(SVNHelper.Commit(pathArr, desc), action, token);
//         }
//
//         //public static async Task<SvnStatusInfo> StatusAsync(string path, CancellationToken token)
//         //{
//         //    var output = await ExecuteOutputAsync($"status \"{path}\"", false, token);
//         //    return SvnStatusInfo.Parse(output);
//         //}
//
//         //public static async Task<SvnVersionInfo> InfoAsync(string path, CancellationToken token)
//         //{
//         //    var output = await ExecuteOutputAsync($"info --xml \"{path}\"", true, token);
//         //    return SvnVersionInfo.Parse(output);
//         //}
//
//         //public static async Task<SvnVersionLog> LogAsync(string path, int version, CancellationToken token)
//         //{
//         //    var versionInfo = await InfoAsync(path, token);
//         //    if (version == 0) version = versionInfo.Revision;
//
//         //    var output = await ExecuteOutputAsync($"log \"{path}\" -r {version} --xml", true, token);
//         //    return SvnVersionLog.Parse(output);
//         //}
//
//         //public static async Task<SvnVersionLog[]> LogAsync(string path, int startVer, int endVer, CancellationToken token)
//         //{
//         //    var output = await ExecuteOutputAsync($"log \"{path}\" -r {startVer}:{endVer} --xml", true, token);
//         //    return SvnVersionLog.ParseMultiple(output);
//         //}
//
//         public static async Task DeleteUnversionedFilesAsync(string path, Action<string> action, CancellationToken token)
//         {
//             var lines = await ExecuteOutputLinesAsync(SVNHelper.Status(path), CancellationToken.None);
//             foreach (var line in lines)
//             {
//                 if (token.IsCancellationRequested) break;
//
//                 try
//                 {
//                     if (line.StartsWith("?"))
//                     {
//                         var filePath = line.Substring(1).Trim();
//                         if (Directory.Exists(filePath))
//                         {
//                             Directory.Delete(filePath, true);
//                             action?.Invoke(filePath);
//                         }
//                         else if (File.Exists(filePath))
//                         {
//                             File.Delete(filePath);
//                             action?.Invoke(filePath);
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     action?.Invoke(ex.ToString());
//                 }
//             }
//         }
//
//         public static async Task<string> ExecuteOutputAsync(string arguments, bool utf8, CancellationToken token)
//         {
//             var processStartInfo = new ProcessStartInfo
//             {
//                 FileName = SvnFileName,
//                 Arguments = arguments,
//                 RedirectStandardOutput = true,
//                 RedirectStandardError = true,
//                 UseShellExecute = false,
//                 CreateNoWindow = true
//             };
//             if (utf8)
//             {
//                 processStartInfo.StandardOutputEncoding = Encoding.UTF8;
//                 processStartInfo.StandardErrorEncoding = Encoding.UTF8;
//             }
//             using (Process process = new Process { StartInfo = processStartInfo })
//             {
//                 var cancellationRegistration = token.Register(() =>
//                 {
//                     try
//                     {
//                         if (!process.HasExited) process.Kill(true); // 终止进程及其所有子进程
//                     }
//                     catch (Exception ex)
//                     {
//                         // 处理可能的异常，例如进程已经退出
//                         Console.WriteLine($"Failed to kill process: {ex.Message}");
//                     }
//                 });
//                 try
//                 {
//                     process.Start();
//                     var output = process.StandardOutput.ReadToEnd();
//                     await process.WaitForExitAsync(token);
//                     if (process.ExitCode == 0) return output;
//                 }
//                 catch (OperationCanceledException)
//                 {
//                     Console.WriteLine("Operation was canceled.");
//                 }
//                 finally
//                 {
//                     cancellationRegistration.Dispose();
//                 }
//             }
//             return string.Empty;
//
//             //Process process = Process.Start(processStartInfo);
//             //var output = process.StandardOutput.ReadToEnd();
//             //string error = process.StandardError.ReadToEnd();
//             //await process.WaitForExitAsync(token);
//             //return process.ExitCode == 0 ? output : string.Empty;
//         }
//
//         public static async Task<string[]> ExecuteOutputLinesAsync(string arguments, CancellationToken token, bool utf = false)
//         {
//             List<string> output = new List<string>();
//             void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
//             {
//                 if (e.Data != null) output.Add(e.Data);
//             }
//             var processStartInfo = new ProcessStartInfo
//             {
//                 FileName = SvnFileName,
//                 Arguments = arguments,
//                 RedirectStandardOutput = true,
//                 RedirectStandardError = true,
//                 UseShellExecute = false,
//                 CreateNoWindow = true
//             };
//             if (utf)
//             {
//                 processStartInfo.StandardOutputEncoding = Encoding.UTF8;
//                 processStartInfo.StandardErrorEncoding = Encoding.UTF8;
//             }
//             using (Process process = new Process { StartInfo = processStartInfo })
//             {
//                 process.OutputDataReceived += Process_OutputDataReceived;
//                 // 注册 CancellationToken 的取消事件
//                 var cancellationRegistration = token.Register(() =>
//                 {
//                     try
//                     {
//                         if (!process.HasExited) process.Kill(true); // 终止进程及其所有子进程
//                     }
//                     catch (Exception ex)
//                     {
//                         // 处理可能的异常，例如进程已经退出
//                         Console.WriteLine($"Failed to kill process: {ex.Message}");
//                     }
//                 });
//                 try
//                 {
//                     process.Start();
//                     process.BeginOutputReadLine();
//                     await process.WaitForExitAsync(token);
//                 }
//                 catch (OperationCanceledException)
//                 {
//                     // 处理取消异常
//                     Console.WriteLine("Operation was canceled.");
//                 }
//                 finally
//                 {
//                     // 取消事件注册
//                     cancellationRegistration.Dispose();
//                     process.OutputDataReceived -= Process_OutputDataReceived;
//                 }
//                 return process.ExitCode == 0 ? output.ToArray() : null;
//             }
//             //Process process = Process.Start(processStartInfo);
//             //process.OutputDataReceived += Process_OutputDataReceived;
//             //process.BeginOutputReadLine();
//             //// string output = process.StandardOutput.ReadToEnd();
//             ////string error = process.StandardError.ReadToEnd();
//             //await process.WaitForExitAsync(token);
//             //process.OutputDataReceived -= Process_OutputDataReceived;
//             //return process.ExitCode == 0 ? output.ToArray() : null;
//         }
//
//         public static async Task<int> ExecuteActionAsync(string arguments, Action<string> action, CancellationToken token)
//         {
//             //Console.WriteLine(arguments);
//
//             void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
//             {
//                 //Console.WriteLine(e?.Data);
//                 if (e.Data != null) action?.Invoke(e.Data);
//             }
//             var processStartInfo = new ProcessStartInfo
//             {
//                 FileName = SvnFileName,
//                 Arguments = arguments,
//                 RedirectStandardOutput = true,
//                 RedirectStandardError = true,
//                 UseShellExecute = false,
//                 CreateNoWindow = true,
//             };
//             using (Process process = new Process { StartInfo = processStartInfo })
//             {
//                 process.OutputDataReceived += Process_OutputDataReceived;
//                 // 注册 CancellationToken 的取消事件
//                 var cancellationRegistration = token.Register(() =>
//                 {
//                     try
//                     {
//                         //Console.WriteLine("终止进程及其所有子进程");
//                         if (!process.HasExited) process.Kill(true); // 终止进程及其所有子进程
//                     }
//                     catch (Exception ex)
//                     {
//                         // 处理可能的异常，例如进程已经退出
//                         Console.WriteLine($"Failed to kill process: {ex.Message}");
//                     }
//                 });
//                 try
//                 {
//                     process.Start();
//                     process.BeginOutputReadLine();
//                     await process.WaitForExitAsync(token);
//                 }
//                 catch (OperationCanceledException)
//                 {
//                     // 处理取消异常
//                     Console.WriteLine("Operation was canceled.");
//                 }
//                 finally
//                 {
//                     // 取消事件注册
//                     cancellationRegistration.Dispose();
//                     process.OutputDataReceived -= Process_OutputDataReceived;
//                 }
//                 //Process process = Process.Start(processStartInfo);
//                 //process.OutputDataReceived += Process_OutputDataReceived;
//                 //process.BeginOutputReadLine();
//
//                 //// string output = process.StandardOutput.ReadToEnd();
//                 ////string error = process.StandardError.ReadToEnd();
//
//                 //await process.WaitForExitAsync(token);
//                 //process.OutputDataReceived -= Process_OutputDataReceived;
//
//                 return process.ExitCode;
//             }
//         }
//
//     }
// }

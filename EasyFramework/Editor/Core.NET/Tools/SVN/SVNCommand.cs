using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyFramework.Editor
{
    public static partial class SVNCommand
    {
        public static string SvnFileName { get; set; } = "svn";

        public static void Checkout(string url, string path, bool cleanup = false, bool revert = false, bool deleteUnversioned = false, int revision = 0, Action<string> action = null)
        {
            Execute(SVNHelper.CheckOut(url, path, revision), action);
            if (cleanup) Execute(SVNHelper.Cleanup(path), action);
            if (revert) Execute(SVNHelper.Revert(path), action);
            if (deleteUnversioned) DeleteUnversionedFiles(path, action);
            Execute(SVNHelper.Update(path, revision), action);
        }
        
        public static async ETask CheckoutAsync(string url, string path, bool cleanup = false, bool revert = false, bool deleteUnversioned = false, int revision = 0, Action<string> action = null,  CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await ExecuteAsync(SVNHelper.CheckOut(url, path, revision), action, token);

            token.ThrowIfCancellationRequested();
            if (cleanup) await ExecuteAsync(SVNHelper.Cleanup(path), action, token);

            token.ThrowIfCancellationRequested();
            if (revert) await ExecuteAsync(SVNHelper.Revert(path), action, token);

            token.ThrowIfCancellationRequested();
            if (deleteUnversioned) DeleteUnversionedFiles(path, action);

            token.ThrowIfCancellationRequested();
            await ExecuteAsync(SVNHelper.Update(path, revision), action, token);
        }

        public static void Update(string path, Action<string> action = null) => Update(path, 0, false, false, false, action);
        public static void Update(string path, int revision, Action<string> action = null)  => Update(path, revision, false, false, false, action);
        public static void Update(string path, int revision, bool cleanup, bool revert, bool deleteUnversioned, Action<string> action = null)
        {
            if (cleanup) Execute(SVNHelper.Cleanup(path), action);
            if (revert) Execute(SVNHelper.Revert(path), action);
            if (deleteUnversioned) DeleteUnversionedFiles(path, action);

            Execute(SVNHelper.Update(path, revision), action);
        }
        public static async ETask UpdateAsync(string path, int revision, bool cleanup, bool revert, bool deleteUnversioned, Action<string> action = null,  CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (cleanup) await ExecuteAsync(SVNHelper.Cleanup(path), action, token);

            token.ThrowIfCancellationRequested();
            if (revert) await ExecuteAsync(SVNHelper.Revert(path), action, token);

            token.ThrowIfCancellationRequested();
            if (deleteUnversioned) DeleteUnversionedFiles(path, action);

            token.ThrowIfCancellationRequested();
            await ExecuteAsync(SVNHelper.Update(path, revision), action, token);
        }

        public static void CommitAll(string path, string desc, Action<string> action = null) => CommitAll(new string[] { path }, desc, action);
        public static void CommitAll(string[] pathArr, string desc, Action<string> action = null)
        {
            List<string> delList = new();
            List<string> commitList = new List<string>();
            foreach (string path in pathArr)
            {
                // 1. 添加未版本控制的文件
                Execute(SVNHelper.Add(path), action);

                // 2. 检查并删除本地已移除的文件（仅删除已被标记为缺失的项）
                List<string> tmpList = new();
                Execute(SVNHelper.Status(path), tmpList);
                // if (exitCode != 0) throw new Exception($"SVNCommand.Status failed with exit code {exitCode}. path: {path}");
                
                foreach (var line in tmpList)
                {
                    if (line.StartsWith("!")) delList.Add(line.Substring(1).Trim());
                }

                var commitPath = GetVersionedPath(path);
                commitList.Add(commitPath);
            }
            Execute(SVNHelper.Del(delList.ToArray()), action);

            // 3. 提交更改
            Execute(SVNHelper.Commit(commitList.ToArray(), desc), action);
        }

        public static ETask CommitAllAsync(string path, string desc, Action<string> action = null,  CancellationToken token = default) => CommitAllAsync(new string[] { path }, desc, action, token);
        public static async ETask CommitAllAsync(string[] pathArr, string desc, Action<string> action = null,  CancellationToken token = default)
        {
            List<string> delList = new();
            List<string> commitList = new List<string>();
            foreach (string path in pathArr)
            {
                token.ThrowIfCancellationRequested();
                
                // 1. 添加未版本控制的文件
                await ExecuteAsync(SVNHelper.Add(path), action, token);

                // 2. 检查并删除本地已移除的文件（仅删除已被标记为缺失的项）
                List<string> tmpList = new();
                var exitCode = await ExecuteAsync(SVNHelper.Status(path), tmpList, token);
                if (exitCode != 0) throw new Exception($"SVNCommand.Status failed with exit code {exitCode}. path: {path}");
                
                foreach (var line in tmpList)
                {
                    if (line.StartsWith("!")) delList.Add(line.Substring(1).Trim());
                }

                var commitPath = GetVersionedPath(path);
                commitList.Add(commitPath);
            }
            
            token.ThrowIfCancellationRequested();
            await ExecuteAsync(SVNHelper.Del(delList.ToArray()), action, token);

            token.ThrowIfCancellationRequested();
            await ExecuteAsync(SVNHelper.Commit(commitList.ToArray(), desc), action, token);
        }

        public static string GetVersionedPath(string path)
        {
            var info = Info(path);
            if (info?.Revision > 0) return path;

            string parentPath = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parentPath) || parentPath == path) return path;
            return GetVersionedPath(parentPath);
        }

        public static SvnStatusInfo Status(string path)
        {
            var exitCode = Execute($"status \"{path}\"", out var outputText);
            if (exitCode != 0) throw new Exception($"SVNCommand.Status failed with exit code {exitCode}. path: {path}");
            
            return SvnStatusInfo.Parse(outputText);
        }

        public static SvnVersionInfo Info(string path)
        {
            var exitCode = Execute($"info --xml \"{path}\"", out var outputText, true);
            if (exitCode != 0) throw new Exception($"SVNCommand.Info failed with exit code {exitCode}. path: {path}");
            
            return SvnVersionInfo.Parse(outputText);
        }

        public static SvnVersionLog Log(string path, int version = 0)
        {
            var versionInfo = Info(path);
            if (version == 0) version = versionInfo.Revision;

            var exitCode = Execute($"log \"{path}\" -r {version} --xml", out var outputText, true);
            if (exitCode != 0) throw new Exception($"SVNCommand.Log failed with exit code {exitCode}. path: {path}");
            
            return SvnVersionLog.Parse(outputText);
        }

        public static SvnVersionLog[] Log(string path, int startVer, int endVer)
        {
            var exitCode = Execute($"log \"{path}\" -r {startVer}:{endVer} --xml", out var outputText, true);
            if (exitCode != 0) throw new Exception($"SVNCommand.Log failed with exit code {exitCode}. path: {path}");
            return SvnVersionLog.ParseMultiple(outputText);
        }

        public static int GetRevision(string path) => TryGetRevision(path, out int revision) ? revision : 0;
        public static bool TryGetRevision(string path, out int revision)
        {
            revision = 0;
            if (string.IsNullOrWhiteSpace(path)) return false;
            try
            {
                var svnInfo = Info(path);
                revision = svnInfo.Revision;
                return true;
            }
            catch (Exception e) { return false; }
        }

        public static void DeleteUnversionedFiles(string path, Action<string> action = null)
        {
            List<string> outputList = new();
            var exitCode = Execute(SVNHelper.Status(path), outputList);
            if (exitCode != 0) throw new Exception($"SVNCommand.DeleteUnversionedFiles failed with exit code {exitCode}. path: {path}");
            foreach (var line in outputList)
            {
                try
                {
                    if (line.StartsWith("?"))
                    {
                        var filePath = line.Substring(1).Trim();
                        if (Directory.Exists(filePath))
                        {
                            Directory.Delete(filePath, true);
                            action?.Invoke(filePath);
                        }
                        else if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            action?.Invoke(filePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    action?.Invoke(ex.ToString());
                }
            }
        }

        public static int Execute(string arguments, out string outputText, bool utf8 = false)
        {
            outputText = string.Empty;
            var processStartInfo = new ProcessStartInfo
            {
                FileName = SvnFileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            if (utf8)
            {
                processStartInfo.StandardOutputEncoding = Encoding.UTF8;
                processStartInfo.StandardErrorEncoding = Encoding.UTF8;
            }

            using Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            outputText = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode;
        }

        public static int Execute(string arguments, Action<string> action) => Execute(arguments, action, null);
        public static int Execute(string arguments, List<string> outputList) => Execute(arguments, null, outputList);
        public static int Execute(string arguments, Action<string> action, List<string> outputList)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = SvnFileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            // if (utf)
            // {
            //     processStartInfo.StandardOutputEncoding = Encoding.UTF8;
            //     processStartInfo.StandardErrorEncoding = Encoding.UTF8;
            // }
            
            using Process process = new Process();
            process.StartInfo = processStartInfo;
            process.OutputDataReceived += ProcessOutputDataReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.OutputDataReceived -= ProcessOutputDataReceived;
            return process.ExitCode;

            void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    action?.Invoke(e.Data);
                    outputList?.Add(e.Data);
                }
            }
        }
        public static ETask<int> ExecuteAsync(string arguments, Action<string> action, CancellationToken token = default) => ExecuteAsync(arguments, action, null, token);
        public static ETask<int> ExecuteAsync(string arguments, List<string> outputList, CancellationToken token = default) => ExecuteAsync(arguments, null, outputList, token);
        public static async ETask<int> ExecuteAsync(string arguments, Action<string> action, List<string> outputList, CancellationToken token = default)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = SvnFileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            // if (utf)
            // {
            //     processStartInfo.StandardOutputEncoding = Encoding.UTF8;
            //     processStartInfo.StandardErrorEncoding = Encoding.UTF8;
            // }
            
            using Process process = new Process();
            process.StartInfo = processStartInfo;
            process.OutputDataReceived += ProcessOutputDataReceived;

            var cancellationRegistration = token.Register(() =>
            {
                try
                {
                    if (process != null)
                    {
                        //Console.WriteLine("终止进程及其所有子进程");
                        if (!process.HasExited) process.Kill(); // 终止进程及其所有子进程
                    }

                }
                catch (Exception ex)
                {
                    // 处理可能的异常，例如进程已经退出
                    Console.WriteLine($"Failed to kill process: {ex.Message}");
                }
            });
            
            try
            {
                await Task.Run(() =>
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                }, token);
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                action?.Invoke(error);
                outputList?.Add(error);
            }
            finally
            {
                // 取消事件注册
                cancellationRegistration.Dispose();
                process.OutputDataReceived -= ProcessOutputDataReceived;
            }
            
            return process.ExitCode;

            void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    action?.Invoke(e.Data);
                    outputList?.Add(e.Data);
                }
            }
        }

    }
}

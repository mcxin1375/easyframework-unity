using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace EasyFramework.Editor
{
    [Serializable]
    public class ProtocCommandSettings
    {
        public string namespaceName = "EasyFramework";
        public string dataPath;
        public string outputProtocPath;
        public string outputProxyPath;
        public string svnVersionPath;
        public string protocExeFile;
    }
    public static class ProtocCommand
    {
        public static void Execute(ProtocCommandSettings settings, Action<string> action = null)
        {
            if (!Directory.Exists(settings.dataPath)) throw new Exception($"dataPath not exists: {settings.dataPath}");
            if (string.IsNullOrWhiteSpace(settings.outputProtocPath)) throw new Exception($"outputProtocPath is empty: {settings.outputProtocPath}");
            if (string.IsNullOrWhiteSpace(settings.outputProxyPath)) throw new Exception($"outputProxyPath is empty : {settings.outputProxyPath}");

            if (string.IsNullOrWhiteSpace(settings.protocExeFile)) settings.protocExeFile = GetProtocPath();
            if (!File.Exists(settings.protocExeFile)) throw new Exception($"protocExeFile not exists: {settings.protocExeFile}");

            FileHelper.CreateDirectory(settings.outputProxyPath);
            FileHelper.CreateDirectory(settings.outputProtocPath);

            FileHelper.DeleteFiles(Directory.GetFiles(settings.outputProxyPath, "*.cs"));
            FileHelper.DeleteFiles(Directory.GetFiles(settings.outputProtocPath, "*.cs"));

            var args = $"--proto_path={settings.dataPath} --csharp_out={settings.outputProtocPath} {settings.dataPath}/*.proto";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = settings.protocExeFile,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                action?.Invoke(output);
                if (!string.IsNullOrEmpty(error))
                {
                    action?.Invoke($"错误信息：{error}");
                }
            }

            ProtocolInfo pbInfo = ProtocolLoader.Load(settings.dataPath);
            if (pbInfo.MessageInfos?.Length < 1)
                return;

            SVNCommand.TryGetRevision(settings.svnVersionPath, out var svnRevision);

            File.WriteAllText($"{settings.outputProxyPath}/CMD.cs", ProtocGenerator.CreateCmd(pbInfo, svnRevision, settings.namespaceName));
            File.WriteAllText($"{settings.outputProxyPath}/IMessageHandler.cs", ProtocGenerator.CreateMessageHandler(pbInfo, settings.namespaceName));
            File.WriteAllText($"{settings.outputProxyPath}/IMessageSender.cs", ProtocGenerator.CreateMessageSenderEx(pbInfo, settings.namespaceName));
            File.WriteAllText($"{settings.outputProxyPath}/MessageProxy.cs", ProtocGenerator.CreateMessageProxy(pbInfo, settings.namespaceName));
            File.WriteAllText($"{settings.outputProxyPath}/MessageProxy.Deserialize.cs", ProtocGenerator.CreateMessageProxyDeserialize(pbInfo, settings.namespaceName));
        }

        static string GetProtocPath()
        {
            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget",
                "packages",
                "google.protobuf.tools");

            if (!Directory.Exists(basePath))
            {
                return null;
            }

            string[] versions = Directory.GetDirectories(basePath);
            Array.Sort(versions, (a, b) => string.Compare(Path.GetFileName(a), Path.GetFileName(b), StringComparison.OrdinalIgnoreCase));
            string latestVersionPath = versions[versions.Length - 1];

            string toolsPath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                toolsPath = Path.Combine(latestVersionPath, "tools", "windows_x64", "protoc.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                toolsPath = Path.Combine(latestVersionPath, "tools", "linux_x64", "protoc");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                toolsPath = Path.Combine(latestVersionPath, "tools", "osx_x64", "protoc");
            }
            else
            {
                return null;
            }

            if (File.Exists(toolsPath))
            {
                return toolsPath;
            }

            return null;
        }
    }
}

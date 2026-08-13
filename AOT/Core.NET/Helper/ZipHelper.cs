/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace EasyFramework
{
    public static class ZipHelper
    {
        public static void ZipDirectory(string sourceDirectory, string zipFile, Action<float> progressCallback = null, CancellationToken cancellationToken = default) =>
            ZipDirectory(sourceDirectory, zipFile, CompressionLevel.Optimal, progressCallback, cancellationToken);
        public static void ZipDirectory(string sourceDirectory, string zipFile, CompressionLevel compressionLevel, Action<float> progressCallback = null, CancellationToken cancellationToken = default)
        {
            if (File.Exists(zipFile)) File.Delete(zipFile);
            string dir = Path.GetDirectoryName(zipFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            
            var directoryInfo = new DirectoryInfo(sourceDirectory);
            var fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            var totalBytes = fileInfos.Sum(file => file.Length);
            long compressedBytes = 0;
            using var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create);
            foreach (var file in fileInfos)
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                string entryName = Path.GetRelativePath(sourceDirectory, file.FullName).Replace('\\', '/');
                archive.CreateEntryFromFile(file.FullName, entryName, compressionLevel);
                compressedBytes += file.Length; // 计算进度并调用回调
                progressCallback?.Invoke((float)compressedBytes / totalBytes);
            }
        }

        public static void ZipFiles(string[] files, string zipFile, Action<string, int, int> progressCallback = null, CancellationToken cancellationToken = default) => ZipFiles(files, zipFile, CompressionLevel.Optimal, progressCallback, cancellationToken);
        public static void ZipFiles(string[] files, string zipFile, CompressionLevel compressionLevel, Action<string, int, int> progressCallback = null, CancellationToken cancellationToken = default)
        {
            if (File.Exists(zipFile)) File.Delete(zipFile);
            string dir = Path.GetDirectoryName(zipFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            
            using var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create);
            for (int i = 0; i < files.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                string file = files[i];
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) continue;
                
                string entryName = Path.GetFileName(file);
                archive.CreateEntryFromFile(file, entryName, compressionLevel);

                progressCallback?.Invoke(file, i, files.Length); // 计算进度并调用回调
            }
        }
        
        public static bool UnzipFile(string zipFile, string extractPath, Action<string, float> progressCallback = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(zipFile)) return false;
            if (!Directory.Exists(extractPath)) Directory.CreateDirectory(extractPath);

            using ZipArchive archive = ZipFile.OpenRead(zipFile);
            var totalFiles = archive.Entries.Count;
            int extractedFiles = 0;

            try
            {
                foreach (var entry in archive.Entries)
                {
                    if (cancellationToken.IsCancellationRequested) return false;

                    string fullPath = Path.Combine(extractPath, entry.FullName);
                    if (entry.Length == 0) Directory.CreateDirectory(fullPath); // 创建空文件夹
                    else entry.ExtractToFile(fullPath, overwrite: true);  // 解压文件
              
                    extractedFiles++;
                    progressCallback?.Invoke(zipFile, (float)extractedFiles / totalFiles);  // 计算进度并调用回调
                }

            }
            catch (Exception e)
            {
                FDebug.LogException(e);
                return false;
            }
            return true;
        }
    }
}
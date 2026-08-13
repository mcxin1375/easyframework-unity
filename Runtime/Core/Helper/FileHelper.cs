using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyFramework
{
    public static class FileHelper
    {
        public static string[] FilterMeta(string[] files) => Filter(files, ".meta");
        public static string[] Filter(string[] files, string filterStr)
        {
            List<string> result = new List<string>();
            foreach (string file in files)
            {
                if (file.IndexOf(filterStr) > 0) continue;
                result.Add(file);
            }
            return result.ToArray();
        }
        
        public static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        public static void DeleteFile(string file)
        {
            if (File.Exists(file)) File.Delete(file);
        }
        public static void DeleteFiles(string[] files)
        {
            foreach (string file in files) DeleteFile(file);
        }
        public static void DeleteDirectoryOrFile(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
            if (File.Exists(path)) File.Delete(path);
        }
        public static long GetFileSize(string file)
        {
            return File.Exists(file) ? new FileInfo(file).Length : 0;
        }
        public static void CreateDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory)) return;
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        }
        public static void ClearDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;

            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            foreach (string file in files) File.Delete(file);

            string[] directories = Directory.GetDirectories(directory);
            foreach (string d in directories) Directory.Delete(d, true);
        }

        public static bool CompareFileLengthAndLastWriteTime(string file1, string file2)
        {
            if (!File.Exists(file1) || !File.Exists(file2)) return true;
            FileInfo fileInfo = new FileInfo(file1);
            FileInfo fileInfoTo = new FileInfo(file2);
            return fileInfo.Length != fileInfoTo.Length || fileInfo.LastWriteTime != fileInfoTo.LastWriteTime;
        }

        public static async Task CopyDirectoryAsync(string sourceDirectory, string destinationDirectory, bool compareDifferent, bool deleteNotExists, Action<string, int, int> copyAction, CancellationToken token = default)
        {
            await Task.Run(() => {
                CopyDirectory(sourceDirectory, destinationDirectory, compareDifferent, deleteNotExists, copyAction);
            }, token);
        }
        public static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool compareDifferent, bool deleteNotExists, Action<string, int, int> copyAction = null)
        {
            if (!Directory.Exists(sourceDirectory)) return;

            var files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
            var directories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);

            for (int i = 0; i < directories.Length; i++)
            {
                string directory = directories[i];

                string relativePath = Path.GetRelativePath(sourceDirectory, directory);
                string destinationPath = Path.Combine(destinationDirectory, relativePath);
                CreateDirectory(destinationPath);
            }
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];

                string relativePath = Path.GetRelativePath(sourceDirectory, file);
                string destinationFile = Path.Combine(destinationDirectory, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                if (compareDifferent)
                {
                    if (CompareFileLengthAndLastWriteTime(file, destinationFile))
                    {
                        File.Copy(file, destinationFile, true);
                        copyAction?.Invoke(file, i + 1, files.Length);
                    }
                    else
                    {
                        copyAction?.Invoke(string.Empty, i + 1, files.Length);
                    }
                }
                else
                {
                    File.Copy(file, destinationFile, true);
                    copyAction?.Invoke(file, i + 1, files.Length);
                }
            }
            if (deleteNotExists)
            {
                DeleteNotExists(sourceDirectory, destinationDirectory);
            }
        }

        public static void CopyDirectoryDeleteNotExists(string sourceDirectory, string destinationDirectory, bool compareDifferent = false, Action<string, float> copyAction = null, Action<string, float> deleteAction = null)
        {
            if (!Directory.Exists(sourceDirectory)) return;

            string[] files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
            CopyFilesKeepRelativePath(sourceDirectory, files, destinationDirectory, compareDifferent, copyAction);
            DeleteNotExists(sourceDirectory, destinationDirectory, deleteAction);
        }

        public static void CopyDirectoryDeleteNotExistsWithoutHidden(string sourceDirectory, string destinationDirectory, bool compareDifferent = false, Action<string, float> copyAction = null, Action<string, float> deleteAction = null)
        {
            if (!Directory.Exists(sourceDirectory)) return;

            string[] files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
            CopyFilesKeepRelativePath(sourceDirectory, files, destinationDirectory, compareDifferent, copyAction);
            DeleteNotExistsWithoutHidden(sourceDirectory, destinationDirectory, deleteAction);
        }

        public static void CopyDirectoryDeleteNotExistsMeta(string sourceDirectory, string destinationDirectory, bool compareDifferent = false, Action<string, float> copyAction = null, Action<string> deleteAction = null)
        {
            if (!Directory.Exists(sourceDirectory)) return;

            string[] files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
            CopyFilesKeepRelativePath(sourceDirectory, files, destinationDirectory, compareDifferent, copyAction);
            DeleteNotExistsMeta(sourceDirectory, destinationDirectory, deleteAction);
        }

        public static void CopyFilesKeepRelativePath(string sourceDirectory, string[] sourceFiles, string destinationDirectory, bool compareDifferent = false, Action<string, float> progressAction = null)
        {
            float index = 0;
            foreach (string file in sourceFiles)
            {
                string relativePath = Path.GetRelativePath(sourceDirectory, file);
                string destinationFile = Path.Combine(destinationDirectory, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                if (compareDifferent)
                {
                    if (CompareFileLengthAndLastWriteTime(file, destinationFile)) File.Copy(file, destinationFile, true);
                }
                else
                {
                    File.Copy(file, destinationFile, true);
                }

                progressAction?.Invoke(file, index++ / sourceFiles.Length);
            }
        }
        
        public static void DeleteNotExists(string sourceDirectory, string destinationDirectory, Action<string, float> progressAction = null)
        {
            if (!Directory.Exists(sourceDirectory) || !Directory.Exists(destinationDirectory)) return;

            var localRelativeDirectories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories)
                .Select(item => Path.GetRelativePath(sourceDirectory, item).Replace('\\', '/')).ToHashSet();
            var localRelativeFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories)
                .Select(item => Path.GetRelativePath(sourceDirectory, item).Replace('\\', '/')).ToHashSet();

            var desRelativeDirectories = Directory.GetDirectories(destinationDirectory, "*", SearchOption.AllDirectories)
                .ToDictionary(item => Path.GetRelativePath(destinationDirectory, item).Replace('\\', '/'), item => item);
            var desRelativeFiles = Directory.GetFiles(destinationDirectory, "*", SearchOption.AllDirectories)
                .ToDictionary(item => Path.GetRelativePath(destinationDirectory, item).Replace('\\', '/'), item => item);

            int index = 0;
            float len = desRelativeFiles.Count + desRelativeDirectories.Count;
            foreach (var dict in desRelativeFiles)
            {
                if (!localRelativeFiles.Contains(dict.Key)) DeleteDirectoryOrFile(dict.Value);
                progressAction?.Invoke(dict.Value, index++ / len);
            }

            foreach (var dict in desRelativeDirectories)
            {
                if (!localRelativeDirectories.Contains(dict.Key)) DeleteDirectoryOrFile(dict.Value);
                progressAction?.Invoke(dict.Value, index++ / len);
            }
        }

        public static void DeleteNotExistsMeta(string sourceDirectory, string destinationDirectory, Action<string> progressAction = null)
        {
            if (!Directory.Exists(sourceDirectory) || !Directory.Exists(destinationDirectory)) return;

            var localRelativeDirectories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories)
                .Select(item => Path.GetRelativePath(sourceDirectory, item).Replace('\\', '/')).ToHashSet();
            var localRelativeFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories)
                .Select(item => Path.GetRelativePath(sourceDirectory, item).Replace('\\', '/')).ToHashSet();

            var desRelativeDirectories = Directory.GetDirectories(destinationDirectory, "*", SearchOption.AllDirectories)
                .ToDictionary(item => Path.GetRelativePath(destinationDirectory, item).Replace('\\', '/'), item => item);
            var desRelativeFiles = Directory.GetFiles(destinationDirectory, "*", SearchOption.AllDirectories)
                .ToDictionary(item => Path.GetRelativePath(destinationDirectory, item).Replace('\\', '/'), item => item);

            int index = 0;
            float len = desRelativeFiles.Count + desRelativeDirectories.Count;
            foreach (var dict in desRelativeFiles)
            {
                index++;
                if (dict.Key.EndsWith(".meta"))
                {
                    string key = dict.Key.EndsWith(".meta") ? dict.Key.Replace(".meta", "") : dict.Key;
                    //Console.WriteLine($"desRelativeFiles: {key}, {localRelativeFiles.Contains(key)}, {localRelativeDirectories.Contains(key)}");
                    if (localRelativeFiles.Contains(key) || localRelativeDirectories.Contains(key)) continue;
                }
                else
                {
                    if (localRelativeFiles.Contains(dict.Key)) continue;
                }

                DeleteDirectoryOrFile(dict.Value);
                progressAction?.Invoke(dict.Value);
            }

            foreach (var dict in desRelativeDirectories)
            {
                if (localRelativeDirectories.Contains(dict.Key)) continue;
                DeleteDirectoryOrFile(dict.Value);
                progressAction?.Invoke(dict.Value);
            }
        }
        
        public static void DeleteNotExistsWithoutHidden(string sourceDirectory, string destinationDirectory, Action<string, float> progressAction = null)
        {
            if (!Directory.Exists(sourceDirectory) || !Directory.Exists(destinationDirectory)) return;

            var localRelativeDirectories = GetDirectoriesWithoutHidden(sourceDirectory)
                .Select(item => Path.GetRelativePath(sourceDirectory, item).Replace('\\', '/')).ToHashSet();
            var localRelativeFiles = GetFilesWithoutHidden(sourceDirectory)
                .Select(item => Path.GetRelativePath(sourceDirectory, item).Replace('\\', '/')).ToHashSet();

            var desRelativeDirectories = GetDirectoriesWithoutHidden(destinationDirectory)
                .ToDictionary(item => Path.GetRelativePath(destinationDirectory, item).Replace('\\', '/'), item => item);
            var desRelativeFiles = GetFilesWithoutHidden(destinationDirectory)
                .ToDictionary(item => Path.GetRelativePath(destinationDirectory, item).Replace('\\', '/'), item => item);

            int index = 0;
            float len = desRelativeFiles.Count + desRelativeDirectories.Count;
            foreach (var dict in desRelativeFiles)
            {
                if (!localRelativeFiles.Contains(dict.Key)) DeleteDirectoryOrFile(dict.Value);
                progressAction?.Invoke(dict.Value, index++ / len);
            }

            foreach (var dict in desRelativeDirectories)
            {
                if (!localRelativeDirectories.Contains(dict.Key)) DeleteDirectoryOrFile(dict.Value);
                progressAction?.Invoke(dict.Value, index++ / len);
            }
        }

        public static void DeleteNotExistsRelativeFiles(string sourceDirectory, string[] relativeFiles, Action<string, float> progressAction = null)
        {
            if (!Directory.Exists(sourceDirectory)) return;

            var relativeFilesHashSet = relativeFiles.ToHashSet();
            var sourceRelativeFilesDict = GetFilesWithoutHidden(sourceDirectory)
                .ToDictionary(item => Path.GetRelativePath(sourceDirectory, item).Replace('\\', '/'), item => item);
            int index = 0;
            float len = sourceRelativeFilesDict.Count;
            foreach (var dict in sourceRelativeFilesDict)
            {
                if (!relativeFilesHashSet.Contains(dict.Key))
                {
                    DeleteDirectoryOrFile(dict.Value);
                }
                progressAction?.Invoke(dict.Value, index++ / len);
            }
        }
        
        public static string[] GetFilesWithoutHidden(string directory)
        {
            List<string> list = new List<string>();
            void GetList(string dir)
            {
                if (!Directory.Exists(dir)) return;

                string fileName = Path.GetFileName(dir);
                if (fileName.StartsWith(".")) return;

                string[] dirs = Directory.GetDirectories(dir);
                foreach (string item in dirs) GetList(item);

                string[] arr = Directory.GetFiles(dir).Where(item => !item.StartsWith(".")).ToArray();
                list.AddRange(arr);
            }
            GetList(directory);
            return list.ToArray();
        }

        public static string[] GetDirectoriesWithoutHidden(string directory)
        {
            List<string> list = new List<string>();
            void GetList(string dir)
            {
                if (!Directory.Exists(dir)) return;

                string fileName = Path.GetFileName(dir);
                if (fileName.StartsWith(".")) return;

                string[] dirs = Directory.GetDirectories(dir);
                foreach (string item in dirs)
                {
                    list.Add(item);
                    GetList(item);
                }
            }
            GetList(directory);
            return list.ToArray();
        }
        
        public static void CopyFiles(string[] files, string destinationDirectory, bool compareDifferent = false, bool deleteNotExists = false, Action<string, int, int> progressAction = null)
        {
            CreateDirectory(destinationDirectory);

            if (deleteNotExists)
            {
                var directories = Directory.GetDirectories(destinationDirectory, "*", SearchOption.AllDirectories);
                foreach (var directory in directories) DeleteDirectory(directory);
                
                var hash = files.Select(Path.GetFileName).ToHashSet();
                var targetFiles = Directory.GetFiles(destinationDirectory, "*", SearchOption.AllDirectories);
                for (int i = 0; i < targetFiles.Length; i++)
                {
                    var file = targetFiles[i];
                    if (hash.Contains(Path.GetFileName(file))) continue;
                    File.Delete(file);
                }
            }
            
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                if (string.IsNullOrWhiteSpace(file)) continue;
                
                string toFile = $"{destinationDirectory}/{Path.GetFileName(file)}";
                if (compareDifferent)
                {
                    if (CompareFileLengthAndLastWriteTime(file, toFile)) File.Copy(file, toFile, true);
                }
                else
                {
                    File.Copy(file, toFile, true);
                }
                progressAction?.Invoke(file, i + 1, files.Length);
            }
        }
        public static void CopyFilesCompareFileLengthAndLastWriteTime(string[] files, string destinationDirectory, Action<string, float> progressAction = null)
        {
            CreateDirectory(destinationDirectory);
            float index = 0;
            foreach (string file in files)
            {
                string toFile = $"{destinationDirectory}/{Path.GetFileName(file)}";
                if (CompareFileLengthAndLastWriteTime(file, toFile)) File.Copy(file, toFile, true);
                progressAction?.Invoke(file, index++ / files.Length);
            }
        }
        public static void CopyFilesMD5(string[] files, string destinationDirectory, Action<string, float> progressAction = null)
        {
            CreateDirectory(destinationDirectory);
            float index = 0;
            foreach (string file in files)
            {
                string toFile = $"{destinationDirectory}/{Path.GetFileName(file)}";
                if (!MD5Helper.MD5FileEqual(file, toFile)) File.Copy(file, toFile, true);
                progressAction?.Invoke(file, index++ / files.Length);
            }
        }

    }
}

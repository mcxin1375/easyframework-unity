/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;

namespace EasyFramework
{
    public static class ResFileHelper
    {
        public static HashFileInfo[] ConvertHashFiles(string directory, Action<float> action = null)
        {
            if (!Directory.Exists(directory)) return null;
            
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            var arr = new HashFileInfo[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                var resFile = files[i];
                var fi = new FileInfo(resFile);
                var md5 = MD5Helper.MD5File(resFile);
                var hashFileName = $"{md5}{Path.GetExtension(resFile)}";
                arr[i] = new HashFileInfo
                {
                    resName = Path.GetFileName(resFile),
                    fileName = hashFileName,
                    length = fi.Length,
                };
                
                var newFile = $"{Path.GetDirectoryName(resFile)}/{hashFileName}";
                File.Move(resFile, newFile);
                
                action?.Invoke(((float)i + 1) / files.Length);
            }
            return arr;
        }
        
        public static ResFileInfo[] CreateResFileInfos(string directory, bool crc, bool relative, Action<float> action = null)
        {
            if (!Directory.Exists(directory)) return null;
            
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            var fileInfos = new ResFileInfo[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                var resFile = files[i];
                var fi = new FileInfo(resFile);
                var name = relative ? Path.GetRelativePath(directory, resFile).Replace("\\", "/") : Path.GetFileName(resFile);
                fileInfos[i] = new ResFileInfo
                {
                    name = name,
                    length = fi.Length,
                    writeTime = fi.LastWriteTime.ToFileTime(),
                    crc32 = crc ? CRC32Helper.HashFile(resFile) : 0,
                };
                
                action?.Invoke(((float)i + 1) / files.Length);
            }
            return fileInfos;
        }
        
        public static ResFileInfo[] CreateResFileInfos(string directory, Action<float> action = null)
        {
            if (!Directory.Exists(directory)) return null;
            
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            return CreateResFileInfos(files, action);
        }
        
        public static ResFileInfo[] CreateResFileInfos(string[] files, Action<float> action = null) => CreateResFileInfos(files, true, action);
        public static ResFileInfo[] CreateResFileInfos(string[] files, bool crc, Action<float> action = null)
        {
            var fileInfos = new ResFileInfo[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                var resFile = files[i];
                var fi = new FileInfo(resFile);
                fileInfos[i] = new ResFileInfo
                {
                    name = Path.GetFileName(resFile),
                    length = fi.Length,
                    writeTime = fi.LastWriteTime.ToFileTime(),
                    crc32 = crc ? CRC32Helper.HashFile(resFile) : 0,
                };
                
                action?.Invoke(((float)i + 1) / files.Length);
            }
            return fileInfos;
        }
        
        
        public static MD5FileInfo[] CreateMD5FileInfos(string directory, Action<float> action = null)
        {
            if (!Directory.Exists(directory)) return null;
            
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            return CreateMD5FileInfos(files, action);
        }
        public static MD5FileInfo[] CreateMD5FileInfos(string[] files, Action<float> action = null)
        {
            var fileInfos = new MD5FileInfo[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                var resFile = files[i];
                var fi = new FileInfo(resFile);
                var md5 = MD5Helper.MD5File(resFile);
                fileInfos[i] = new MD5FileInfo
                {
                    fileName = Path.GetFileName(resFile),
                    md5 = md5,
                    length = fi.Length,
                    writeTime = fi.LastWriteTime.ToFileTime(),
                };
                
                action?.Invoke(((float)i + 1) / files.Length);
            }
            return fileInfos;
        }
    }
}
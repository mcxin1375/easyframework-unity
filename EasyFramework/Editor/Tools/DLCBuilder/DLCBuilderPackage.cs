/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;

namespace EasyFramework.Editor
{
    public class DLCResBuildInfo
    {
        public string ResTag { get; private set; }
        public string ResName { get; private set; }
        public string[] ResFiles { get; private set; }
        public string BuildFile { get; private set; }

        public DLCResBuildInfo(string resTag, string buildFile, string[] resFiles)
        {
            ResTag = resTag;
            ResName = Path.GetFileName(buildFile);
            ResFiles = resFiles;
            BuildFile = buildFile;
        }
    }
    
    public class DLCBuilderPackage
    {
        public readonly string PackageName;
        public readonly List<string> BuildDirectories = new();
        
        public List<DLCResBuildInfo> ResBuildList { get; } = new();
        public Dictionary<string, List<DLCResBuildInfo>> ZipDict  { get; } = new();
        
        public DLCBuilderPackage(string packageName)
        {
            PackageName = packageName;
        }
        public DLCBuilderPackage(string packageName, string[] buildDirectories)
        {
            if (string.IsNullOrWhiteSpace(packageName)) throw new Exception("Package name can not be null");
            
            PackageName = packageName;
            BuildDirectories.AddRange(buildDirectories);
        }
        public void AddDirectory(string directory)
        {
            var path = directory.Replace("\\", "/");
            if (!BuildDirectories.Contains(path)) BuildDirectories.Add(path);
        }
        
        public bool ContainsFile(string resFile)
        {
            foreach (var directory in BuildDirectories) if (resFile.StartsWith(directory)) return true;
            return false;
        }

        public void Add(string resTag, string buildFile, string[] resFiles = null)
        {
            var buildInfo = new DLCResBuildInfo(resTag, buildFile, resFiles);
            ResBuildList.Add(buildInfo);
            
            string resPath = resFiles?.Length > 0 ? Path.GetDirectoryName(resFiles[0]) : string.Empty;
            if (!string.IsNullOrWhiteSpace(resPath))
            {
                resPath = $"{resTag} : {resPath}";
                if (!ZipDict.ContainsKey(resPath)) ZipDict.Add(resPath, new());
                ZipDict[resPath].Add(buildInfo);
            }
            else
            {
                string commonKey = $"{resTag} : AAA_{PackageName}";
                if (!ZipDict.ContainsKey(commonKey)) ZipDict.Add(commonKey, new());
                ZipDict[commonKey].Add(buildInfo);
            }
        }
    }

}
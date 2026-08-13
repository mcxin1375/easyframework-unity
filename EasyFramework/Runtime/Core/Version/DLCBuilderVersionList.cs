/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyFramework
{
    [Serializable]
    public class DLCBuilderVersionList
    {
        public const string FileName = "DLCBuilderVersionList.json";
        public const string LatestVersion = "DLCVersion:0 (LatestVersion)";
        
        public DLCBuilderVersion[] versions;
        
        
        public string[] ToHistoryVersions()
        {
            var selections = versions
                ?.Select(item => $"DLCVersion:{item.version} SVNRevision:{item.revision} DateTime:{item.DateTimeStr}")
                .ToArray() ?? Array.Empty<string>();
            return selections;
        }
        
        public string[] ToSelections()
        {
            List<string> tmpList = new();
            tmpList.Add(LatestVersion);
            // tmpList.Add(SkipVersion);
            var selections = versions
                ?.Select(item => $"DLCVersion:{item.version} SVNRevision:{item.revision} DateTime:{item.DateTimeStr}")
                .ToArray() ?? Array.Empty<string>();
            tmpList.AddRange(selections);
            return tmpList.ToArray();
        }

        public static int ParseSelectionVersion(string item)
        {
            if (item == LatestVersion) return 0;
            // if (item == SkipVersion) return -1;
            
            if (int.TryParse(item, out var itemVal)) return itemVal;
            var str = SubString(item, "DLCVersion:", "SVNRevision").Trim();
            if (int.TryParse(str, out var result)) return result;
            return -1;
        }
        
        private static string SubString(string source, string start, string end)
        {
            string pattern = $"{Regex.Escape(start)}(.*?){Regex.Escape(end)}";
            Match match = Regex.Match(source, pattern);
            return match.Success && match.Groups.Count > 0 ? match.Groups[1].Value : string.Empty;
        }
        
        public static void Refresh(string rootPath, int maxCount = 0, HashSet<string> archiveNameList = null)
        {
            if (!Directory.Exists(rootPath)) return;
            
            var directories = Directory.GetDirectories(rootPath);
            List<DLCBuilderVersion> versionList = new();
            foreach (var directory in directories)
            {
                string versionFile = $"{directory}/{nameof(DLCBuilderVersion)}.json";
                if (!File.Exists(versionFile)) continue;

                var info = UnityJsonHelper.LoadOrCreate<DLCBuilderVersion>(versionFile);
                versionList.Add(info);
            }
            versionList = versionList.OrderByDescending(item => item.DateTime).ToList();

            if (maxCount > 0)
            {
                foreach (var v in versionList)
                {
                    if (archiveNameList != null && archiveNameList.Contains(v.dlcVersion.versionName)) maxCount++;
                }

                for (int i = versionList.Count - 1; i >= 0; i--)
                {
                    if (versionList.Count <= maxCount) break;
                    var info = versionList[i];
                    if (archiveNameList != null && archiveNameList.Contains(info.dlcVersion.versionName)) continue;

                    var dir = $"{rootPath}/{info.dlcVersion.versionName}";
                    FileHelper.DeleteDirectory(dir);
                    versionList.RemoveAt(i);
                }
            }
            var dlcVersionList = new DLCBuilderVersionList();
            dlcVersionList.versions = versionList.ToArray();
            UnityJsonHelper.Save($"{rootPath}/{nameof(DLCBuilderVersionList)}.json", dlcVersionList, true);
        }
        
    }
}
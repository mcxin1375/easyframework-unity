using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class AssetBundleFileDepsInfo
    {
        public string File { get; }
        public List<string> DepList { get; } = new List<string>();
        public AssetBundleFileDepsInfo(string file)
        {
            File = file;
        }
    }
    public class AssetBundleBuildDepsDebug
    {
        public Dictionary<string, AssetBundleFileDepsInfo> DepFiles { get; } = new();

        public AssetBundleBuildDepsDebug(AssetBundleBuild[] assetBundleBuilds)
        {

            HashSet<string> abFileHash = new HashSet<string>();
            foreach (AssetBundleBuild build in assetBundleBuilds)
            {
                for (int i = 0; i < build.assetNames.Length; i++)
                {
                    var file = build.assetNames[i];
                    if (abFileHash.Contains(file)) continue;
                    abFileHash.Add(file.Replace("\\", "/"));
                }
            }

            float index = 1;
            foreach (var abFile in abFileHash)
            {
                var depGuids = AssetDatabase.GetDependencies(abFile);
                foreach (var depFile in depGuids)
                {
                    if (abFileHash.Contains(depFile)) continue;
                    if (depFile.EndsWith(".cs")) continue;
                    if (depFile.EndsWith(".dll")) continue;

                    if (!DepFiles.ContainsKey(depFile))
                    {
                        DepFiles.Add(depFile, new AssetBundleFileDepsInfo(depFile));
                    }

                    var info = DepFiles[depFile];
                    info.DepList.Add(abFile);
                }

                EditorUtility.DisplayProgressBar("Create AssetBundleDepsDebugInfo",
                    $"({index}/{abFileHash.Count}){abFile}", index++ / abFileHash.Count);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
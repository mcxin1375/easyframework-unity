using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class AssetBundleBuildFileTreeDebug
    {
        public string BuildPath => AssetBundleBuilder.Instance.ProjectDataPath;
        public string RootDirectory { get; }
        public long TotalSize { get; private set; }

        public Dictionary<string, AssetBundleBuildFileTreeDebug> ChildDict { get; } = new ();
        public Dictionary<string, long> ChildFiles { get; } = new Dictionary<string, long>();

        public AssetBundleBuildFileTreeDebug(AssetBundleBuild[] assetBundleBuilds)
        {
            // RootDirectory = rootDirectory;
            
            float index = 1;
            foreach (var assetBundleBuild in assetBundleBuilds)
            {
                if (assetBundleBuild.assetNames.Length > 0)
                {
                    var str = assetBundleBuild.assetNames[0].Replace("\\", "/");
                    var arr = str.Split("/");
                    string childName = arr[0];
                    AssetBundleBuildFileTreeDebug childDebug = GetOrCreateTreeInfo(childName);
                    for (int i = 1; i < arr.Length - 1; i++)
                    {
                        childName = $"{childName}/{arr[i]}";
                        childDebug = childDebug.GetOrCreateTreeInfo(childName);
                    }
                    childDebug.Add(assetBundleBuild);
                }
                
                EditorUtility.DisplayProgressBar("Create AssetBundleBuildTreeInfo", $"({index}/{assetBundleBuilds.Length}){assetBundleBuild.assetBundleName}", index++ / assetBundleBuilds.Length);
            }
            EditorUtility.ClearProgressBar();

            CalculateSize();
        }
        public AssetBundleBuildFileTreeDebug(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public void Add(AssetBundleBuild assetBundleBuild)
        {
            string fileName = assetBundleBuild.assetBundleName;
            string abFilePath = $"{BuildPath}/{fileName}";
            if (File.Exists(abFilePath))
            {
                var fileInfo = new FileInfo(abFilePath);
                ChildFiles.Add(assetBundleBuild.assetBundleName, fileInfo.Length);
            }
            else
            {
                ChildFiles.Add(assetBundleBuild.assetBundleName, 0);
            }
        }

        public AssetBundleBuildFileTreeDebug GetOrCreateTreeInfo(string name)
        {
            if (!ChildDict.ContainsKey(name))
            {
                ChildDict.Add(name, new AssetBundleBuildFileTreeDebug(name));
            }
            return ChildDict[name];
        }

        public void CalculateSize()
        {
            TotalSize = 0;
            foreach (var value in ChildDict.Values)
            {
                value.CalculateSize();
                TotalSize += value.TotalSize;
            }
            foreach (var v in ChildFiles.Values)
            {
                TotalSize += v;
            }
        }
    }
}
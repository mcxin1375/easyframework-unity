/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2020/7/3
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class AssetBundleBuilderHelper
    {
        
        public static AssetBundleBuild[] CreateAssetBundleBuildBySettings()
        {
            var settings = AssetBundleBuilderSettings.Instance;
            
            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
            
            var directories = settings.buildDirectories.ToHashSet();
            if (settings.Extensions?.Length > 0)
            {
                foreach (var create in settings.Extensions)
                {
                    if (create.BuildDirectories?.Length > 0)
                    {
                        foreach (var directory in create.BuildDirectories)
                            directories.Add(directory);
                    }
                    if (create.BuildInfos?.Length > 0)
                    {
                        foreach (var buildInfo in create.BuildInfos)
                        {
                            switch (buildInfo.abResType)
                            {
                                case EAssetBundleBuildResType.Shader:
                                    buildList.Add(AssetBundleBuilderHelper.CreateShaderDirectory(buildInfo.abName, buildInfo.directories));
                                    break;
                                default:
                                    buildList.Add(AssetBundleBuilderHelper.CreateDirectory(buildInfo.abName, buildInfo.directories));
                                    break;
                            }
                        }
                    }
                }
            }
            
            var arr = CreateAssetBundleBuildsByDirectories(directories.ToArray());
            if (arr?.Length > 0) buildList.AddRange(arr);

            Dictionary<string, AssetBundleBuild> dict = new Dictionary<string, AssetBundleBuild>();
            foreach (AssetBundleBuild ab in buildList)
            {
                if (dict.ContainsKey(ab.assetBundleName)) dict[ab.assetBundleName] = AssetBundleBuilderHelper.Merge(dict[ab.assetBundleName], ab);
                else dict.Add(ab.assetBundleName, ab);
            }
            
            return dict.Values.ToArray();
        }

        public static AssetBundleBuild[] CreateAssetBundleBuildsByDirectories(params string[] directories)
        {
            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
            List<string> folderList = new List<string>();
            List<string> fileList = new List<string>();
            
            var instance = AssetBundleBuilderSettings.Instance;
            var resFileIgnoreExs = instance.ignoreFileExes.ToHashSet();
            var resFileIgnoreNames = instance.ignoreFileNames.ToHashSet();
            
            void For(string path)
            {
                path = path.Replace("\\", "/");
                if (!Directory.Exists(path)) return;
                if (path.IsEditorPath()) return;
                
                string[] folders = Directory.GetDirectories(path).Where(file => !file.EndsWith(".meta")).ToArray();
                string[] files = Directory.GetFiles(path).Where(file => !file.EndsWith(".meta")).ToArray();

                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file);
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    
                    if (resFileIgnoreExs.Contains(extension) || resFileIgnoreNames.Contains(fileName)) continue;

                    fileList.Add(file.Replace("\\", "/"));
                }

                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    if (name == "Editor") continue;
                    if (name.EndsWith(EasyFrameworkConst.ABSuffix))
                    {
                        // folderList.Add(folder);
                        string newPath = folder.Replace('\\', '/');
                        if (!folderList.Contains(newPath)) folderList.Add(newPath);
                    }
                    else
                    {
                        For(folder);
                    }
                }
            }

            foreach (var directory in directories)
            {
                For(directory);
            }
            
            foreach (string folder in folderList)
            {
                buildList.Add(AssetBundleBuilderHelper.CreateDirectory(Path.GetFileNameWithoutExtension(folder), folder));
            }
            foreach (string file in fileList)
            {
                buildList.Add(AssetBundleBuilderHelper.CreateFile(Path.GetFileNameWithoutExtension(file), file));
            }

            return buildList.ToArray();
        }

        public static AssetBundleBuild CreateShaderDirectory(string abName, string directory) => CreateShaderDirectory(abName, new string[] { directory });
        public static AssetBundleBuild CreateShaderDirectory(string abName, string[] directories, string[] files = null)
        {
            List<string> tmpList = new List<string>();
            if (files?.Length > 0) tmpList.AddRange(files);
            string[] guids = AssetDatabase.FindAssets("t:Shader", directories);
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IsEditorPath()) continue;
                if (!path.Contains(".meta")) // 过滤掉元数据文件（如果需要）
                {
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    if (shader == null) continue;
                    tmpList.Add(path);
                }
            }
            return CreateFile(abName, tmpList.ToArray());
        }

        public static AssetBundleBuild CreateFile(string file) => CreateFile(Path.GetFileNameWithoutExtension(file), new string[] { file });
        public static AssetBundleBuild CreateFile(string abName, string file) => CreateFile(abName, new string[] { file });
        public static AssetBundleBuild CreateFile(string abName, string[] files) => CreateDirectory(abName, null, files);
        public static AssetBundleBuild CreateDirectory(string abName, string directory) => CreateDirectory(abName, new string[] { directory }, null);
        public static AssetBundleBuild CreateDirectory(string abName, string[] directories) => CreateDirectory(abName, directories, null);
        public static AssetBundleBuild CreateDirectory(string abName, string[] directories, string[] files)
        {
            List<string> assetNameList = new List<string>();
            void Add(string[] arr)
            {
                if (arr?.Length > 0)
                {
                    foreach (string s in arr) if (!assetNameList.Contains(s)) assetNameList.Add(s);
                }
            }

            Add(files);

            if (directories?.Length > 0)
            {
                foreach (string directory in directories)
                {
                    string[] arr = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Where(file => !file.EndsWith(".meta")).ToArray();
                    Add(arr);
                }
            }

            List<string> addressableNameList = new List<string>();
            // bool repeatAddressableName = false;
            for (int i = 0; i < assetNameList.Count; i++)
            {
                string addressableName = Path.GetFileNameWithoutExtension(assetNameList[i]);
                // if (addressableNameList.Contains(addressableName))
                // {
                //     repeatAddressableName = true;
                // }
                addressableNameList.Add(addressableName);
            }

            // if (repeatAddressableName)
            // {
            //     Log.Error($"---------------------------- Create AssetBundle {abName} AssetNames Repeated!");
            //     foreach (string s in fileList)
            //     {
            //         Log.Error(Path.GetFileNameWithoutExtension(s), s);
            //     }
            //     Log.Error($"----------------------------");
            // }

            return CreateAssetBundleBuild(abName, assetNameList.ToArray(), addressableNameList.ToArray());
        }

        public static AssetBundleBuild CreateAssetBundleBuild(string abName, string[] assetNames, string[] addressableNames)
        {
            if (string.IsNullOrWhiteSpace(abName))
            {
                Debug.LogError($"CreateAssetBundleBuild abName is null or whitespace");
                return default;
            }

            if (!abName.EndsWith(EasyFrameworkConst.ABSuffix))
            {
                abName = $"{abName}{EasyFrameworkConst.ABSuffix}";
            }

            return new AssetBundleBuild
            {
                assetBundleName = abName,
                assetBundleVariant = null,
                assetNames = assetNames,
                addressableNames = addressableNames
            };
        }

        public static AssetBundleBuild Merge(AssetBundleBuild ab, AssetBundleBuild ab2)
        {
            List<string> assetNameList = new List<string>();
            List<string> addressableNameList = new List<string>();
            
            foreach (string s in ab.assetNames) if (!assetNameList.Contains(s)) assetNameList.Add(s);
            foreach (string s in ab2.assetNames) if (!assetNameList.Contains(s)) assetNameList.Add(s);
            
            for (int i = 0; i < assetNameList.Count; i++)
            {
                string addressableName = Path.GetFileNameWithoutExtension(assetNameList[i]);
                addressableNameList.Add(addressableName);
            }
            
            // List<string> repeatNameList = assetNameList.GroupBy(x => x)
            //     .Where(x => x.Count() > 1)
            //     .Select(x => x.Key).ToList();
            // if (repeatNameList.Count > 0)
            // {
            //     Log.Error($"---------------------------- Merge AssetBundle {ab.assetBundleName} AssetNames Repeated!");
            //     foreach (string s in ab.assetNames)
            //     {
            //         Log.Error(Path.GetFileNameWithoutExtension(s), s);
            //     }
            //     Log.Error($"----------------------------");
            // }

            ab.assetNames = assetNameList.ToArray();
            ab.addressableNames = addressableNameList.ToArray();
            return ab;
        }
    }
}
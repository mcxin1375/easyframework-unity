/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/1/22
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public abstract class EditorTool<T, TEx> : EditorTool<T> where T : EditorTool<T, TEx>, new() where TEx : IEditorToolExtension
    {
        public readonly TEx[] Extensions = EasyFrameworkReflection.CreateInstances<TEx>().OrderBy(o => o.Order).ToArray();
    }

    public abstract class EditorTool<T> : Singleton<T> where T : EditorTool<T>, new()
    {
        public readonly string ProjectDataPath;
        public readonly string AssetsDataPath;
        public readonly string DebugPath;
        private readonly string VersionFilePath;

        public ToolVersion Version { get; private set; }

        protected EditorTool()
        {
            Version = UnityJsonHelper.LoadOrCreate<ToolVersion>(VersionFilePath);
            ProjectDataPath = $"{EasyFrameworkPreferences.ProjectDataPath}/{typeof(T).Name}/{PlatformHelper.PlatformName}";
            AssetsDataPath = $"{EasyFrameworkPreferences.AssetsDataPath}/{typeof(T).Name}/{PlatformHelper.PlatformName}";
            DebugPath = $"{EasyFrameworkPreferences.ProjectDataPath}/.ToolsDebug/{typeof(T).Name}/{PlatformHelper.PlatformName}";
            VersionFilePath = $"{EasyFrameworkPreferences.ProjectDataPath}/.ToolsVersion/{PlatformHelper.PlatformName}/{typeof(T).Name}.json";
        }

        protected void UpgradeVersion()
        {
            Version.version++;
            Version.dateTime = DateTime.Now.ToFileTime();
            SVNCommand.TryGetRevision(EasyFrameworkPreferences.ProjectFullPath, out Version.revision);
            UnityJsonHelper.Save(VersionFilePath, Version, true);
        }

        public string[] GetBuildFiles() => GetBuildFiles(EditorUserBuildSettings.activeBuildTarget.ToPlatform());
        public string[] GetBuildFiles(Platform platform)
        {
            var path = $"{Instance.ProjectDataPath}/{platform}";
            return Directory.Exists(path) ? Directory.GetFiles(path, "*", SearchOption.AllDirectories) : null;
        }

        public string GetProjectDataPath(Platform platform)
        {
            return $"{EasyFrameworkPreferences.ProjectDataPath}/{typeof(T).Name}/{platform.ToPlatformName()}";
        }
        public string GetAssetsDataPath(Platform platform)
        {
            return $"{EasyFrameworkPreferences.AssetsDataPath}/{typeof(T).Name}/{platform.ToPlatformName()}";
        }
    }
}
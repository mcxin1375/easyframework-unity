/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/1/22
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;
using UnityEditor;

namespace EasyFramework.Editor
{
    public abstract class ToolBase<T> : Singleton<T>, ITool, IToolEvent<T> where T : ToolBase<T>, new()
    {
        public virtual int Order => 0;

        public readonly string ProjectDataPath;
        public readonly string AssetsDataPath;
        public readonly string DebugPath;
        private readonly string VersionFilePath;

        public ToolVersion Version { get; private set; }
        public IToolEvent<T>[] ToolEvents => ToolExtension<IToolEvent<T>>.Instances;
        public IToolExtension[] Extension => ToolExtension<IToolEvent<T>>.Instances;

        protected ToolBase()
        {
            ProjectDataPath = $"{EasyFrameworkPreferences.ProjectDataPath}/{typeof(T).Name}/{PlatformHelper.PlatformName}";
            AssetsDataPath = $"{EasyFrameworkPreferences.AssetsDataPath}/{typeof(T).Name}/{PlatformHelper.PlatformName}";
            DebugPath = $"{EasyFrameworkPreferences.ProjectDataPath}/.ToolsDebug/{typeof(T).Name}/{PlatformHelper.PlatformName}";
            VersionFilePath = $"{EasyFrameworkPreferences.ProjectDataPath}/.ToolsVersion/{PlatformHelper.PlatformName}/{typeof(T).Name}.json";
            Version = UnityJsonHelper.LoadOrCreate<ToolVersion>(VersionFilePath);
        }
        
        protected void UpgradeVersion()
        {
            Version.buildIndex++;
            Version.dateTime = DateTime.Now.ToFileTime();
            // SVNCommand.TryGetRevision(EasyFrameworkPreferences.ProjectFullPath, out Version.revision);
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
        
        public void Execute()
        {
            Refresh();

            var timeDebug = FDebug.StartTime();
            FDebug.Log($"[{GetType().Name} - {typeof(T).Name}] Execute");

            foreach (var extension in ToolEvents) extension.OnExecuteBefore();
            UpgradeVersion();
            foreach (var extension in ToolEvents) extension.OnExecute();
            foreach (var extension in ToolEvents) extension.OnExecuteAfter();
            
            FDebug.Log($"[{GetType().Name} - {typeof(T).Name}] Execute Completed! Time: {timeDebug.StopToSeconds():hh:mm:nn}");
            
            AssetDatabase.Refresh();
        }
        
        public void Refresh()
        {
            ToolExtension<IToolEvent<T>>.Refresh();
        }

        void IToolEvent<T>.OnExecuteBefore() => OnToolExecuteBefore();
        void IToolEvent<T>.OnExecute() => OnToolExecute();
        void IToolEvent<T>.OnExecuteAfter() => OnToolExecuteAfter();
        protected virtual void OnToolExecuteBefore() { }
        protected virtual void OnToolExecute() { }
        protected virtual void OnToolExecuteAfter() { }
        
    }
}

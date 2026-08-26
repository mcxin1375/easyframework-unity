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
            Version = ConfigHelper.LoadOrCreate<ToolVersion>(VersionFilePath);
        }
        
        protected void UpgradeVersion()
        {
            Version.buildIndex++;
            Version.dateTime = DateTime.Now.ToFileTime();
            // SVNCommand.TryGetRevision(EasyFrameworkPreferences.ProjectFullPath, out Version.revision);
            ConfigHelper.Save(Version, VersionFilePath, true);
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
            var timeDebug = FDebug.StartTime();
            FDebug.Log($"[{GetType().Name} - {typeof(T).Name}] Execute");

            Refresh();
            foreach (var extension in ToolEvents) extension.OnExecuteBefore();
            UpgradeVersion();
            foreach (var extension in ToolEvents) extension.OnExecute();
            foreach (var extension in ToolEvents) extension.OnExecuteAfter();
            
            FDebug.Log($"[{GetType().Name} - {typeof(T).Name}] Execute Completed! Time: {TimeSpan.FromSeconds(timeDebug.StopToSeconds()):hh\\:mm\\:ss\\.fff}");
            
            AssetDatabase.Refresh();
        }
        
        public void Refresh()
        {
            ToolExtension<IToolEvent<T>>.Refresh();
        }

        public void OnExecute() => Execute();

        void IToolEvent<T>.OnExecuteBefore() => OnSelfExecuteBefore();
        void IToolEvent<T>.OnExecute() => OnSelfExecute();
        void IToolEvent<T>.OnExecuteAfter() => OnSelfExecuteAfter();
        protected virtual void OnSelfExecuteBefore() { }
        protected virtual void OnSelfExecute() { }
        protected virtual void OnSelfExecuteAfter() { }
        
    }
}

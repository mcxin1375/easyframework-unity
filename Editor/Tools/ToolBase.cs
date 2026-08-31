/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/1/22
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEditor;

namespace EasyFramework.Editor
{
    public abstract class ToolBase<T> : Singleton<T>, ITool, IToolEvent<T> where T : ToolBase<T>, new()
    {
        public virtual int Order => 0;

        public readonly string AssetsPath;
        public readonly string AssetsPlatformPath;
        
        public readonly string ProjectPath;
        public readonly string ToolsPath;
        public readonly string DebugPath;
        public readonly string ProjectPlatformPath;
        public readonly string ToolsPlatformPath;
        public readonly string DebugPlatformPath;
        
        private string VersionFilePath => $"{ToolsPlatformPath}/{ToolVersion.FileName}";

        public ToolVersion Version { get; private set; }
        public IToolEvent<T>[] ToolEvents => ToolExtension<IToolEvent<T>>.Instances;
        // public IToolExtension[] Extension => ToolExtension<IToolEvent<T>>.Instances;

        protected ToolBase()
        {
            AssetsPath = $"{EasyFrameworkPreferences.AssetsDataPath}/{typeof(T).Name}";
            
            ProjectPath = $"{EasyFrameworkPreferences.ProjectDataPath}/{typeof(T).Name}";
            ToolsPath = $"{EasyFrameworkPreferences.ProjectDataPath}/.Tools/{typeof(T).Name}";
            DebugPath = $"{EasyFrameworkPreferences.ProjectDataPath}/.Debug/{typeof(T).Name}";
            
            ProjectPlatformPath = $"{ProjectPath}/{PlatformHelper.PlatformName}";
            AssetsPlatformPath = $"{AssetsPath}/{PlatformHelper.PlatformName}";
            ToolsPlatformPath = $"{ToolsPath}/{PlatformHelper.PlatformName}";
            DebugPlatformPath = $"{DebugPath}/{PlatformHelper.PlatformName}";
            
            Version = ConfigHelper.LoadOrCreate<ToolVersion>(VersionFilePath);
        }
        
        protected void UpgradeVersion()
        {
            Version.buildIndex++;
            Version.dateTime = DateTime.Now.ToFileTime();
            // SVNCommand.TryGetRevision(EasyFrameworkPreferences.ProjectFullPath, out Version.revision);
            ConfigHelper.Save(Version, VersionFilePath, true);
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

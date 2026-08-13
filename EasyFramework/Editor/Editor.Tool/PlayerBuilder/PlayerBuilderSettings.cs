/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EasyFramework.Editor
{
    
    public interface IPlayerBuilderBuildResult
    {
        void OnResult(bool exportProject, BuildPlayerOptions buildPlayerOptions, BuildReport buildReport);
    }

    [Flags]
    public enum EStreamingAssetsMode
    {
        None = 0,
        AssetBundleBuilder = 1,
        HybridCLRBuilder = 2
    }
    
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class PlayerBuilderSettings : ProjectSettingsEditor<PlayerBuilderSettings>
    {
        [Header("Build Player")]
        public bool buildPlayer;
        public bool buildProject;
        public bool developmentBuild;
        
        [Header("Cleanup: *_DoNotShip")]
        public bool cleanupTempDir = true;
        
        [Header("StreamingAssets")] 
        public bool streamingAssetsEnabled;
        public EStreamingAssetsMode streamingAssetsMode = EStreamingAssetsMode.None;
        
        [Header("DLCRes Settings")]
        public bool dlcResEnabled;
        public string dlcVersion;
        public int maxZipSizeMb = 1024;
        public string[] dlcPackages;
        
        
        public IPlayerBuilderBuildResult[] BuildResultExtensions { get; } = EasyFrameworkReflection.CreateInstances<IPlayerBuilderBuildResult>();
    }
}
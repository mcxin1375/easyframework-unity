/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    // [Flags]
    // public enum EStreamingAssetsMode
    // {
    //     None = 0,
    //     AssetBundleBuilder = 1,
    //     HybridCLRBuilder = 2
    // }

    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class PlayerBuilderSettings : ProjectSettingsEditor<PlayerBuilderSettings>
    {
        [Header("Build Player")]
        public bool enabled;
        public bool exportAsGoogleAndroidProject;
        public bool developmentBuild;
        
        [Header("Cleanup: *_DoNotShip")]
        public bool cleanupTempDir = true;
        
        // [Header("StreamingAssets")] 
        // public bool streamingAssetsEnabled;
        // public EStreamingAssetsMode streamingAssetsMode = EStreamingAssetsMode.None;
        //
        // [Header("DLCRes Settings")]
        // public bool dlcResEnabled;
        // public string dlcVersion;
        // public int maxZipSizeMb = 1024;
        // public string[] dlcPackages;
    }
}
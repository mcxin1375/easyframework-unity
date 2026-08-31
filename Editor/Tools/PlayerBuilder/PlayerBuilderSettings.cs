/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Editor
{
    [ProjectSettings(ProjectSettingsAttribute.ETag.Editor)]
    public class PlayerBuilderSettings : ProjectSettings<PlayerBuilderSettings>
    {
        [Header("Base Settings")]
        public string companyName;
        public string productName;
        
        [Header("Pre Settings")]
        public bool preSettingsEnabled = true;
        
        [Header("StreamingAssets Settings")]
        public EStreamingAssetsOptions streamingAssetsOptions = EStreamingAssetsOptions.None;
        public string dlcVersion;
        
        [Header("Build Player")]
        public bool enabled = true;
        public bool exportAsGoogleAndroidProject;
        public bool developmentBuild;
        
        [Header("Cleanup: *_DoNotShip")]
        public bool cleanupTempDir = true;
        
        // [Header("DLCRes Settings")]
        // public bool dlcResEnabled;
        // public string dlcVersion;
        // public int maxZipSizeMb = 1024;
        // public string[] dlcPackages;
    }

    public enum EStreamingAssetsOptions
    {
        None,
        DLCList,
    }
}
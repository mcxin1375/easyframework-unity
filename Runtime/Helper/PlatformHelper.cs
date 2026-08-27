
using UnityEngine;

namespace EasyFramework
{
    public static class PlatformHelper
    {
        public static readonly string PlatformName;
        
        static PlatformHelper()
        {
#if UNITY_EDITOR
            PlatformName = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToPlatformName();
#else
            PlatformName = Application.platform.ToPlatformName();
#endif
        }

        public static Platform ToPlatform(this RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android: return Platform.Android;
                case RuntimePlatform.IPhonePlayer: return Platform.IOS;
                case RuntimePlatform.WebGLPlayer: return Platform.WebGL;
                case RuntimePlatform.WindowsPlayer: return Platform.Windows;
                default: return Platform.Unknown;
            }
        }
        
        public static string ToPlatformName(this RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android: return "Android";
                case RuntimePlatform.IPhonePlayer: return "IOS";
                case RuntimePlatform.WebGLPlayer: return "WebGL";
                case RuntimePlatform.WindowsPlayer: return "Windows";
                default: return "Unknown";
            }
        }
        
        public static string ToPlatformName(this Platform platform)
        {
            return platform.ToString();
        }

#if UNITY_EDITOR
        
        public static string ToPlatformName(this UnityEditor.BuildTarget target)
        {
            return target.ToPlatform().ToString();
        }
        
        public static Platform ToPlatform(this UnityEditor.BuildTarget target)
        {
            switch (target)
            {
                case UnityEditor.BuildTarget.Android: return Platform.Android;
                case UnityEditor.BuildTarget.iOS: return Platform.IOS;
                case UnityEditor.BuildTarget.WebGL: return Platform.WebGL;
                case UnityEditor.BuildTarget.StandaloneWindows: 
                case UnityEditor.BuildTarget.StandaloneWindows64: 
                    return Platform.Windows;
                default: return Platform.Unknown;
            }
        }
        
        public static UnityEditor.BuildTarget ToBuildTarget(this Platform platform)
        {
            switch (platform)
            {
                case Platform.Android: return UnityEditor.BuildTarget.Android;
                case Platform.IOS: return UnityEditor.BuildTarget.iOS;
                case Platform.WebGL: return UnityEditor.BuildTarget.WebGL;
                case Platform.Windows: return UnityEditor.BuildTarget.StandaloneWindows64;
                default: return UnityEditor.BuildTarget.NoTarget;
            }
        }
        
#endif
    }
}
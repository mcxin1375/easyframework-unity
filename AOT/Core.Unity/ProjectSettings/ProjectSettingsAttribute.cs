using System;

namespace EasyFramework
{
    public enum EProjectSettingsTag
    {
        Resources,
        AssetBundle,
        Editor
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProjectSettingsAttribute : Attribute
    {
        public readonly string BasePath;
        public ProjectSettingsAttribute(string basePath)
        {
            BasePath = basePath;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProjectSettingsTagAttribute : Attribute
    {
        public readonly EProjectSettingsTag SettingsTag;
        public ProjectSettingsTagAttribute(EProjectSettingsTag settingsTag)
        {
            SettingsTag = settingsTag;
        }
    }
}
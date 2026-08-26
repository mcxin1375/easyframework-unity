using System;

namespace EasyFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProjectSettingsAttribute : Attribute
    {
        public readonly ETag Tag;
        public readonly string FilePath;
        public ProjectSettingsAttribute(ETag tag)
        {
            Tag = tag;
            FilePath = string.Empty;
        }
        // public ProjectSettingsAttribute(string filePath)
        // {
        //     Tag = ETag.None;
        //     FilePath = filePath;
        // }
        
        public enum ETag
        {
            None,
            Resources,
            Editor,
        }
    }
}
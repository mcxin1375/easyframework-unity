using Unity.Profiling;

namespace EasyFramework.Profiler
{
    public class RecorderInfoAttribute : System.Attribute
    {
        public enum Category
        {
            Internal,
            Memory,
            Render
        }
        
        public readonly ProfilerCategory ProfilerCategory;
        public readonly string StatName;
        public readonly bool EditorOnly;

        public RecorderInfoAttribute(Category category, string statName, bool editorOnly = false)
        {
            switch (category)
            {
                case Category.Internal:
                    ProfilerCategory = ProfilerCategory.Internal;
                    break;
                case Category.Memory:
                    ProfilerCategory = ProfilerCategory.Memory;
                    break;
                case Category.Render:
                    ProfilerCategory = ProfilerCategory.Render;
                    break;
            }
            StatName = statName;
            EditorOnly = editorOnly;
        }
    }
}

namespace EasyFramework.Profiler
{
    public static class FProfiler
    {
        public static EasyFrameworkProfilerSettings Settings => EasyFrameworkProfilerSettings.Instance;
        
        public static ProfilerSystem ProfilerSystem => F.World.GetSystem<ProfilerSystem>();

        
        internal static void Initialize()
        {
            EasyFrameworkProfilerSettings.CreateInstance();
            F.World.CreateSystem<ProfilerSystem>();
        }
    }
}
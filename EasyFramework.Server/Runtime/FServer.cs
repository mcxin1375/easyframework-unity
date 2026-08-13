
namespace EasyFramework.Server
{
    public static class FServer
    {
        public static EasyFrameworkServerSettings Settings => EasyFrameworkServerSettings.Instance;
        
        public static SVCServerSystem SVCServerSystem { get; private set; }

        internal static void Initialize()
        {
            EasyFrameworkServerSettings.CreateInstance();
            
            F.World.CreateSystem(typeof(FServer).Assembly);
            SVCServerSystem = F.World.GetSystem<SVCServerSystem>();
        }
    }
}
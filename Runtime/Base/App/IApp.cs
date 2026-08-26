
namespace EasyFramework
{

    public interface IApp
    {
        string AppVersionFileUrl { get; }
        string DLCPlatformServerUrl { get; }
        
        string[] AppSymbols { get; }
    }
}
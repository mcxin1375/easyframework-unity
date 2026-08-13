
namespace EasyFramework
{

    public interface IApp
    {
        string CompanyName { get; }
        string AppName { get; }
        string ProductName { get; }
        string BundleIdentifier { get; }
        string AppVersionFileUrl { get; }
        string DLCPlatformServerUrl { get; }
        
        int MainVersion { get; }
        int Ver1 { get; }
        int Ver2 { get; }
        int Ver3 { get; }
        string BundleVersion { get; }
        
        string[] AppSymbols { get; }
    }
}
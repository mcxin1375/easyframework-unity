/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IAppSettings
    {
        string CompanyName { get; }
        string ProductName { get; }
        string AppName { get; }
        string BundleVersion { get; }
        string BundleIdentifier { get; }
        int BuildIndex { get; }
        string CdnURL { get; }
        string AppVersionURL { get; }
    }
}
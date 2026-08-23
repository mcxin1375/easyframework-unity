/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IAppSettings
    {
        string AppName { get; }
        string BundleVersion { get; }
        string CdnURL { get; }
        string AppVersionURL { get; }
    }
}
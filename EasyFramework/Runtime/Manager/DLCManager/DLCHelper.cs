/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public static class DLCHelper
    {

        public static readonly string DLCBuilderPlatformPath = $"{EasyFrameworkSettings.AppSettings.CdnURL}/{PlatformHelper.PlatformName}";

        
        public static string GetDLCVersionURL(string versionName)
        {
            return $"{DLCBuilderPlatformPath}/{versionName}/{DLCVersion.FileName}";
        }
        
        public static string GetDLCResListURL(string versionName)
        {
            return $"{DLCBuilderPlatformPath}/{versionName}/{EDLCModeOptions.DLC}";
        }
    }
}
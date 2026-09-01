/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    [CreateAssetMenu(menuName = "EasyFramework/AppSettings", fileName = "AppSettings.asset")]
    public class AppSettings : ScriptableObject, IAppSettings
    {
        public string AppName => appName;
        public string BundleVersion => $"{ver1}.{ver2}.{ver3}";
        public string BundleIdentifier => bundleIdentifier;
        public int BuildIndex => buildIndex;
        public string CdnURL => dlcURL;
        public string AppVersionURL => appVersionURL.Replace("{Platform}", PlatformHelper.PlatformName);

        public string appName = "MainApp";
        public string bundleIdentifier = "cn.cookie.easyframework";
        public int ver1;
        public int ver2;
        public int ver3 = 1;
        public int buildIndex;
        public string dlcURL = "";
        public string appVersionURL = "";
    }
}
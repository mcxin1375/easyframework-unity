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
        public string AppVersionURL => appVersionURL;

        [SerializeField] private string appName = "MainApp";
        [SerializeField] private string bundleIdentifier = "cn.cookie.easyframework";
        [SerializeField] private int ver1;
        [SerializeField] private int ver2;
        [SerializeField] private int ver3 = 1;
        [SerializeField] private int buildIndex;
        [SerializeField] private string dlcURL = "";
        [SerializeField] private string appVersionURL = "";
    }
}
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
        public string CdnURL => dlcURL;
        public string AppVersionURL => appVersionURL;

        [SerializeField] private string appName = "EasyFramework";
        [SerializeField] private int ver1;
        [SerializeField] private int ver2;
        [SerializeField] private int ver3 = 1;
        [SerializeField] private string dlcURL = "";
        [SerializeField] private string appVersionURL = "";
    }
}
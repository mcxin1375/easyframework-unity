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
        public string CompanyName
        {
            get => companyName;
#if UNITY_EDITOR
            set => companyName = value;
#endif
        }
        public string ProductName
        {
            get => productName;
#if UNITY_EDITOR
            set => productName = value;
#endif
        }
        public string AppName
        {
            get => appName;
#if UNITY_EDITOR
            set => appName = value;
#endif
        }
        public string BundleVersion
        {
            get => $"{ver1}.{ver2}.{ver3}";
#if UNITY_EDITOR
            set
            {
                var arr = value.Split('.');
                ver1 = int.Parse(arr[0]);
                ver2 = int.Parse(arr[1]);
                ver3 = int.Parse(arr[2]);
            }
#endif
        }
        public string BundleIdentifier
        {
            get => bundleIdentifier;
#if UNITY_EDITOR
            set => bundleIdentifier = value;
#endif
        }
        public int BuildIndex
        {
            get => buildIndex;
#if UNITY_EDITOR
            set => buildIndex = value;
#endif
        }
        public string CdnURL
        {
            get => dlcURL;
#if UNITY_EDITOR
            set => dlcURL = value;
#endif
        }
        public string AppVersionURL
        {
            get => appVersionURL.Replace("{Platform}", PlatformHelper.PlatformName);
#if UNITY_EDITOR
            set => appVersionURL = value;
#endif
        }

        [SerializeField] private string companyName = "companyName";
        [SerializeField] private string productName = "productName";
        [SerializeField] private string appName = "appName";
        [SerializeField] private string bundleIdentifier = "com.companyName.productName";
        [SerializeField] private int ver1;
        [SerializeField] private int ver2;
        [SerializeField] private int ver3 = 1;
        [SerializeField] private int buildIndex;
        [SerializeField] private string dlcURL = "";
        [SerializeField] private string appVersionURL = "";
    }
}
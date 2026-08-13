/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/1/5
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Server
{
    public class EasyFrameworkServerSettings : ProjectSettingsAssetBundle<EasyFrameworkServerSettings>
    {
        public string UploadProjectConfig => $"{serverUrl}/uploadProjectConfig";
        public string UploadSVCError => $"{serverUrl}/uploadSVCError";
        public string GetSVCErrorConfig => $"{serverUrl}/getSVCErrorConfig";
        public string ClearSVCErrorConfig => $"{serverUrl}/clearSVCErrorConfig";
        public string UploadDLCBefore => $"{serverUrl}/uploadDLCBefore";
        public string UploadDLC => $"{serverUrl}/uploadDLC";
        
        
        public string serverUrl = "https://client.nineland.cn:8443/RG";
        
        [Header("变体日志上传")]
        public bool svcServerSystem = true;
        
    }
}
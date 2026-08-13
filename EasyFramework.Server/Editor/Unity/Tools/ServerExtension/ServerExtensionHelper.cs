
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EasyFramework.Editor;
using EasyFramework.AOT;

namespace EasyFramework.Server.Editor
{
    public static class ServerExtensionHelper
    {
        public static HttpContent CreateUploadProjectConfigContent(ProjectUnityConfig projectConfig)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(projectConfig.ToJsonEx(), Encoding.UTF8, "application/json"), "projectConfig");
            return content;
        }
        
        public static HttpContent CreateDLCUploadBeforeContent(string platformName, string versionName, ResFileInfo[] resFileInfos)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(platformName), "platform");
            content.Add(new StringContent(versionName), "versionName");
            content.Add(new StringContent(resFileInfos.ToJsonEx(), Encoding.UTF8, "application/json"), "resFileInfos");
            return content;
        }

        public static HttpContent CreateDLCUploadFileContent(string uploadFile, string uploadRelativeFile, string platformName, string versionName, bool isLast)
        {
            var fileInfo = new FileInfo(uploadFile);
            var uploadContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(File.ReadAllBytes(uploadFile));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            uploadContent.Add(fileContent, "files", Path.GetFileName(uploadFile));
            uploadContent.Add(new StringContent(uploadRelativeFile), "paths");
            uploadContent.Add(new StringContent(fileInfo.LastWriteTime.ToFileTime().ToString()), "writeTimes");
            uploadContent.Add(new StringContent(platformName), "platform");
            uploadContent.Add(new StringContent(versionName), "versionName");
            uploadContent.Add(new StringContent(isLast ? "true" : "false"), "isLast");
            return uploadContent;
        }

        public static HttpClientHandler CreateCustomValidationHandler()
        {
            return new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        }
        // class AcceptAllCertificates : CertificateHandler
        // {
        //     protected override bool ValidateCertificate(byte[] certData) => true;
        // }
    }
}
/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/1/5
// describe:
//----------------------------------------------------------------*/

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyFramework.Server
{
    public static class ServerAPI
    {
        public static async Task<HttpResponseMessage> UploadSVCErrorAsync(string svcError)
        {
            using HttpClient http = new HttpClient(CreateCustomValidationHandler());
            http.Timeout = TimeSpan.FromSeconds(10);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(svcError), "svcError");
            return await http.PostAsync(EasyFrameworkServerSettings.Instance.UploadSVCError, content);
        }
        
        public static HttpClientHandler CreateCustomValidationHandler()
        {
            return new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        }
    }
}
/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/4/25
// describe:
//----------------------------------------------------------------*/

using System.Net.Http;
using System.Text;
using System.Threading;

namespace EasyFramework
{
    public static class HttpClientEx
    {
        public static ETask<string> PostReadAsStringAsync(this HttpClient httpClient, string url, string body, CancellationToken cancellationToken = default)
        {
            return PostReadAsStringAsync(httpClient, url, new StringContent(body, Encoding.UTF8), cancellationToken);
        }

        public static async ETask<string> PostReadAsStringAsync(this HttpClient httpClient, string url, StringContent stringContent, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsync(url, stringContent, cancellationToken);
            response.EnsureSuccessStatusCode(); // 确保请求成功
            return await response.Content.ReadAsStringAsync(); // 读取响应内容
        }
        
        public static async ETask<long> GetContentLengthAsync(this HttpClient httpClient, string url, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentLength ?? 0;
        }
    }
}
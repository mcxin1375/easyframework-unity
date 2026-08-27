/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/4/25
// describe:
//----------------------------------------------------------------*/

using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace EasyFramework
{
    public interface IHttpReceiver
    {
        void OnProgress(int requestIndex, ulong bytesReceived);
    }
    
    public static class HttpClientExtensions
    {
        private const int BufferSize = 256 * 1024;
        
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
        
        
        public static async ETask<bool> DownloadAsync(this HttpClient httpClient, string url, string file, int requestIndex = -1, IHttpReceiver httpReceiver = null, CancellationToken token = default)
        {
            string directory = Path.GetDirectoryName(file);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            if (File.Exists(file)) File.Delete(file);
            
            byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
                response.EnsureSuccessStatusCode();
                var contentLength = response.Content.Headers.ContentLength ?? 0;
                
                await using Stream contentStream = await response.Content.ReadAsStreamAsync(),
                    fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize);

                int bytesRead;
                ulong bytesReceived = 0;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                    bytesReceived += (ulong)bytesRead;

                    httpReceiver?.OnProgress(requestIndex, bytesReceived);
                }

                return true;
            }
            catch (Exception ex)
            {
                FDebug.LogException(ex);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            return false;
        }
    }
}

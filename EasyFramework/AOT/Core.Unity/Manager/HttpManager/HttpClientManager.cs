// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Buffers;
// using System.IO;
// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
// using UnityEngine;
//
// namespace EasyFramework
// {
//     public class HttpClientManager : Singleton<HttpClientManager>, IHttpManager
//     {
//         private const int BufferSize = 256 * 1024;
//         private readonly HttpClient _client = new ();
//         
//         public async Task<bool> DownloadAsync(string url, string file, CancellationToken cancellationToken = default)
//         {
//             string directory = Path.GetDirectoryName(file);
//             if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
//             if (File.Exists(file)) File.Delete(file);
//
//             byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
//             
//             try
//             {
//                 Debug.Log($"Download: {url}");
//                 var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
//                 response.EnsureSuccessStatusCode();
//                 
//                 await using Stream contentStream = await response.Content.ReadAsStreamAsync(),
//                     fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize);
//
//                 int bytesRead;
//                 while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
//                 {
//                     await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
//                 }
//                 
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError(ex);
//             }
//             finally
//             {
//                 ArrayPool<byte>.Shared.Return(buffer);
//             }
//             return false;
//         }
//         
//         
//         public async Task<bool> DownloadAsync(string url, string file, Action<string, long> progressAction, CancellationToken token = default)
//         {
//             if (File.Exists(file)) File.Delete(file);
//             
//             byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
//             try
//             {
//                 Debug.Log($"Download: {url}");
//                 var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
//                 response.EnsureSuccessStatusCode();
//                 
//                 await using Stream contentStream = await response.Content.ReadAsStreamAsync(),
//                     fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize);
//
//                 int bytesRead;
//                 long downloadedBytes = 0;
//                 while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
//                 {
//                     await fileStream.WriteAsync(buffer, 0, bytesRead, token);
//                     downloadedBytes += bytesRead;
//
//                     progressAction?.Invoke(file, downloadedBytes);
//                 }
//
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError(ex);
//             }
//             finally
//             {
//                 ArrayPool<byte>.Shared.Return(buffer);
//             }
//             return false;
//         }
//         
//         public async Task<long> GetContentLengthAsync(string url, CancellationToken cancellationToken = default)
//         {
//             try
//             {
//                 var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
//                 response.EnsureSuccessStatusCode();
//                 return response.Content.Headers.ContentLength ?? 0;
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogError(
//                     $"Http Request Timeout.\n" +
//                     $"Url: {url}");
//             }
//             catch (HttpRequestException ex)
//             {
//                 Debug.LogError(
//                     $"Http Request Failed.\n" +
//                     $"Url: {url}\n" +
//                     $"{ex}");
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError(
//                     $"DeserializeObjectAsync Exception.\n" +
//                     $"Url: {url}\n" +
//                     $"{ex}");
//             }
//             
//             return 0;
//         }
//         
//     }
// }
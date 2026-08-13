// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/11/28
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.IO;
// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
// using UnityEngine;
//
// namespace EasyFramework
// {
//     public class HttpClientDownloadRequest
//     {
//         public string DownloadUrl { get; }
//         public string DownloadFile { get; }
//         public string DownloadMd5 { get; }
//         public long DownloadedBytes { get; internal set; }
//         public long TotalBytes { get; internal set; }
//         
//         public HttpClientDownloadRequest(string downloadUrl, string downloadFile, string downloadMd5, long totalBytes)
//         {
//             DownloadUrl = downloadUrl;
//             DownloadFile = downloadFile;
//             DownloadMd5 = downloadMd5;
//             TotalBytes = totalBytes;
//             DownloadedBytes = 0;
//         }
//     }
//     
//     public class HttpClientDownloader : IDisposable
//     {
//         public HttpClientDownloadRequest Request { get; private set; }
//         
//         private const int BufferSize = 8192 * 128;
//         private byte[] _buffer = new byte[BufferSize];
//         private HttpClient _client = new HttpClient();
//
//         public async Task<bool> DownloadAsync(HttpClientDownloadRequest request, CancellationToken cancellationToken)
//         {
//             if (Request != null)
//             {
//                 return false;
//                 // throw new Exception("DLCDownloaderEx is downloading");
//             }
//
//             Request = request;
//             Request.DownloadedBytes = 0;
//             
//             try
//             {
//                 return await Task.Run(async () =>
//                 {
//                     var response = await _client.GetAsync(Request.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
//                     response.EnsureSuccessStatusCode();
//
//                     Request.DownloadedBytes = 0;
//                     Request.TotalBytes = response.Content.Headers.ContentLength ?? -1L;
//
//                     await using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
//                                  fileStream = new FileStream(Request.DownloadFile, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize))
//                     {
//                         int bytesRead;
//                         while ((bytesRead = await contentStream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken)) > 0)
//                         {
//                             await fileStream.WriteAsync(_buffer, 0, bytesRead, cancellationToken);
//                             Request.DownloadedBytes += bytesRead;
//                         }
//                     }
//                     Request = null;
//                     return true;
//                 }, cancellationToken);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogWarning($"DownloadUrl: {request.DownloadUrl} Error: {e}");
//                 Request = null;
//                 return false;
//             }
//         }
//         
//         public void Dispose()
//         {
//             _client.Dispose();
//             _client = null;
//             _buffer = null;
//             Request = null;
//         }
//     }
// }
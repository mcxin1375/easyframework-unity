/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyFramework.Editor
{
    public static class DingTalkHelper
    {
        public static async Task SendMarkdownMessageAsync(string webhook, string secret, string messageTitle, string message)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string sign = GenerateSign(timestamp, secret);
            string fullUrl = $"{webhook}&timestamp={timestamp}&sign={sign}";
            
            message = message.Replace("\r\n", "<br/>")
                .Replace("\n", "<br/>")
                .Replace("\r", "<br/>");
            
            var jsonObject = new
            {
                msgtype = "markdown",
                markdown = new
                {
                    title = messageTitle,
                    text = message,
                }
            };
            string sendMessage = JsonConvert.SerializeObject(jsonObject);
            await SendMessageAsync(fullUrl, sendMessage);
        }
        
        public static async Task SendLinkMessageAsync(string webhook, string secret, string linkTitle, string linkText, string linkUrl)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string sign = GenerateSign(timestamp, secret);
            string fullUrl = $"{webhook}&timestamp={timestamp}&sign={sign}";
            
            var jsonObject = new
            {
                msgtype = "link",
                link = new
                {
                    text = linkText,
                    title = linkTitle,
                    messageUrl = linkUrl,
                }
            };
            string sendMessage = JsonConvert.SerializeObject(jsonObject);
            await SendMessageAsync(fullUrl, sendMessage);
        }
        
        public static async Task SendTextMessageAsync(string webhook, string secret, string message)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string sign = GenerateSign(timestamp, secret);
            string fullUrl = $"{webhook}&timestamp={timestamp}&sign={sign}";
            
            var jsonObject = new
            {
                msgtype = "text",
                text = new
                {
                    content = message
                }
            };
            string sendMessage = JsonConvert.SerializeObject(jsonObject);
            await SendMessageAsync(fullUrl, sendMessage);
        }

        public static async Task<HttpResponseMessage> SendMessageAsync(string url, string message)
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent stringContent = new StringContent(message, Encoding.UTF8, "application/json");
                return await client.PostAsync(url, stringContent);
            }
        }

        public static string GenerateSign(long timestamp, string secret)
        {
            string stringToSign = $"{timestamp}\n{secret}";
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] signData = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                string sign = Convert.ToBase64String(signData);
                return Uri.EscapeDataString(sign);
            }
        }

        public static string CreateLinkMessage(string linkUrl, string linkText)
        {
            return $"<a href=\"{linkUrl}\">{linkText}</a>";
        }

    }
}
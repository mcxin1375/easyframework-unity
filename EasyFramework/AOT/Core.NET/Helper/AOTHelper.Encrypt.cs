/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EasyFramework
{
    public static partial class AOTHelper
    {
        public static byte[] EncryptAES(byte[] data, string secretKey)
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var iv = new byte[16]; // 使用默认的 IV（Initialization Vector）
            return EncryptAES(data, key, iv);
        }
        
        public static byte[] EncryptAES(byte[] data, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();
            return memoryStream.ToArray();
        }

        public static void EncryptAES(Stream inputStream, Stream outputStream, string secretKey)
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var iv = new byte[16]; // 使用默认的 IV（Initialization Vector）
            EncryptAES(inputStream, outputStream, key, iv);
        }

        public static void EncryptAES(Stream inputStream, Stream outputStream, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] buffer = new byte[8192]; // 缓冲区大小
            int bytesRead;
            while ((bytesRead = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
            }
        }
        
        public static string EncryptAES(string plainText, string secretKey)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(secretKey);
            aes.IV = new byte[16]; // 使用默认的 IV（Initialization Vector）

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public static byte[] DecryptAES(byte[] encryptedData, string secretKey)
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var iv = new byte[16]; // 使用默认的 IV（Initialization Vector）
            return DecryptAES(encryptedData, key, iv);
        }
        
        public static byte[] DecryptAES(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var memoryStream = new MemoryStream(encryptedData);
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var outputStream = new MemoryStream();
            cryptoStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }

        public static void DecryptAES(Stream inputStream, Stream outputStream, string secretKey)
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var iv = new byte[16]; // 使用默认的 IV（Initialization Vector）
            DecryptAES(inputStream, outputStream, key, iv);
        }

        public static void DecryptAES(Stream inputStream, Stream outputStream, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            byte[] buffer = new byte[8192]; // 缓冲区大小
            int bytesRead;
            while ((bytesRead = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
            }
        }

        public static string DecryptAES(string cipherText, string secretKey)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(secretKey);
            aes.IV = new byte[16]; // 使用默认的 IV（Initialization Vector）

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using var msDecrypt = new MemoryStream(cipherBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }

        public static string EncryptDES(string plainText, string secretKey)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] ivBytes = new byte[8]; // 使用默认的 IV（Initialization Vector）

            using DES des = DES.Create();
            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(keyBytes, ivBytes), CryptoStreamMode.Write);
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(inputBytes, 0, inputBytes.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        public static byte[] EncryptDES(byte[] data, string secretKey)
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var iv = new byte[8]; // 使用默认的 IV（Initialization Vector）
            return EncryptDES(data, key, iv);
        }
        
        public static byte[] EncryptDES(byte[] data, byte[] key, byte[] iv)
        {
            using var des = DES.Create();
            des.Key = key;
            des.IV = iv;

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();
            return memoryStream.ToArray();
        }

        public static string DecryptDES(string cipherText, string secretKey)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] ivBytes = new byte[8]; // 使用默认的 IV（Initialization Vector）

            using DES des = DES.Create();
            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(keyBytes, ivBytes), CryptoStreamMode.Write);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            cs.Write(cipherBytes, 0, cipherBytes.Length);
            cs.FlushFinalBlock();
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static byte[] DecryptDES(byte[] data, string secretKey)
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var iv = new byte[8]; // 使用默认的 IV（Initialization Vector）
            return DecryptDES(data, key, iv);
        }
        
        public static byte[] DecryptDES(byte[] data, byte[] key, byte[] iv)
        {
            using var des = DES.Create();
            des.Key = key;
            des.IV = iv;

            using var memoryStream = new MemoryStream(data);
            using var cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Read);
            using var outputStream = new MemoryStream();
            cryptoStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
        
        public static void EncryptXOR(ref byte[] bytes, byte secretKey)
        {
            for (int j = 0; j < bytes.Length; j++)
            {
                bytes[j] ^= secretKey;
            }
        }
        
    }
}
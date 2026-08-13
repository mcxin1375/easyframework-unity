using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace EasyFramework
{
    public static class HashHelper
    {
        private const int BufferSize = 64 * 1024;

        
        // =========================
        // FILE MD5
        // =========================
        public static string MD5File(string filePath)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            try
            {
                using var md5 = System.Security.Cryptography.MD5.Create();
                using var fs = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    BufferSize,
                    FileOptions.SequentialScan);

                int read;

                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    md5.TransformBlock(buffer, 0, read, null, 0);
                }

                md5.TransformFinalBlock(buffer, 0, 0);

                return ConvertToHex(md5.Hash);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        // =========================
        // OBJECT MD5
        // ⚠️ 注意：必须统一序列化，否则不同机器结果不同
        // =========================
        // public static string MD5(object obj)
        // {
        //     if (obj == null)
        //         return string.Empty;
        //
        //     // 简单稳定策略：ToString + UTF8
        //     // ⚠️如果是热更系统建议换成 JSON/二进制序列化
        //     string str = obj.ToString();
        //
        //     return MD5(str);
        // }

        // =========================
        // BYTE[] MD5
        // =========================
        public static string MD5(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            using var md5 = System.Security.Cryptography.MD5.Create();

            byte[] hash = md5.ComputeHash(data);

            return ConvertToHex(hash);
        }

        // =========================
        // STRING MD5（建议补充）
        // =========================
        public static string MD5String(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            using var md5 = System.Security.Cryptography.MD5.Create();

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] hash = md5.ComputeHash(bytes);

            return ConvertToHex(hash);
        }

        // =========================
        // HEX转换（无额外string拼接GC优化版）
        // =========================
        private static string ConvertToHex(byte[] hash)
        {
            char[] c = new char[hash.Length * 2];

            int i = 0;

            for (int j = 0; j < hash.Length; j++)
            {
                byte b = hash[j];

                c[i++] = GetHexValue(b >> 4);
                c[i++] = GetHexValue(b & 0xF);
            }

            return new string(c);
        }

        private static char GetHexValue(int i)
        {
            return (char)(i < 10 ? (i + '0') : (i - 10 + 'a'));
        }

    }
}
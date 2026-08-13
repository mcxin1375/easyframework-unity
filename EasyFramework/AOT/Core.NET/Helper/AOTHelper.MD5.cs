using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace EasyFramework
{
    public static partial class AOTHelper
    {
        public static string MD5File(string file)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(file);
            var hashBytes = md5.ComputeHash(stream);
            return Encode(hashBytes);
        }
        public static string MD5String(string str) => MD5Bytes(Encoding.UTF8.GetBytes(str));
        public static string MD5Bytes(byte[] bytes)
        {
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(bytes);
            return Encode(hashBytes);
        }

        public static bool MD5FileEqual(string file1, string file2)
        {
            if (!File.Exists(file1) || !File.Exists(file2)) return true;
            return MD5File(file1) == MD5File(file2);
        }
        
        private static readonly char[] _lookup = CreateLookup();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Encode(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            // 租用 char 数组，避免频繁分配
            int len = bytes.Length * 2;
            char[] buffer = ArrayPool<char>.Shared.Rent(len);

            try
            {
                EncodeToSpan(bytes, buffer.AsSpan(0, len));
                return new string(buffer, 0, len); // 返回只读字符串
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeToSpan(ReadOnlySpan<byte> bytes, Span<char> output)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                output[i * 2] = _lookup[b >> 4];
                output[i * 2 + 1] = _lookup[b & 0xF];
            }
        }

        private static char[] CreateLookup()
        {
            char[] table = new char[16];
            for (int i = 0; i < 10; i++) table[i] = (char)('0' + i);
            for (int i = 10; i < 16; i++) table[i] = (char)('A' + (i - 10));
            return table;
        }
    }
}